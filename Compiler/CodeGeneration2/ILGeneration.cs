﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Compiler.Parser.Nodes;
using Compiler.Parser.Nodes.Statements;
using Compiler.Tokenizer.Tokens;
using static Compiler.TypeChecker.SimpleTypeChecker;

namespace Compiler.CodeGeneration2
{
    public class CurrentMethodInfo
    {
        public Type ReturnType { get; }

        public Type Type { get; }

        public bool IsStatic { get; }

        public Dictionary<Type, LocalBuilder> RefStoreLocals { get; } = new Dictionary<Type, LocalBuilder>();

        public Dictionary<string, LocalBuilder> Locals { get; } = new Dictionary<string, LocalBuilder>();

        public IReadOnlyDictionary<string, (short idx, Type type)> Parameters { get; }

        public IReadOnlyDictionary<string, FieldInfo> Fields { get; }

        public CurrentMethodInfo(Type type, Type returnType, bool isStatic,
            IReadOnlyDictionary<string, (short idx, Type type)> parameters,
            IReadOnlyDictionary<string, FieldInfo> fields)
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

        public ILGeneration(IILGenerator generator, CodeGenerationStore store, CurrentMethodInfo currentMethodInfo)
        {
            this.generator = generator;
            this.store = store;
            this.currentMethodInfo = currentMethodInfo;
        }

        // x = a + b

        private Action WriteLValueExpression(ExpressionSyntaxNode expression, out Type? expressionResultType)
        {
            switch (expression)
            {
                case ArrayIndexExpression arrIdx:
                    Type? arrayType = null;
                    Type? lengthType = null;

                    WriteExpression(arrIdx.Expression, true, false, ref arrayType);
                    WriteExpression(arrIdx.LengthExpression, true, false, ref lengthType);

                    TypeCheck(typeof(int), lengthType);

                    if (arrayType == null)
                    {
                        throw new InvalidOperationException("Array type must have been found");
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
                        throw new InvalidOperationException("Not supported");
                    }
                case VariableAccessExpression varAccess:
                    {
                        Type? callTarget = null;
                        WriteExpression(varAccess.Expression, true, false, ref callTarget);

                        if (callTarget == null)
                        {
                            throw new InvalidOperationException("No target for field access");
                        }

                        FieldInfo? fieldToCall = null;

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
                            throw new InvalidOperationException("Field target not found");
                        }

                        expressionResultType = fieldToCall.FieldType;
                        return () => generator.EmitStfld(fieldToCall);
                    }
                default:
                    throw new InvalidOperationException("No other type of operations supported as lvalue");
            }
        }

        private Type WriteCallParameter(CallParameterSyntaxNode callNode)
        {
            if (callNode == null)
            {
                throw new ArgumentNullException(nameof(callNode));
            }

            Type? expressionResultType = null;
            WriteExpression(callNode.Expression, true, false, ref expressionResultType);
            if (expressionResultType == null)
            {
                throw new InvalidOperationException("Expression must return something here");
            }
            return expressionResultType;
        }

        private void HandleVariableExpression(VariableSyntaxNode varNode, bool isRight, bool willBeMethodCall, ref Type? expressionResultType)
        {
            {
                if (!isRight && willBeMethodCall)
                {
                    throw new InvalidOperationException("Cannot have a method call on the left");
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
                    if (currentMethodInfo.IsStatic)
                    {
                        throw new InvalidOperationException("Invalid to do this on static");
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
                            throw new InvalidOperationException("Expression result type cannot be null here");
                        }

                        if (!method.IsStatic && currentMethodInfo.IsStatic)
                        {
                            throw new InvalidOperationException("Cannot grab a direct reference to a instance delegate");
                        }

                        if (expressionResultType.IsAssignableFrom(typeof(MulticastDelegate)))
                        {
                            throw new InvalidOperationException("Target must be a delegate");
                        }

                        var rightParameters = store.MethodParameters[method];
                        var leftPossibleMethods = store.Methods[expressionResultType].Where(x => x.Name == "Invoke").ToArray();
                        if (leftPossibleMethods.Length != 1)
                        {
                            throw new InvalidOperationException("Must only have 1 invoke method on a delegate");
                        }
                        var leftParameters = store.MethodParameters[leftPossibleMethods[0]];

                        if (!leftParameters.SequenceEqual(rightParameters))
                        {
                            throw new InvalidOperationException("Method and delegate types do not match");
                        }

                        if (method.ReturnType != leftPossibleMethods[0].ReturnType)
                        {
                            throw new InvalidOperationException("Method and delegate return types do not match");
                        }

                        // Find the constructor
                        var constructor = store.Constructors[expressionResultType]
                            .Where(x => store.ConstructorParameters[x].SequenceEqual(new Type[] { typeof(object), typeof(IntPtr) }))
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

                    throw new InvalidOperationException("Not supported");
                }


            }
        }

        private void HandleMethodReference(MethodReferenceExpression methodRef, ref Type? expressionResultType)
        {
            Type? callTarget = null;
            WriteExpression(methodRef.Expression, true, false, ref callTarget);

            if (callTarget == null)
            {
                throw new InvalidOperationException("Method ref target cannot be null");
            }

            foreach (var method in store.Methods![callTarget])
            {
                if (method.Name != methodRef.Name)
                {
                    continue;
                }

                if (expressionResultType == null)
                {
                    throw new InvalidOperationException("Expression result type cannot be null here");
                }

                if (expressionResultType.IsAssignableFrom(typeof(MulticastDelegate)))
                {
                    throw new InvalidOperationException("Target must be a delegate");
                }

                var rightParameters = store.MethodParameters[method];
                var leftPossibleMethods = store.Methods[expressionResultType].Where(x => x.Name == "Invoke").ToArray();
                if (leftPossibleMethods.Length != 1)
                {
                    throw new InvalidOperationException("Must only have 1 invoke method on a delegate");
                }
                var leftParameters = store.MethodParameters[leftPossibleMethods[0]];

                if (!leftParameters.SequenceEqual(rightParameters))
                {
                    throw new InvalidOperationException("Method and delegate types do not match");
                }

                if (method.ReturnType != leftPossibleMethods[0].ReturnType)
                {
                    throw new InvalidOperationException("Method and delegate return types do not match");
                }


                if (method.IsStatic)
                {
                    throw new InvalidOperationException("Cannot grab an instance reference to a static delegate");
                }

                // Find the constructor
                var constructor = store.Constructors[expressionResultType]
                    .Where(x => store.ConstructorParameters[x].SequenceEqual(new Type[] { typeof(object), typeof(IntPtr) }))
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

            throw new InvalidOperationException("Not supported");
        }

        private void HandleExpressionOpExpression(ExpressionOpExpressionSyntaxNode expOpEx, ref Type? expressionResultType)
        {
            Type? leftType = null;
            Type? rightType = null;

            WriteExpression(expOpEx.Left, true, false, ref leftType);
            WriteExpression(expOpEx.Right, true, false, ref rightType);
            TypeCheck(leftType, rightType);

            switch (expOpEx.Operation.Operation)
            {
                case SupportedOperation.Add:
                    CheckCanArithmaticTypeOperations(leftType, rightType);
                    generator.EmitAdd();
                    break;
                case SupportedOperation.Subtract:
                    CheckCanArithmaticTypeOperations(leftType, rightType);
                    generator.EmitSub();
                    break;
                case SupportedOperation.Multiply:
                    CheckCanArithmaticTypeOperations(leftType, rightType);
                    generator.EmitMul();
                    break;
                case SupportedOperation.Divide:
                    CheckCanArithmaticTypeOperations(leftType, rightType);
                    generator.EmitDiv();
                    break;
                case SupportedOperation.LessThen:
                    CheckCanArithmaticTypeOperations(leftType, rightType);
                    generator.EmitClt();
                    expressionResultType = typeof(bool);
                    return;
                case SupportedOperation.GreaterThen:
                    CheckCanArithmaticTypeOperations(leftType, rightType);
                    generator.EmitCgt();
                    expressionResultType = typeof(bool);
                    return;
                case SupportedOperation.NotEqual:
                    CheckCanArithmaticTypeOperations(leftType, rightType);
                    generator.EmitCeq();
                    // This is how to invert a bool
                    generator.EmitLdcI40();
                    generator.EmitCeq();
                    expressionResultType = typeof(bool);
                    return;
                case SupportedOperation.Equals:
                    CheckCanArithmaticTypeOperations(leftType, rightType);
                    generator.EmitCeq();
                    expressionResultType = typeof(bool);
                    return;
                case SupportedOperation.LessThenOrEqualTo:
                    CheckCanArithmaticTypeOperations(leftType, rightType);
                    generator.EmitCgt();
                    // This is how to invert a bool
                    generator.EmitLdcI40();
                    generator.EmitCeq();
                    expressionResultType = typeof(bool);
                    return;
                case SupportedOperation.GreaterThenOrEqualTo:
                    CheckCanArithmaticTypeOperations(leftType, rightType);
                    generator.EmitClt();
                    // This is how to invert a bool
                    generator.EmitLdcI40();
                    generator.EmitCeq();
                    expressionResultType = typeof(bool);
                    return;
                default:
                    throw new InvalidOperationException("Unsupported operation");
            }
            expressionResultType = leftType;
        }

        private void HandleMethodCall(MethodCallExpression methodCall, ref Type? expressionResultType)
        {
            bool isStatic = true;
            Type? callTarget = null;
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

                    Type? rexpressionType = null;

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
            else
            {
                throw new InvalidOperationException("Op not suppored");
            }

            if (callTarget == null)
            {
                throw new InvalidOperationException("Must have a target");
            }

            var callParameterTypes = new List<Type>();
            foreach (var callParams in methodCall.Parameters)
            {
                callParameterTypes.Add(WriteCallParameter(callParams));
            }

            MethodInfo? methodToCall = null;

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

        private void HandleNewConstructor(NewConstructorExpression newConstructor, ref Type? expressionResultType)
        {
            if (store.Types.TryGetValue(newConstructor.Name, out var callTarget))
            {
                var callParameterTypes = new List<Type>();
                // Calling a static function
                foreach (var callParams in newConstructor.Parameters)
                {
                    callParameterTypes.Add(WriteCallParameter(callParams));
                }

                ConstructorInfo? methodToCall = null;

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

        private void HandleNewArray(NewArrExpression newConstructor, ref Type? expressionResultType)
        {
            if (store.Types.TryGetValue(newConstructor.Name, out var callTarget))
            {
                var arrayType = callTarget.MakeArrayType();

                Type? sizeResultType = null;
                WriteExpression(newConstructor.Expression, true, false, ref sizeResultType);
                TypeCheck(typeof(int), sizeResultType);

                generator.EmitNewarr(callTarget);

                expressionResultType = arrayType;
            }
            else
            {
                throw new InvalidOperationException("Cannot construct this type");
            }
        }

        private void HandleVariableAccess(VariableAccessExpression varAccess, bool isRight, ref Type? expressionResultType)
        {
            Type? callTarget = null;
            WriteExpression(varAccess.Expression, true, false, ref callTarget);

            if (callTarget == null)
            {
                throw new InvalidOperationException("No target for field access");
            }

            FieldInfo? methodToCall = null;

            if (store.Fields!.TryGetValue(callTarget, out var localMethodList))
            {
                foreach (var localMethod in localMethodList)
                {
                    if (localMethod.Name == varAccess.Name)
                    {
                        methodToCall = localMethod;
                        break;
                    }
                }
            }

            if (methodToCall == null)
            {
                throw new InvalidOperationException("Field target not found");
            }

            if (isRight)
            {
                generator.EmitLdfld(methodToCall);
            }
            else
            {
                generator.EmitStfld(methodToCall);
            }
            expressionResultType = methodToCall.FieldType;
        }

        private void HandleArrayExpression(ArrayIndexExpression arrIdx, bool isRight, ref Type? expressionResultType)
        {
            Type? callTarget = null;
            WriteExpression(arrIdx.Expression, true, false, ref callTarget);

            if (callTarget == null)
            {
                throw new InvalidOperationException("No target for array access");
            }

            if (!callTarget.IsArray)
            {
                throw new InvalidOperationException("Target must be an array");
            }

            Type? lengthType = null;
            WriteExpression(arrIdx.LengthExpression, true, false, ref lengthType);

            TypeCheck(typeof(int), lengthType);

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

        private void WriteExpression(ExpressionSyntaxNode? expression, bool isRight, bool willBeMethodCall, ref Type? expressionResultType)
        {
            if (expression == null)
            {
                return;
            }

            switch (expression)
            {
                case IntConstantSyntaxNode intConstant:
                    generator.EmitLdcI4(intConstant.Value);
                    expressionResultType = typeof(int);
                    break;
                case StringConstantNode stringConstant:
                    generator.EmitLdstr(stringConstant.Value);
                    expressionResultType = typeof(string);
                    break;
                case TrueConstantNode _:
                    generator.EmitTrue();
                    expressionResultType = typeof(bool);
                    break;
                case FalseConstantNode _:
                    generator.EmitFalse();
                    expressionResultType = typeof(bool);
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
                        throw new InvalidOperationException("Method ref must be on the right");
                    }
                    HandleMethodReference(methodRef, ref expressionResultType);
                    break;
                case ExpressionOpExpressionSyntaxNode expOpEx:
                    if (!isRight)
                    {
                        throw new InvalidOperationException("Exp op Exp must be on the right");
                    }
                    HandleExpressionOpExpression(expOpEx, ref expressionResultType);
                    break;
                case MethodCallExpression methodCall:
                    if (!isRight)
                    {
                        throw new InvalidOperationException("Method Call must be on the right");
                    }
                    HandleMethodCall(methodCall, ref expressionResultType);
                    break;
                case NewConstructorExpression newConstructor:
                    if (!isRight)
                    {
                        throw new InvalidOperationException("New must be on the right");
                    }
                    HandleNewConstructor(newConstructor, ref expressionResultType);
                    break;
                case VariableAccessExpression varAccess:
                    HandleVariableAccess(varAccess, isRight, ref expressionResultType);
                    break;

                case NewArrExpression newArr:
                    if (!isRight)
                    {
                        throw new InvalidOperationException("newarr must be on the right");
                    }
                    HandleNewArray(newArr, ref expressionResultType);
                    break;
                case ArrayIndexExpression arrIdx:
                    HandleArrayExpression(arrIdx, isRight, ref expressionResultType);
                    break;
                default:
                    throw new InvalidOperationException("Expression not supported");
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

            Type? expressionResultType = null;
            WriteExpression(statement.Expression, true, false, ref expressionResultType);
            // Result of expression must be a bool
            TypeCheck(typeof(bool), expressionResultType);
            generator.EmitBrtrue(topLabel);


        }

        private void HandleIfStatement(IfElseStatement statement)
        {
            var elseLabel = generator.DefineLabel();
            var endLabel = generator.DefineLabel();

            Type? expressionResultType = null;
            WriteExpression(statement.Expression, true, false, ref expressionResultType);
            // Result of expression must be a bool
            TypeCheck(typeof(bool), expressionResultType);
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
            Type? expressionResultType = null;

            switch (statement)
            {
                case ReturnStatementNode ret:
                    expressionResultType = currentMethodInfo.ReturnType;
                    WriteExpression(ret.Expression, true, false, ref expressionResultType);
                    TypeCheck(currentMethodInfo.ReturnType, expressionResultType);
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
                            if (expressionResultType == null)
                            {
                                throw new InvalidOperationException("Failure to type infer");
                            }
                            type = expressionResultType.FullName;
                        }
                        else
                        {
                            TypeCheck(store.Types[type], expressionResultType);
                        }
                        var loc = generator.DeclareLocal(store.Types[type]);
                        currentMethodInfo.Locals.Add(vardec.Name, loc);
                        generator.EmitStloc(loc);
                    }
                    break;
                case ExpressionEqualsExpressionSyntaxNode expEqualsExp:
                    {
                        Type? rightType = null;

                        var lastOp = WriteLValueExpression(expEqualsExp.Left, out var leftType);

                        WriteExpression(expEqualsExp.Right, true, false, ref rightType);
                        TypeCheck(leftType, rightType);

                        lastOp();
                    }
                    break;
                case ExpressionSyntaxNode expStatement:
                    WriteExpression(expStatement, true, false, ref expressionResultType);
                    if (expressionResultType != null && expressionResultType != typeof(void))
                    {
                        throw new NotSupportedException("Stack must be emptied");
                    }
                    break;
                case BaseClassConstructorSyntax _:
                    generator.EmitLdthis();
                    generator.EmitConstructorCall(typeof(object).GetConstructor(Array.Empty<Type>()));
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
