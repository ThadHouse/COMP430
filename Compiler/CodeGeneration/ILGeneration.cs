using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Compiler.Parser.Nodes;
using Compiler.Parser.Nodes.Statements;

using static Compiler.TypeChecker.SimpleTypeChecker;

namespace Compiler.CodeGeneration
{
    public class GenerationStore
    {
        public bool IsStatic { get; }
        public IDictionary<string, LocalBuilder> Locals { get; } = new Dictionary<string, LocalBuilder>();
        public IReadOnlyDictionary<string, FieldBuilder> Fields { get; }

        public IReadOnlyDictionary<Type, IReadOnlyList<FieldBuilder>>? GettingCompiledFields { get; set; }

        public IReadOnlyDictionary<string, int> Parameters { get; }

        public IReadOnlyDictionary<string, Type> AllowedTypes { get; }

        public IReadOnlyDictionary<Type, IReadOnlyList<(MethodBuilder builder, MethodSyntaxNode syntax, GenerationStore store)>>? GettingCompiledTypes { get; set; }

        public IReadOnlyDictionary<Type, IReadOnlyList<(ConstructorBuilder builder, ConstructorSyntaxNode syntax, GenerationStore store)>>? GettingCompiledTypeConstructors { get; set; }

        public IReadOnlyDictionary<int, Type> ParameterTypes { get; }

        public Type? ReturnType { get; }

        public GenerationStore(bool isStatic, IReadOnlyDictionary<string, FieldBuilder> fields,
            IReadOnlyDictionary<string, int> parameters, IReadOnlyDictionary<string, Type> allowedTypes,
            IReadOnlyDictionary<int, Type> parameterTypes, Type? returnType)
        {
            IsStatic = isStatic;
            Fields = fields;
            Parameters = parameters;
            AllowedTypes = allowedTypes;
            ParameterTypes = parameterTypes;
            ReturnType = returnType;
        }
    }

    public static class ILGeneration
    {
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
                case ExpressionSyntaxNode expStatement:
                    WriteExpression(generator, store, expStatement, false, ref expressionResultType);
                    if (expressionResultType != null && expressionResultType != typeof(void))
                    {
                        generator.Emit(OpCodes.Pop);
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
                                generator.Emit(OpCodes.Pop);
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
                                generator.Emit(OpCodes.Pop);
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
                            throw new InvalidOperationException("Not supported");
                        }


                    }
                    break;
                case ExpressionOpExpressionSyntaxNode expOpEx:
                    {
                        Type? leftType = null;
                        Type? rightType = null;

                        if (expOpEx.Operation.Operation == '=')
                        {
                            if (expOpEx.Left is VariableSyntaxNode && !store.IsStatic)
                            {
                                generator.Emit(OpCodes.Ldarg_0);
                            }

                            WriteExpression(generator, store, expOpEx.Right, true, ref rightType);
                            WriteExpression(generator, store, expOpEx.Left, false, ref leftType);
                            TypeCheck(leftType, rightType);



                            return;
                        }

                        WriteExpression(generator, store, expOpEx.Right, true, ref rightType);
                        WriteExpression(generator, store, expOpEx.Left, true, ref leftType);
                        TypeCheck(leftType, rightType);

                        switch (expOpEx.Operation.Operation)
                        {
                            case '+':
                                CheckCanArithmaticTypeOperations(leftType, rightType);
                                generator.Emit(OpCodes.Add);
                                break;
                            case '-':
                                CheckCanArithmaticTypeOperations(leftType, rightType);
                                generator.Emit(OpCodes.Sub);
                                break;
                            case '*':
                                CheckCanArithmaticTypeOperations(leftType, rightType);
                                generator.Emit(OpCodes.Mul);
                                break;
                            case '/':
                                CheckCanArithmaticTypeOperations(leftType, rightType);
                                generator.Emit(OpCodes.Div);
                                break;
                            case '=':
                                // Do nothing, type checking was enough assuming expressions were in the right order
                                break;
                            default:
                                throw new InvalidOperationException("Unsupported operation");
                        }
                        expressionResultType = leftType;
                    }
                    ;
                    break;
                case MethodCallExpression methodCall:
                    if (methodCall.Expression is VariableSyntaxNode vdn)
                    {
                        bool isStatic = true;
                        if (!store.AllowedTypes.TryGetValue(vdn.Name, out var callTarget))
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
                                if (localMethod.syntax.Name == methodCall.Name && (localMethod.builder.IsStatic && isStatic))
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

                        generator.Emit(OpCodes.Ldfld, methodToCall);
                        expressionResultType = methodToCall.FieldType;

                        ;
                    }
                    break;
                default:
                    throw new InvalidOperationException("Not implemented expression type");
            }
        }

    }
}
