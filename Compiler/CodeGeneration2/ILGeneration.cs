using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Compiler.Parser.Nodes;
using Compiler.Parser.Nodes.Statements;

using static Compiler.TypeChecker.SimpleTypeChecker;

namespace Compiler.CodeGeneration2
{
    public class CurrentMethodInfo
    {
        public Type ReturnType { get; }

        public Type Type { get; }

        public bool IsStatic { get; }

        public Dictionary<string, LocalBuilder> Locals { get; } = new Dictionary<string, LocalBuilder>();

        public IReadOnlyDictionary<string, (int idx, Type type)> Parameters { get; }

        public IReadOnlyDictionary<string, FieldInfo> Fields { get; }

        public CurrentMethodInfo(Type type, Type returnType, bool isStatic,
            IReadOnlyDictionary<string, (int idx, Type type)> parameters,
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
        private readonly ILGenerator generator;
        private readonly CurrentMethodInfo currentMethodInfo;

        public ILGeneration(ILGenerator generator, CodeGenerationStore store, CurrentMethodInfo currentMethodInfo)
        {
            this.generator = generator;
            this.store = store;
            this.currentMethodInfo = currentMethodInfo;
        }

        private Action WriteLValueExpression(ExpressionSyntaxNode expression, out Type? expressionResultType)
        {
            switch (expression)
            {
                case VariableSyntaxNode varNode:
                    if (currentMethodInfo.Locals.TryGetValue(varNode.Name, out var localVar))
                    {
                        expressionResultType = localVar.LocalType;
                        return () => generator.Emit(OpCodes.Stloc, localVar);
                    }
                    else if (currentMethodInfo.Parameters.TryGetValue(varNode.Name, out var parameterVar))
                    {
                        expressionResultType = parameterVar.type;
                        return () => generator.Emit(OpCodes.Starg, parameterVar.idx);
                    }
                    else if (currentMethodInfo.Fields.TryGetValue(varNode.Name, out var fieldVar))
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
                        WriteExpression(varAccess.Expression, true, ref callTarget);

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
                        return () => generator.Emit(OpCodes.Stfld, fieldToCall);
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
            WriteExpression(callNode.Expression, true, ref expressionResultType);
            if (expressionResultType == null)
            {
                throw new InvalidOperationException("Expression must return something here");
            }
            return expressionResultType;
        }

        private void HandleVariableExpression(VariableSyntaxNode varNode, bool isRight, ref Type? expressionResultType)
        {
            {

                if (currentMethodInfo.Locals.TryGetValue(varNode.Name, out var localVar))
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
                else if (currentMethodInfo.Parameters.TryGetValue(varNode.Name, out var parameterVar))
                {
                    if (isRight)
                    {
                        generator.Emit(OpCodes.Ldarg, parameterVar.idx);
                    }
                    else
                    {
                        generator.Emit(OpCodes.Starg, parameterVar.idx);
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
                    foreach (var method in store.Methods![currentMethodInfo.Type])
                    {
                        if (method.Name != varNode.Name)
                        {
                            continue;
                        }

                        if (!method.IsStatic && currentMethodInfo.IsStatic)
                        {
                            throw new InvalidOperationException("Cannot grab a direct reference to a instance delegate");
                        }

                        var parameters = store.MethodParameters[method];

                        Type? actionType = null;
                        ConstructorBuilder? constructor = null;

                        foreach (var del in store.Delegates)
                        {
                            if (del.returnType == method.ReturnType
                                && parameters.SequenceEqual(del.parameters))
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

                        if (currentMethodInfo.IsStatic)
                        {
                            generator.Emit(OpCodes.Ldnull);
                        }
                        else
                        {
                            generator.Emit(OpCodes.Ldarg_0);
                        }
                        generator.Emit(OpCodes.Ldftn, method);
                        generator.Emit(OpCodes.Newobj, constructor);
                        expressionResultType = actionType;
                        return;
                        ;
                    }

                    throw new InvalidOperationException("Not supported");
                }


            }
        }

        private void WriteExpression(ExpressionSyntaxNode? expression, bool isRight,
  ref Type? expressionResultType)
        {
            GC.KeepAlive(expressionResultType);
            GC.KeepAlive(isRight);
            if (expression == null)
            {
                return;
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
                    HandleVariableExpression(varNode, isRight, ref expressionResultType);
                    break;
                default:
                    throw new InvalidOperationException("Expression not supported");
            }


        }

        public bool WriteStatement(StatementSyntaxNode statement)
        {
            Type? expressionResultType = null;

            switch (statement)
            {
                case ReturnStatementNode ret:
                    WriteExpression(ret.Expression, true, ref expressionResultType);
                    TypeCheck(currentMethodInfo.ReturnType, expressionResultType);
                    generator.Emit(OpCodes.Ret);
                    return true;
                case VariableDeclarationNode vardec:
                    {
                        WriteExpression(vardec.Expression, true, ref expressionResultType);
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
                        generator.Emit(OpCodes.Stloc, loc);
                    }
                    break;
                case ExpressionEqualsExpressionSyntaxNode expEqualsExp:
                    {
                        Type? rightType = null;

                        var lastOp = WriteLValueExpression(expEqualsExp.Left, out var leftType);

                        WriteExpression(expEqualsExp.Right, true, ref rightType);
                        TypeCheck(leftType, rightType);

                        lastOp();
                    }
                    break;
                case ExpressionSyntaxNode expStatement:
                    WriteExpression(expStatement, false, ref expressionResultType);
                    if (expressionResultType != null && expressionResultType != typeof(void))
                    {
                        throw new NotSupportedException("Stack must be emptied");
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

        public void EmitRet()
        {
            generator.Emit(OpCodes.Ret);
        }
    }
}
