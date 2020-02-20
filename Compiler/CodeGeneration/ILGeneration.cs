using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Compiler.Parser.Nodes;
using Compiler.Parser.Nodes.Statements;
using Compiler.Tokenizer.Tokens;
using static Compiler.TypeChecker.SimpleTypeChecker;

namespace Compiler.CodeGeneration
{
    public class GenerationStore
    {
        public Type CurrentType { get; }

        public bool IsStatic { get; }
        public IDictionary<string, LocalBuilder> Locals { get; } = new Dictionary<string, LocalBuilder>();

        public IReadOnlyList<(DelegateSyntaxNode syntax, Type type, ConstructorBuilder constructor)> Delegates { get; }

        public IReadOnlyDictionary<string, FieldBuilder> Fields { get; }

        public IReadOnlyDictionary<Type, IReadOnlyList<FieldBuilder>>? GettingCompiledFields { get; set; }

        public IReadOnlyDictionary<string, int> Parameters { get; }

        public IReadOnlyDictionary<string, Type> AllowedTypes { get; }

        public IReadOnlyDictionary<Type, IReadOnlyList<(MethodBuilder builder, MethodSyntaxNode syntax, GenerationStore store)>>? GettingCompiledTypes { get; set; }

        public IReadOnlyDictionary<Type, IReadOnlyList<(ConstructorBuilder builder, ConstructorSyntaxNode syntax, GenerationStore store)>>? GettingCompiledTypeConstructors { get; set; }

        public IReadOnlyDictionary<int, Type> ParameterTypes { get; }

        public Type? ReturnType { get; }

        public GenerationStore(Type currentType, bool isStatic, IReadOnlyDictionary<string, FieldBuilder> fields,
            IReadOnlyDictionary<string, int> parameters, IReadOnlyDictionary<string, Type> allowedTypes,
            IReadOnlyDictionary<int, Type> parameterTypes,
            IReadOnlyList<(DelegateSyntaxNode syntax, Type type, ConstructorBuilder constructor)> delegates, Type? returnType)
        {
            CurrentType = currentType;
            IsStatic = isStatic;
            Fields = fields;
            Parameters = parameters;
            AllowedTypes = allowedTypes;
            ParameterTypes = parameterTypes;
            ReturnType = returnType;
            Delegates = delegates;
        }
    }

    public static class ILGeneration
    {
        public static Action WriteLValueExpression(ILGenerator generator, GenerationStore store, ExpressionSyntaxNode expression, out Type? expressionResultType)
        {
            if (generator == null)
            {
                throw new ArgumentNullException(nameof(generator));
            }

            if (store == null)
            {
                throw new ArgumentNullException(nameof(store));
            }

            switch (expression)
            {
                case VariableSyntaxNode varNode:
                    if (store.Locals.TryGetValue(varNode.Name, out var localVar))
                    {
                        expressionResultType = localVar.LocalType;
                        return () => generator.Emit(OpCodes.Stloc, localVar);
                    }
                    else if (store.Parameters.TryGetValue(varNode.Name, out var parameterVar))
                    {
                        expressionResultType = store.ParameterTypes[parameterVar];
                        return () => generator.Emit(OpCodes.Starg, parameterVar);
                    }
                    else if (store.Fields.TryGetValue(varNode.Name, out var fieldVar))
                    {
                        generator.Emit(OpCodes.Ldarg_0);
                        expressionResultType = fieldVar.FieldType;
                        return () => generator.Emit(OpCodes.Stfld, fieldVar);
                    }
                    else
                    {
                        throw new InvalidOperationException("Not supported");
                    }
                case VariableAccessExpression varAccess:
                    {
                        Type? callTarget = null;
                        WriteExpression(generator, store, varAccess.Expression, true, ref callTarget);

                        if (callTarget == null)
                        {
                            throw new InvalidOperationException("No target for field access");
                        }

                        FieldInfo? methodToCall = null;

                        if (store.GettingCompiledFields!.TryGetValue(callTarget, out var localMethodList))
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
                        else
                        {
                            var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
                            methodToCall = callTarget.GetField(varAccess.Name, bindingFlags);
                        }

                        if (methodToCall == null)
                        {
                            throw new InvalidOperationException("Field target not found");
                        }

                        expressionResultType = methodToCall.FieldType;
                        return () => generator.Emit(OpCodes.Stfld, methodToCall);
                    }
                default:
                    throw new InvalidOperationException("No other type of operations supported as lvalue");
            }
        }

        public static bool WriteStatement(ILGenerator generator, GenerationStore store, StatementSyntaxNode statement)
        {
            if (generator == null)
            {
                throw new ArgumentNullException(nameof(generator));
            }

            if (store == null)
            {
                throw new ArgumentNullException(nameof(store));
            }

            Type? expressionResultType = null;

            switch (statement)
            {
                case ReturnStatementNode ret:
                    WriteExpression(generator, store, ret.Expression, true, ref expressionResultType);
                    TypeCheck(store.ReturnType, expressionResultType);
                    generator.Emit(OpCodes.Ret);
                    return true;
                case VariableDeclarationNode vardec:
                    {
                        WriteExpression(generator, store, vardec.Expression, true, ref expressionResultType);
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
                            TypeCheck(store.AllowedTypes[type], expressionResultType);
                        }
                        var loc = generator.DeclareLocal(store.AllowedTypes[type]);
                        store.Locals.Add(vardec.Name, loc);
                        generator.Emit(OpCodes.Stloc, loc);
                    }
                    break;
                case ExpressionEqualsExpressionSyntaxNode expEqualsExp:
                    {
                        Type? rightType = null;

                        var lastOp = WriteLValueExpression(generator, store, expEqualsExp.Left, out var leftType);

                        WriteExpression(generator, store, expEqualsExp.Right, true, ref rightType);
                        TypeCheck(leftType, rightType);

                        lastOp();
                    }
                    break;
                case ExpressionSyntaxNode expStatement:
                    WriteExpression(generator, store, expStatement, false, ref expressionResultType);
                    if (expressionResultType != null && expressionResultType != typeof(void))
                    {
                        throw new NotSupportedException("Stack must be emptied");
                        //generator.Emit(OpCodes.Pop);
                    }
                    break;
                case BaseClassConstructorSyntax _:
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Call, typeof(object).GetConstructor(Array.Empty<Type>()));
                    break;
                default:
                    throw new NotSupportedException("This statement is not supported");
            }
            return false;
        }

        public static Type WriteCallParameter(ILGenerator generator, GenerationStore store, CallParameterSyntaxNode callNode)
        {
            if (callNode == null)
            {
                throw new ArgumentNullException(nameof(callNode));
            }

            Type? expressionResultType = null;
            WriteExpression(generator, store, callNode.Expression, true, ref expressionResultType);
            if (expressionResultType == null)
            {
                throw new InvalidOperationException("Expression must return something here");
            }
            return expressionResultType;
        }

        public static void WriteExpression(ILGenerator generator, GenerationStore store, ExpressionSyntaxNode? expression, bool isRight,
      ref Type? expressionResultType)
        {
            if (expression == null)
            {
                return;
            }

            if (generator == null)
            {
                throw new ArgumentNullException(nameof(generator));
            }

            if (store == null)
            {
                throw new ArgumentNullException(nameof(store));
            }

            switch (expression)
            {
                case IntConstantSyntaxNode intConstant:
                    generator.Emit(OpCodes.Ldc_I4, intConstant.Value);
                    expressionResultType = typeof(int);
                    break;
                case StringConstantNode stringConstant:
                    generator.Emit(OpCodes.Ldstr, stringConstant.Value);
                    expressionResultType = typeof(string);
                    break;
                case TrueConstantNode _:
                    generator.Emit(OpCodes.Ldc_I4_1);
                    expressionResultType = typeof(bool);
                    break;
                case FalseConstantNode _:
                    generator.Emit(OpCodes.Ldc_I4_1);
                    expressionResultType = typeof(bool);
                    break;
                case NullConstantNode _:
                    generator.Emit(OpCodes.Ldnull);
                    expressionResultType = null;
                    break;
                case VariableSyntaxNode varNode:
                    {

                        if (store.Locals.TryGetValue(varNode.Name, out var localVar))
                        {
                            if (isRight)
                            {
                                generator.Emit(OpCodes.Ldloc, localVar);
                            }
                            else
                            {
                                generator.Emit(OpCodes.Stloc, localVar);
                            }
                            expressionResultType = localVar.LocalType;
                        }
                        else if (store.Parameters.TryGetValue(varNode.Name, out var parameterVar))
                        {
                            if (isRight)
                            {
                                generator.Emit(OpCodes.Ldarg, parameterVar);
                            }
                            else
                            {
                                generator.Emit(OpCodes.Starg, parameterVar);
                            }
                            expressionResultType = store.ParameterTypes[parameterVar];
                        }
                        else if (store.Fields.TryGetValue(varNode.Name, out var fieldVar))
                        {
                            if (store.IsStatic)
                            {
                                throw new InvalidOperationException("Invalid to do this on static");
                            }

                            if (isRight)
                            {
                                generator.Emit(OpCodes.Ldarg_0);
                                generator.Emit(OpCodes.Ldfld, fieldVar);
                            }
                            else
                            {
                                generator.Emit(OpCodes.Stfld, fieldVar);
                            }
                            expressionResultType = fieldVar.FieldType;
                        }
                        else
                        {
                            // Try to see if we are trying to grab a delegate
                            foreach (var method in store.GettingCompiledTypes![store.CurrentType])
                            {
                                if (method.syntax.Name != varNode.Name)
                                {
                                    continue;
                                }

                                if (!method.syntax.IsStatic)
                                {
                                    throw new InvalidOperationException("Cannot grab a direct reference to a instance delegate");
                                }

                                var numParameters = method.syntax.Parameters.Count;

                                Type? actionType = null;
                                ConstructorBuilder? constructor = null;

                                foreach (var del in store.Delegates)
                                {
                                    if (del.syntax.ReturnType == method.syntax.ReturnType
                                        && del.syntax.Parameters.Select(x => x.Type).SequenceEqual(method.syntax.Parameters.Select(x => x.Type)))
                                    {
                                        actionType = del.type;
                                        constructor = del.constructor;
                                        break;
                                    }
                                }

                                if (actionType == null || constructor == null)
                                {
                                    throw new InvalidOperationException("Action type was not found");
                                }

                                generator.Emit(OpCodes.Ldnull);
                                generator.Emit(OpCodes.Ldftn, method.builder);
                                generator.Emit(OpCodes.Newobj, constructor);
                                expressionResultType = actionType;
                                return;
                                ;
                            }

                            throw new InvalidOperationException("Not supported");
                        }


                    }
                    break;
                case ExpressionOpExpressionSyntaxNode expOpEx:
                    {
                        Type? leftType = null;
                        Type? rightType = null;

                        WriteExpression(generator, store, expOpEx.Right, true, ref rightType);
                        WriteExpression(generator, store, expOpEx.Left, true, ref leftType);
                        TypeCheck(leftType, rightType);

                        switch (expOpEx.Operation.Operation)
                        {
                            case SupportedOperation.Add:
                                CheckCanArithmaticTypeOperations(leftType, rightType);
                                generator.Emit(OpCodes.Add);
                                break;
                            case SupportedOperation.Subtract:
                                CheckCanArithmaticTypeOperations(leftType, rightType);
                                generator.Emit(OpCodes.Sub);
                                break;
                            case SupportedOperation.Multiply:
                                CheckCanArithmaticTypeOperations(leftType, rightType);
                                generator.Emit(OpCodes.Mul);
                                break;
                            case SupportedOperation.Divide:
                                CheckCanArithmaticTypeOperations(leftType, rightType);
                                generator.Emit(OpCodes.Div);
                                break;
                            default:
                                throw new InvalidOperationException("Unsupported operation");
                        }
                        expressionResultType = leftType;
                    }
                    ;
                    break;
                case MethodCallExpression methodCall:
                    {
                        bool isStatic = true;
                        Type? callTarget = null;
                        if (methodCall.Expression is MethodCallExpression)
                        {
                            isStatic = false;
                            WriteExpression(generator, store, methodCall.Expression, true, ref callTarget);
                        }

                        else if (methodCall.Expression is VariableSyntaxNode vdn)
                        {
                            if (!store.AllowedTypes.TryGetValue(vdn.Name, out callTarget))
                            {
                                isStatic = false;

                                Type? rexpressionType = null;

                                WriteExpression(generator, store, vdn, true, ref rexpressionType);

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
                        // Calling a static function
                        foreach (var callParams in methodCall.Parameters)
                        {
                            callParameterTypes.Add(WriteCallParameter(generator, store, callParams));
                        }

                        MethodInfo? methodToCall = null;

                        if (store.GettingCompiledTypes!.TryGetValue(callTarget, out var localMethodList))
                        {
                            foreach (var localMethod in localMethodList)
                            {
                                if (localMethod.syntax.Name == methodCall.Name && (localMethod.builder.IsStatic == isStatic))
                                {
                                    var localMethodParameters = localMethod.syntax.Parameters.Select(x => store.AllowedTypes[x.Type]);
                                    if (callParameterTypes.SequenceEqual(localMethodParameters))
                                    {
                                        methodToCall = localMethod.builder;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            var bindingFlags = BindingFlags.Public;
                            if (isStatic)
                            {
                                bindingFlags |= BindingFlags.Static;
                            }
                            else
                            {
                                bindingFlags |= BindingFlags.Instance;
                            }
                            methodToCall = callTarget.GetMethod(methodCall.Name, bindingFlags, null, callParameterTypes.ToArray(), null);
                        }


                        if (methodToCall == null)
                        {
                            throw new InvalidOperationException("Method not found");
                        }
                        if (isStatic)
                        {
                            generator.EmitCall(OpCodes.Call, methodToCall, null);
                        }
                        else
                        {
                            if (callTarget == typeof(int) || callTarget == typeof(bool))
                            {
                                // Special case int and bool because value type
                                if (methodToCall.Name == "ToString" && methodToCall.GetParameters().Length == 0)
                                {
                                    // We know top of stack is our instance var
                                    generator.Emit(OpCodes.Box, callTarget);
                                    generator.EmitCall(OpCodes.Callvirt, typeof(object).GetMethod("ToString", Array.Empty<Type>()), null);
                                }
                                else
                                {
                                    throw new InvalidOperationException("Calling instance methods on struct types is not supported");
                                }
                            }
                            else
                            {
                                generator.EmitCall(OpCodes.Callvirt, methodToCall, null);
                            }


                        }
                        expressionResultType = methodToCall.ReturnType;

                    }
                    break;
                case NewConstructorExpression newConstructor:
                    {
                        if (store.AllowedTypes.TryGetValue(newConstructor.Name, out var callTarget))
                        {
                            var callParameterTypes = new List<Type>();
                            // Calling a static function
                            foreach (var callParams in newConstructor.Parameters)
                            {
                                callParameterTypes.Add(WriteCallParameter(generator, store, callParams));
                            }

                            ConstructorInfo? methodToCall = null;

                            if (store.GettingCompiledTypeConstructors!.TryGetValue(callTarget, out var localConstructorList))
                            {
                                foreach (var localMethod in localConstructorList)
                                {
                                    var localMethodParameters = localMethod.syntax.Parameters.Select(x => store.AllowedTypes[x.Type]);
                                    if (callParameterTypes.SequenceEqual(localMethodParameters))
                                    {
                                        methodToCall = localMethod.builder;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                methodToCall = callTarget.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, callParameterTypes.ToArray(), null);
                            }


                            if (methodToCall == null)
                            {
                                throw new InvalidOperationException("Method not found");
                            }
                            generator.Emit(OpCodes.Newobj, methodToCall);
                            expressionResultType = callTarget;
                        }
                        else
                        {
                            ;
                        }
                    }
                    break;
                case VariableAccessExpression varAccess:
                    {
                        Type? callTarget = null;
                        WriteExpression(generator, store, varAccess.Expression, true, ref callTarget);

                        if (callTarget == null)
                        {
                            throw new InvalidOperationException("No target for field access");
                        }

                        FieldInfo? methodToCall = null;

                        if (store.GettingCompiledFields!.TryGetValue(callTarget, out var localMethodList))
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
                        else
                        {
                            var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
                            methodToCall = callTarget.GetField(varAccess.Name, bindingFlags);
                        }

                        if (methodToCall == null)
                        {
                            throw new InvalidOperationException("Field target not found");
                        }

                        if (isRight)
                        {

                            generator.Emit(OpCodes.Ldfld, methodToCall);
                        }
                        else
                        {
                            generator.Emit(OpCodes.Stfld, methodToCall);
                        }
                        expressionResultType = methodToCall.FieldType;
                    }
                    break;
                default:
                    throw new InvalidOperationException("Not implemented expression type");
            }
        }

    }
}
