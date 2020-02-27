using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Compiler.CodeGeneration2.Builders;
using Compiler.CodeGeneration2.Exceptions;
using Compiler.Parser.Nodes;
using Compiler.Parser.Nodes.Statements;
using Compiler.Tokenizer.Tokens;
using Compiler.TypeChecker;

namespace Compiler.CodeGeneration2
{
    public class CurrentMethodInfo
    {
        public IType ReturnType { get; }

        public IType Type { get; }

        public bool IsStatic { get; }

        public Dictionary<IType, ILocalBuilder> RefStoreLocals { get; } = new Dictionary<IType, ILocalBuilder>();

        public Dictionary<string, ILocalBuilder> Locals { get; } = new Dictionary<string, ILocalBuilder>();

        public IReadOnlyDictionary<string, (short idx, IType type)> Parameters { get; }

        public IReadOnlyDictionary<string, IFieldInfo> Fields { get; }

        public CurrentMethodInfo(IType type, IType returnType, bool isStatic,
            IReadOnlyDictionary<string, (short idx, IType type)> parameters,
            IReadOnlyDictionary<string, IFieldInfo> fields)
        {
            Type = type;
            ReturnType = returnType;
            IsStatic = isStatic;
            Parameters = parameters;
            Fields = fields;
        }
    }

    public class ILGeneration
    {
        private readonly CodeGenerationStore store;
        private readonly IILGenerator generator;
        private readonly CurrentMethodInfo currentMethodInfo;
        private readonly IType[] delegateConstructorTypes;
        private readonly IConstructorInfo baseConstructorCall;
        private readonly SimpleTypeChecker typeChecker;

        public ILGeneration(IILGenerator generator, CodeGenerationStore store, CurrentMethodInfo currentMethodInfo,
            IType[] delegateConstructorTypes, IConstructorInfo baseConstructorCall, SimpleTypeChecker typeChecker)
        {
            this.generator = generator;
            this.store = store;
            this.currentMethodInfo = currentMethodInfo;
            this.delegateConstructorTypes = delegateConstructorTypes;
            this.baseConstructorCall = baseConstructorCall;
            this.typeChecker = typeChecker;
        }

        // x = a + b

        private Action WriteLValueExpression(ExpressionSyntaxNode expression, out IType? expressionResultType)
        {
            switch (expression)
            {
                case ArrayIndexExpression arrIdx:
                    IType? arrayType = null;
                    IType? lengthType = null;

                    WriteExpression(arrIdx.Expression, true, false, ref arrayType);
                    WriteExpression(arrIdx.LengthExpression, true, false, ref lengthType);

                    typeChecker.TypeCheck(store.Types["System.Int32"], lengthType);

                    if (arrayType == null)
                    {
                        throw new MissingTypeException("Array type must have been found");
                    }

                    // Use the runtime provided Get() method to determine the inner type of the array
                    var arrayRootType = arrayType.GetMethod("Get").ReturnType;

                    // The expression needs the inner array type, then will call a Stelem
                    expressionResultType = arrayRootType;
                    return () => generator.EmitStelem(arrayRootType);
                case VariableSyntaxNode varNode:
                    if (currentMethodInfo.Locals.TryGetValue(varNode.Name, out var localVar))
                    {
                        expressionResultType = localVar.LocalType;
                        return () => generator.EmitStloc(localVar);
                    }
                    else if (currentMethodInfo.Parameters.TryGetValue(varNode.Name, out var parameterVar))
                    {
                        expressionResultType = parameterVar.type;
                        return () => generator.EmitStarg(parameterVar.idx);
                    }
                    else if (currentMethodInfo.Fields.TryGetValue(varNode.Name, out var fieldVar))
                    {
                        generator.EmitLdthis();
                        expressionResultType = fieldVar.FieldType;
                        return () => generator.EmitStfld(fieldVar);
                    }
                    else
                    {
                        throw new MissingVariableDefinitionException("Not supported");
                    }
                case VariableAccessExpression varAccess:
                    {
                        IType? callTarget = null;
                        WriteExpression(varAccess.Expression, true, false, ref callTarget);

                        if (callTarget == null)
                        {
                            throw new MissingTypeException("No target for field access");
                        }

                        IFieldInfo? fieldToCall = null;

                        if (store.Fields.TryGetValue(callTarget, out var typeFieldList))
                        {
                            foreach (var localField in typeFieldList)
                            {
                                if (localField.Name == varAccess.Name)
                                {
                                    fieldToCall = localField;
                                    break;
                                }
                            }
                        }

                        if (fieldToCall == null)
                        {
                            throw new MissingTypeException("Field not found");
                        }

                        expressionResultType = fieldToCall.FieldType;
                        return () => generator.EmitStfld(fieldToCall);
                    }
                default:
                    throw new LValueOperationException("No other type of operations supported as lvalue");
            }
        }

        private IType WriteCallParameter(CallParameterSyntaxNode callNode)
        {
            if (callNode == null)
            {
                throw new ArgumentNullException(nameof(callNode));
            }

            IType? expressionResultType = null;
            WriteExpression(callNode.Expression, true, false, ref expressionResultType);
            if (expressionResultType == null)
            {
                throw new InvalidMethodParameterException("Method call parameter expression cannot return void");
            }
            return expressionResultType;
        }

        private void HandleVariableExpression(VariableSyntaxNode varNode, bool isRight, bool willBeMethodCall, ref IType? expressionResultType)
        {
            {
                if (!isRight && willBeMethodCall)
                {
                    throw new InvalidLValueException("Cannot have a method call on the left");
                }


                if (currentMethodInfo.Locals.TryGetValue(varNode.Name, out var localVar))
                {
                    if (isRight)
                    {
                        if (willBeMethodCall && localVar.LocalType.IsValueType)
                        {
                            generator.EmitLdloca(localVar);
                        }
                        else
                        {
                            generator.EmitLdloc(localVar);
                        }
                    }
                    else
                    {
                        generator.EmitStloc(localVar);
                    }
                    expressionResultType = localVar.LocalType;
                }
                else if (currentMethodInfo.Parameters.TryGetValue(varNode.Name, out var parameterVar))
                {
                    if (isRight)
                    {
                        if (willBeMethodCall && parameterVar.type.IsValueType)
                        {
                            generator.EmitLdarga(parameterVar.idx);
                        }
                        else
                        {
                            generator.EmitLdarg(parameterVar.idx);
                        }
                    }
                    else
                    {
                        generator.EmitStarg(parameterVar.idx);
                    }
                    expressionResultType = parameterVar.type;
                }
                else if (currentMethodInfo.Fields.TryGetValue(varNode.Name, out var fieldVar))
                {
                    if (fieldVar.IsStatic)
                    {
                        // If field is static, it can always be accessed
                        if (isRight)
                        {
                            if (willBeMethodCall && fieldVar.FieldType.IsValueType)
                            {
                                generator.EmitLdsflda(fieldVar);
                            }
                            else
                            {
                                generator.EmitLdsfld(fieldVar);
                            }


                        }
                        else
                        {
                            generator.EmitStsfld(fieldVar);
                        }
                    }
                    else
                    {

                        if (currentMethodInfo.IsStatic)
                        {
                            throw new InstanceFieldAccessException("Invalid to access instance field in static method");
                        }

                        if (isRight)
                        {
                            generator.EmitLdthis();
                            if (willBeMethodCall && fieldVar.FieldType.IsValueType)
                            {
                                generator.EmitLdflda(fieldVar);
                            }
                            else
                            {
                                generator.EmitLdfld(fieldVar);
                            }


                        }
                        else
                        {
                            generator.EmitStfld(fieldVar);
                        }
                    }
                    expressionResultType = fieldVar.FieldType;
                }
                else
                {
                    // Try to see if we are trying to grab a delegate
                    foreach (var method in store.Methods![currentMethodInfo.Type])
                    {
                        if (method.Name != varNode.Name)
                        {
                            continue;
                        }

                        if (expressionResultType == null)
                        {
                            throw new TypeCheckException("Expression result type cannot be null here, must have a type to assign delegate to");
                        }

                        if (!method.IsStatic && currentMethodInfo.IsStatic)
                        {
                            throw new MethodReferenceException("Cannot grab a direct reference to a instance delegate");
                        }

                        if (expressionResultType.IsAssignableFrom(store.Types["System.MulticastDelegate"]))
                        {
                            throw new MethodReferenceException("Target must be a delegate");
                        }

                        var rightParameters = store.MethodParameters[method];
                        var leftPossibleMethods = store.Methods[expressionResultType].Where(x => x.Name == "Invoke").ToArray();
                        if (leftPossibleMethods.Length != 1)
                        {
                            throw new MethodReferenceException("Must only have 1 invoke method on a delegate");
                        }
                        var leftParameters = store.MethodParameters[leftPossibleMethods[0]];

                        if (!leftParameters.SequenceEqual(rightParameters))
                        {
                            throw new MethodReferenceException("Method and delegate parameter types do not match");
                        }

                        if (method.ReturnType != leftPossibleMethods[0].ReturnType)
                        {
                            throw new MethodReferenceException("Method and delegate return types do not match");
                        }

                        // Find the constructor
                        var constructor = store.Constructors[expressionResultType]
                            .Where(x => store.ConstructorParameters[x].SequenceEqual(delegateConstructorTypes))
                            .First();

                        if (currentMethodInfo.IsStatic)
                        {
                            generator.EmitLdnull();
                        }
                        else
                        {
                            generator.EmitLdthis();
                        }

                        if (method.IsStatic)
                        {
                            generator.EmitLdftn(method);
                        }
                        else
                        {
                            generator.EmitDup();
                            generator.EmitLdvirtftn(method);
                        }

                        generator.EmitNewobj(constructor);
                        return;
                        ;
                    }

                    // If we get here, we are likely looking up a type. Try to look up the type
                    if (store.Types.TryGetValue(varNode.Name, out var typeLookup))
                    {
                        expressionResultType = typeLookup;
                        return;
                    }

                    throw new UnknownExpressionException($"Odd variable lookup: {varNode.Name}");
                }


            }
        }

        private void HandleMethodReference(MethodReferenceExpression methodRef, ref IType? expressionResultType)
        {
            IType? callTarget = null;
            WriteExpression(methodRef.Expression, true, false, ref callTarget);

            if (callTarget == null)
            {
                throw new MethodReferenceException("Method ref target cannot be null");
            }

            foreach (var method in store.Methods![callTarget])
            {
                if (method.Name != methodRef.Name)
                {
                    continue;
                }

                if (expressionResultType == null)
                {
                    throw new TypeCheckException("Expression result type cannot be null here, must have a delegate type to assign to");
                }

                if (expressionResultType.IsAssignableFrom(store.Types["System.MulticastDelegate"]))
                {
                    throw new MethodReferenceException("Target must be a delegate");
                }

                var rightParameters = store.MethodParameters[method];
                var leftPossibleMethods = store.Methods[expressionResultType].Where(x => x.Name == "Invoke").ToArray();
                if (leftPossibleMethods.Length != 1)
                {
                    throw new MethodReferenceException("Must only have 1 invoke method on a delegate");
                }
                var leftParameters = store.MethodParameters[leftPossibleMethods[0]];

                if (!leftParameters.SequenceEqual(rightParameters))
                {
                    throw new MethodReferenceException("Method and delegate parameter types do not match");
                }

                if (method.ReturnType != leftPossibleMethods[0].ReturnType)
                {
                    throw new MethodReferenceException("Method and delegate return types do not match");
                }


                if (method.IsStatic)
                {
                    throw new MethodReferenceException("Cannot grab an instance reference to a static delegate");
                }

                // Find the constructor
                var constructor = store.Constructors[expressionResultType]
                    .Where(x => store.ConstructorParameters[x].SequenceEqual(delegateConstructorTypes))
                    .First();

                // Object is already on stack
                if (method.IsStatic)
                {
                    generator.EmitLdftn(method);
                }
                else
                {
                    if (callTarget.IsValueType)
                    {
                        generator.EmitBox(callTarget);
                    }

                    generator.EmitDup();
                    generator.EmitLdvirtftn(method);
                }
                generator.EmitNewobj(constructor);
                return;
                ;
            }

            throw new UnknownExpressionException($"Odd method reference lookup");
        }

        private void HandleExpressionOpExpression(ExpressionOpExpressionSyntaxNode expOpEx, ref IType? expressionResultType)
        {
            IType? leftType = null;
            IType? rightType = null;

            WriteExpression(expOpEx.Left, true, false, ref leftType);
            WriteExpression(expOpEx.Right, true, false, ref rightType);
            typeChecker.TypeCheck(leftType, rightType);

            switch (expOpEx.Operation.Operation)
            {
                case SupportedOperation.Add:
                    typeChecker.CheckCanArithmaticTypeOperations(leftType, rightType);
                    generator.EmitAdd();
                    break;
                case SupportedOperation.Subtract:
                    typeChecker.CheckCanArithmaticTypeOperations(leftType, rightType);
                    generator.EmitSub();
                    break;
                case SupportedOperation.Multiply:
                    typeChecker.CheckCanArithmaticTypeOperations(leftType, rightType);
                    generator.EmitMul();
                    break;
                case SupportedOperation.Divide:
                    typeChecker.CheckCanArithmaticTypeOperations(leftType, rightType);
                    generator.EmitDiv();
                    break;
                case SupportedOperation.LessThen:
                    typeChecker.CheckCanArithmaticTypeOperations(leftType, rightType);
                    generator.EmitClt();
                    expressionResultType = store.Types["System.Boolean"];
                    return;
                case SupportedOperation.GreaterThen:
                    typeChecker.CheckCanArithmaticTypeOperations(leftType, rightType);
                    generator.EmitCgt();
                    expressionResultType = store.Types["System.Boolean"];
                    return;
                case SupportedOperation.NotEqual:
                    typeChecker.CheckCanArithmaticTypeOperations(leftType, rightType);
                    generator.EmitCeq();
                    // This is how to invert a bool
                    generator.EmitLdcI40();
                    generator.EmitCeq();
                    expressionResultType = store.Types["System.Boolean"];
                    return;
                case SupportedOperation.Equals:
                    typeChecker.CheckCanArithmaticTypeOperations(leftType, rightType);
                    generator.EmitCeq();
                    expressionResultType = store.Types["System.Boolean"];
                    return;
                case SupportedOperation.LessThenOrEqualTo:
                    typeChecker.CheckCanArithmaticTypeOperations(leftType, rightType);
                    generator.EmitCgt();
                    // This is how to invert a bool
                    generator.EmitLdcI40();
                    generator.EmitCeq();
                    expressionResultType = store.Types["System.Boolean"];
                    return;
                case SupportedOperation.GreaterThenOrEqualTo:
                    typeChecker.CheckCanArithmaticTypeOperations(leftType, rightType);
                    generator.EmitClt();
                    // This is how to invert a bool
                    generator.EmitLdcI40();
                    generator.EmitCeq();
                    expressionResultType = store.Types["System.Boolean"];
                    return;
                default:
                    throw new InvalidOperationException("Unsupported operation");
            }
            expressionResultType = leftType;
        }

        private void HandleMethodCall(MethodCallExpression methodCall, ref IType? expressionResultType)
        {
            bool isStatic = true;
            IType? callTarget = null;
            if (methodCall.Expression is MethodCallExpression)
            {
                isStatic = false;
                WriteExpression(methodCall.Expression, true, true, ref callTarget);
            }

            else if (methodCall.Expression is VariableSyntaxNode vdn)
            {
                if (!store.Types.TryGetValue(vdn.Name, out callTarget))
                {
                    isStatic = false;

                    IType? rexpressionType = null;

                    WriteExpression(vdn, true, true, ref rexpressionType);

                    if (rexpressionType == null)
                    {
                        throw new InvalidOperationException("Target for call not found");
                    }
                    else
                    {
                        callTarget = rexpressionType;
                    }
                }
            }
            else if (methodCall.Expression is VariableAccessExpression vae)
            {
                isStatic = false;
                //HandleVariableAccess(vae, true, ref callTarget);
                WriteExpression(vae, true, true, ref callTarget);
            }
            else
            {
                throw new InvalidOperationException("Op not suppored");
            }

            if (callTarget == null)
            {
                throw new InvalidOperationException("Must have a target");
            }

            var callParameterTypes = new List<IType>();
            foreach (var callParams in methodCall.Parameters)
            {
                callParameterTypes.Add(WriteCallParameter(callParams));
            }

            IMethodInfo? methodToCall = null;

            if (store.Methods!.TryGetValue(callTarget, out var localMethodList))
            {
                foreach (var localMethod in localMethodList)
                {
                    if (localMethod.Name == methodCall.Name && (localMethod.IsStatic == isStatic))
                    {
                        var localMethodParameters = store.MethodParameters[localMethod];
                        if (callParameterTypes.SequenceEqual(localMethodParameters))
                        {
                            methodToCall = localMethod;
                            break;
                        }
                    }
                }
            }

            if (methodToCall == null)
            {
                throw new InvalidOperationException("Method not found");
            }
            if (isStatic)
            {
                generator.EmitCall(methodToCall);
            }
            else
            {
                if (callTarget.IsValueType)
                {
                    generator.EmitCall(methodToCall);
                }
                else
                {

                    generator.EmitCallvirt(methodToCall);
                }


            }
            expressionResultType = methodToCall.ReturnType;
        }

        private void HandleNewConstructor(NewConstructorExpression newConstructor, ref IType? expressionResultType)
        {
            if (store.Types.TryGetValue(newConstructor.Name, out var callTarget))
            {
                var callParameterTypes = new List<IType>();
                // Calling a static function
                foreach (var callParams in newConstructor.Parameters)
                {
                    callParameterTypes.Add(WriteCallParameter(callParams));
                }

                IConstructorInfo? methodToCall = null;

                if (store.Constructors!.TryGetValue(callTarget, out var localConstructorList))
                {
                    foreach (var localMethod in localConstructorList)
                    {
                        var localMethodParameters = store.ConstructorParameters[localMethod];
                        if (callParameterTypes.SequenceEqual(localMethodParameters))
                        {
                            methodToCall = localMethod;
                            break;
                        }
                    }
                }

                if (methodToCall == null)
                {
                    throw new InvalidOperationException("Method not found");
                }
                generator.EmitNewobj(methodToCall);
                expressionResultType = callTarget;
            }
            else
            {
                throw new InvalidOperationException("Cannot construct this type");
            }


        }

        private void HandleNewArray(NewArrExpression newConstructor, ref IType? expressionResultType)
        {
            if (store.Types.TryGetValue(newConstructor.Name, out var callTarget))
            {
                var arrayType = callTarget.MakeArrayType();

                IType? sizeResultType = null;
                WriteExpression(newConstructor.Expression, true, false, ref sizeResultType);
                typeChecker.TypeCheck(store.Types["System.Int32"], sizeResultType);

                generator.EmitNewarr(callTarget);

                expressionResultType = arrayType;
            }
            else
            {
                throw new InvalidOperationException("Cannot construct this type");
            }
        }

        private void HandleVariableAccess(VariableAccessExpression varAccess, bool isRight, bool isMethodAccess, ref IType? expressionResultType)
        {
            IType? callTarget = null;
            WriteExpression(varAccess.Expression, true, isMethodAccess, ref callTarget);

            if (callTarget == null)
            {
                throw new InstanceFieldAccessException("No target for field access");
            }

            IFieldInfo? fieldToAccess = null;

            if (store.Fields!.TryGetValue(callTarget, out var localFieldList))
            {
                foreach (var localField in localFieldList)
                {
                    if (localField.Name == varAccess.Name)
                    {
                        fieldToAccess = localField;
                        break;
                    }
                }
            }

            if (fieldToAccess == null)
            {
                throw new InstanceFieldAccessException("Field not found");
            }

            if (isRight)
            {
                if (isMethodAccess && fieldToAccess.FieldType.IsValueType)
                {
                    generator.EmitLdflda(fieldToAccess);
                }
                else
                {
                    generator.EmitLdfld(fieldToAccess);
                }
            }
            else
            {
                generator.EmitStfld(fieldToAccess);
            }
            expressionResultType = fieldToAccess.FieldType;
        }

        private void HandleArrayExpression(ArrayIndexExpression arrIdx, bool isRight, ref IType? expressionResultType)
        {
            IType? callTarget = null;
            WriteExpression(arrIdx.Expression, true, false, ref callTarget);

            if (callTarget == null)
            {
                throw new MissingTypeException("No target for array access");
            }

            if (!callTarget.IsArray)
            {
                throw new TypeCheckException("Target must be an array");
            }

            IType? lengthType = null;
            WriteExpression(arrIdx.LengthExpression, true, false, ref lengthType);

            typeChecker.TypeCheck(store.Types["System.Int32"], lengthType);

            expressionResultType = callTarget.GetMethod("Get").ReturnType;

            if (isRight)
            {
                generator.EmitLdelem(expressionResultType);
            }
            else
            {
                generator.EmitStelem(expressionResultType);
            }
        }

        private void WriteExpression(ExpressionSyntaxNode? expression, bool isRight, bool willBeMethodCall, ref IType? expressionResultType)
        {
            if (expression == null)
            {
                return;
            }

            switch (expression)
            {
                case IntConstantSyntaxNode intConstant:
                    generator.EmitLdcI4(intConstant.Value);
                    expressionResultType = store.Types["System.Int32"];
                    break;
                case StringConstantNode stringConstant:
                    generator.EmitLdstr(stringConstant.Value);
                    expressionResultType = store.Types["System.String"];
                    break;
                case TrueConstantNode _:
                    generator.EmitTrue();
                    expressionResultType = store.Types["System.Boolean"];
                    break;
                case FalseConstantNode _:
                    generator.EmitFalse();
                    expressionResultType = store.Types["System.Boolean"];
                    break;
                case NullConstantNode _:
                    generator.EmitLdnull();
                    expressionResultType = null;
                    break;
                case VariableSyntaxNode varNode:
                    HandleVariableExpression(varNode, isRight, willBeMethodCall, ref expressionResultType);
                    break;
                case MethodReferenceExpression methodRef:
                    if (!isRight)
                    {
                        throw new InvalidLValueException("Method ref must be on the right");
                    }
                    HandleMethodReference(methodRef, ref expressionResultType);
                    break;
                case ExpressionOpExpressionSyntaxNode expOpEx:
                    if (!isRight)
                    {
                        throw new InvalidLValueException("Exp op Exp must be on the right");
                    }
                    HandleExpressionOpExpression(expOpEx, ref expressionResultType);
                    break;
                case MethodCallExpression methodCall:
                    if (!isRight)
                    {
                        throw new InvalidLValueException("Method Call must be on the right");
                    }
                    HandleMethodCall(methodCall, ref expressionResultType);
                    break;
                case NewConstructorExpression newConstructor:
                    if (!isRight)
                    {
                        throw new InvalidLValueException("New must be on the right");
                    }
                    HandleNewConstructor(newConstructor, ref expressionResultType);
                    break;
                case VariableAccessExpression varAccess:
                    HandleVariableAccess(varAccess, isRight, willBeMethodCall, ref expressionResultType);
                    break;

                case NewArrExpression newArr:
                    if (!isRight)
                    {
                        throw new InvalidLValueException("newarr must be on the right");
                    }
                    HandleNewArray(newArr, ref expressionResultType);
                    break;
                case ArrayIndexExpression arrIdx:
                    HandleArrayExpression(arrIdx, isRight, ref expressionResultType);
                    break;
                default:
                    throw new UnknownExpressionException("Expression not supported");
            }


        }

        private void HandleWhileStatement(WhileStatement statement)
        {
            // Make a top label

            var topLabel = generator.DefineLabel();
            var bottomLabel = generator.DefineLabel();

            // Jump to our bottom label, mark top label after the jump
            generator.EmitBr(bottomLabel);
            generator.MarkLabel(topLabel);

            // Write our statements
            foreach (var stmt in statement.Statements)
            {
                WriteStatement(stmt);
            }

            // mark bottom label after statments
            generator.MarkLabel(bottomLabel);

            IType? expressionResultType = null;
            WriteExpression(statement.Expression, true, false, ref expressionResultType);
            // Result of expression must be a bool
            typeChecker.TypeCheck(store.Types["System.Boolean"], expressionResultType);
            generator.EmitBrtrue(topLabel);


        }

        private void HandleIfStatement(IfElseStatement statement)
        {
            var elseLabel = generator.DefineLabel();
            var endLabel = generator.DefineLabel();

            IType? expressionResultType = null;
            WriteExpression(statement.Expression, true, false, ref expressionResultType);
            // Result of expression must be a bool
            typeChecker.TypeCheck(store.Types["System.Boolean"], expressionResultType);
            generator.EmitBrfalse(elseLabel);

            // Emit top statements
            foreach (var stmt in statement.Statements)
            {
                WriteStatement(stmt);
            }

            generator.EmitBr(endLabel);

            generator.MarkLabel(elseLabel);

            foreach (var stmt in statement.ElseStatements)
            {
                WriteStatement(stmt);
            }

            generator.MarkLabel(endLabel);
        }

        public bool WriteStatement(StatementSyntaxNode statement)
        {
            IType? expressionResultType = null;

            switch (statement)
            {
                case ReturnStatementNode ret:
                    expressionResultType = currentMethodInfo.ReturnType;
                    WriteExpression(ret.Expression, true, false, ref expressionResultType);
                    typeChecker.TypeCheck(currentMethodInfo.ReturnType, expressionResultType);
                    generator.EmitRet();
                    return true;
                case VariableDeclarationNode vardec:
                    {
                        if (vardec.Type != null)
                        {
                            expressionResultType = store.TypeDefLookup(vardec.Type);
                        }

                        WriteExpression(vardec.Expression, true, false, ref expressionResultType);
                        var type = vardec.Type;
                        if (type == null)
                        {
                            // This is where type inferrence is handled
                            if (expressionResultType == null)
                            {
                                throw new TypeInferrenceException("Failure to type infer");
                            }
                            type = expressionResultType.FullName;
                        }
                        else
                        {
                            typeChecker.TypeCheck(store.Types[type], expressionResultType);
                        }
                        var loc = generator.DeclareLocal(store.Types[type], vardec.Name);
                        currentMethodInfo.Locals.Add(vardec.Name, loc);
                        generator.EmitStloc(loc);
                    }
                    break;
                case ExpressionEqualsExpressionSyntaxNode expEqualsExp:
                    {
                        IType? rightType = null;

                        var lastOp = WriteLValueExpression(expEqualsExp.Left, out var leftType);

                        WriteExpression(expEqualsExp.Right, true, false, ref rightType);
                        typeChecker.TypeCheck(leftType, rightType);

                        lastOp();
                    }
                    break;
                case ExpressionSyntaxNode expStatement:
                    WriteExpression(expStatement, true, false, ref expressionResultType);
                    if (expressionResultType != null && expressionResultType.FullName != "System.Void")
                    {
                        throw new NotSupportedException("Stack must be emptied");
                    }
                    break;
                case BaseClassConstructorSyntax _:
                    generator.EmitLdthis();
                    generator.EmitConstructorCall(baseConstructorCall);
                    break;
                case WhileStatement whileStatement:
                    HandleWhileStatement(whileStatement);
                    break;
                case IfElseStatement ifElseStatement:
                    HandleIfStatement(ifElseStatement);
                    break;
                default:
                    throw new NotSupportedException("This statement is not supported");
            }
            return false;
        }

        public void EmitRet()
        {
            generator.EmitRet();
        }
    }
}
