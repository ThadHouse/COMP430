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

        public IReadOnlyDictionary<string, int> Parameters { get; }

        public IReadOnlyDictionary<string, Type> AllowedTypes { get; }

        public IReadOnlyDictionary<Type, IReadOnlyList<(MethodBuilder builder, MethodSyntaxNode syntax, GenerationStore store)>>? GettingCompiledTypes { get; set; }

        public IReadOnlyDictionary<int, Type> ParameterTypes { get; }

        public Type ReturnType { get; }

        public GenerationStore(bool isStatic, IReadOnlyDictionary<string, FieldBuilder> fields,
            IReadOnlyDictionary<string, int> parameters, IReadOnlyDictionary<string, Type> allowedTypes,
            IReadOnlyDictionary<int, Type> parameterTypes, Type returnType)
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
                        else if (!store.Parameters.TryGetValue(varNode.Name, out var parameterVar))
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

                            generator.Emit(OpCodes.Ldarg_0);

                            if (isRight)
                            {
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
                        if (store.AllowedTypes.TryGetValue(vdn.Name, out var callTarget))
                        {
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
                                    if (localMethod.syntax.Name == methodCall.Name && localMethod.builder.IsStatic)
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
                                methodToCall = callTarget.GetMethod(methodCall.Name, BindingFlags.Public | BindingFlags.Static, null, callParameterTypes.ToArray(), null);
                            }


                            if (methodToCall == null)
                            {
                                throw new InvalidOperationException("Method not found");
                            }
                            generator.EmitCall(OpCodes.Call, methodToCall, null);
                            expressionResultType = methodToCall.ReturnType;
                        }
                        else
                        {
                            ;
                        }
                        ;
                    }
                    // methodCall.Expression

                    var nme = methodCall.Name;
                    break;
                default:
                    throw new InvalidOperationException("Not implemented expression type");
            }

            //if (expression is ExpressionOpExpressionSyntaxNode exp)
            //{
            //    //if (exp.Operation.Operation == '=')
            //    //{
            //    //    var left = exp.Left;
            //    //    if (!(left is VariableSyntaxNode varNode))
            //    //    {
            //    //        throw new InvalidOperationException("Left must be a variable");
            //    //    }

            //    //    LocalBuilder? localVar;
            //    //    FieldBuilder? fieldVar = null;

            //    //    if (!store.Locals.TryGetValue(varNode.Name, out localVar ))
            //    //    {
            //    //        if (!store.Fields.TryGetValue(varNode.Name, out fieldVar))
            //    //        {
            //    //            throw new InvalidOperationException("Field or Local not found");
            //    //        }
            //    //    }

            //    //    switch (exp.Right)
            //    //    {
            //    //        case IntConstantSyntaxNode intConstant:
            //    //            generator.Emit(OpCodes.Ldc_I4, intConstant.Value);
            //    //            break;
            //    //        case StringConstantNode stringConstant:
            //    //            generator.Emit(OpCodes.Ldstr, stringConstant.Value);
            //    //            break;
            //    //        default:
            //    //            throw new NotSupportedException("This expression type is not supported yet");
            //    //    }

            //    //    if (localVar != null)
            //    //    {
            //    //        generator.Emit(OpCodes.Stloc, localVar);
            //    //    }
            //    //    else if (fieldVar != null)
            //    //    {
            //    //        if (store.IsStatic)
            //    //        {
            //    //            throw new InvalidOperationException("Cannot store field in static function");
            //    //        }
            //    //        generator.Emit(OpCodes.Ldarg_0);
            //    //        generator.Emit(OpCodes.Stfld, fieldVar);
            //    //    }
            //    //}
            //}
            //else if (expression is IntConstantSyntaxNode)
            //{

            //}

        }
    }
}
