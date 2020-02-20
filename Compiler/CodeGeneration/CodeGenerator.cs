using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Compiler.Parser.Nodes;
using Compiler.Parser.Nodes.Statements;

namespace Compiler.CodeGeneration
{
    public class CodeGenerator : ICodeGenerator
    {
        private readonly Type[] bclTypes = typeof(object).Assembly.GetTypes();

        private readonly Type[] delegateConstructorTypes = new Type[] { typeof(object), typeof(IntPtr) };

        private ConstructorBuilder GenerateDelegateFunctions(TypeBuilder type, DelegateSyntaxNode syntaxNode, Dictionary<string, Type> legalTypes)
        {
            var parameterTypes = new Type[syntaxNode.Parameters.Count];

            for (int i = 0; i < parameterTypes.Length; i++)
            {
                var paramType = legalTypes[syntaxNode.Parameters[i].Type];
                if (syntaxNode.Parameters[i].IsRef)
                {
                    paramType = paramType.MakeByRefType();
                }
                parameterTypes[i] = paramType;
            }

            var method = type.DefineMethod("Invoke", MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot, legalTypes[syntaxNode.ReturnType], parameterTypes);
            method.SetImplementationFlags(MethodImplAttributes.Runtime);

            for (int i = 0; i < parameterTypes.Length; i++)
            {
                method.DefineParameter(i + 1, ParameterAttributes.None, syntaxNode.Parameters[i].Name);
            }

            var constructor = type.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig, CallingConventions.Standard, delegateConstructorTypes);
            constructor.SetImplementationFlags(MethodImplAttributes.Runtime);
            return constructor;
        }

        private (IReadOnlyList<(MethodBuilder builder, MethodSyntaxNode syntax, GenerationStore store)> methods,
            IReadOnlyList<(ConstructorBuilder builder, ConstructorSyntaxNode syntax, GenerationStore store)> constructors,
            IReadOnlyList<FieldBuilder> fields)
            GenerateClassDefinitions(TypeBuilder type, ClassSyntaxNode syntaxNode, IReadOnlyDictionary<string, Type> legalTypes,
            IReadOnlyList<(DelegateSyntaxNode syntax, Type type, ConstructorBuilder construcotr)> delegates, ref MethodInfo? entryPoint)
        {
            var fields = new Dictionary<string, FieldBuilder>();
            var fieldBuilders = new List<FieldBuilder>();
            var fieldsToConstructInConstructor = new List<StatementSyntaxNode>();

            var toCompileMethodList = new List<(MethodBuilder builder, MethodSyntaxNode syntax, GenerationStore store)>();
            var toCompileConstructorsList = new List<(ConstructorBuilder builder, ConstructorSyntaxNode syntax, GenerationStore store)>();

            foreach (var field in syntaxNode.Fields)
            {
                var fieldType = legalTypes[field.Type];
                var fieldBuilder = type.DefineField(field.Name, fieldType, FieldAttributes.Public);
                fieldBuilders.Add(fieldBuilder);

                fields.Add(field.Name, fieldBuilder);

                if (field.Expression != null)
                {
                    fieldsToConstructInConstructor.Add(new ExpressionEqualsExpressionSyntaxNode(syntaxNode, new VariableSyntaxNode(syntaxNode, field.Name), field.Expression));
                }
            }

            fieldsToConstructInConstructor.Add(new BaseClassConstructorSyntax(syntaxNode));

            var constructorsToGenerate = new List<ConstructorSyntaxNode>();

            if (syntaxNode.Constructors.Count == 0)
            {
                // Add a default constructor
                constructorsToGenerate.Add(new ConstructorSyntaxNode(syntaxNode, Array.Empty<ParameterDefinitionSyntaxNode>(), fieldsToConstructInConstructor));
            }
            else
            {
                foreach (var cnode in syntaxNode.Constructors)
                {
                    var statements = new List<StatementSyntaxNode>(fieldsToConstructInConstructor);
                    statements.AddRange(cnode.Statements);
                    constructorsToGenerate.Add(new ConstructorSyntaxNode(syntaxNode, cnode.Parameters, statements));
                }
            }

            foreach (var methodNode in constructorsToGenerate)
            {
                var parameters = methodNode.Parameters.Select(x =>
                {
                    var tpe = legalTypes[x.Type];
                    if (x.IsRef)
                    {
                        tpe = tpe.MakeByRefType();
                    }
                    return tpe;
                }).ToArray();

                var method = type.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, parameters);

                var parametersInfo = new Dictionary<string, int>();
                var parameterTypes = new Dictionary<int, Type>();

                for (int i = 0; i < methodNode.Parameters.Count; i++)
                {
                    var p = method.DefineParameter(i + 1, ParameterAttributes.None, methodNode.Parameters[i].Name);
                    parametersInfo.Add(methodNode.Parameters[i].Name, i + 1);
                    parameterTypes.Add(i + 1, parameters[i]);
                }

                var genStore = new GenerationStore(type, false, fields, parametersInfo, legalTypes, parameterTypes, delegates, null);
                toCompileConstructorsList.Add((method, methodNode, genStore));
            }

            foreach (var methodNode in syntaxNode.Methods)
            {
                var methodAttributes = MethodAttributes.Public;
                if (methodNode.IsStatic)
                {
                    methodAttributes |= MethodAttributes.Static;
                }

                var parameters = methodNode.Parameters.Select(x =>
                {
                    var tpe = legalTypes[x.Type];
                    if (x.IsRef)
                    {
                        tpe = tpe.MakeByRefType();
                    }
                    return tpe;
                }).ToArray();

                var returnType = legalTypes[methodNode.ReturnType];

                var method = type.DefineMethod(methodNode.Name, methodAttributes, returnType, parameters);

                if (methodNode.IsEntryPoint)
                {
                    if (entryPoint != null)
                    {
                        throw new InvalidOperationException("Can only have 1 entry point");
                    }
                    entryPoint = method;
                }

                int offset = 0;
                if (methodNode.IsStatic)
                {
                    offset = 1;
                }

                var parametersInfo = new Dictionary<string, int>();
                var parameterTypes = new Dictionary<int, Type>();

                for (int i = 0; i < methodNode.Parameters.Count; i++)
                {
                    var p = method.DefineParameter(i + 1 + offset, ParameterAttributes.None, methodNode.Parameters[i].Name);
                    parametersInfo.Add(methodNode.Parameters[i].Name, i + 1 + offset);
                    parameterTypes.Add(i + 1 + offset, parameters[i]);
                }

                var genStore = new GenerationStore(type, methodNode.IsStatic, fields, parametersInfo, legalTypes, parameterTypes, delegates, returnType);
                toCompileMethodList.Add((method, methodNode, genStore));
            }

            return (toCompileMethodList, toCompileConstructorsList, fieldBuilders);
        }

        private void GenerateMethods((MethodBuilder builder, MethodSyntaxNode syntax, GenerationStore store) data)
        {
            var generator = data.builder.GetILGenerator();

            bool wasLastReturn = false;

            if (data.syntax.Name == "testFunc")
            {
                ;
            }

            foreach (var stmt in data.syntax.Statements)
            {
                wasLastReturn = ILGeneration.WriteStatement(generator, data.store, stmt);
            }

            if (!wasLastReturn)
            {
                if (data.syntax.ReturnType != typeof(void).FullName)
                {
                    throw new InvalidOperationException("Implicit return can only happen with a void function");
                }
                generator.Emit(OpCodes.Ret);
            }
        }

        private void GenerateConstructors((ConstructorBuilder builder, ConstructorSyntaxNode syntax, GenerationStore store) data)
        {
            var generator = data.builder.GetILGenerator();

            bool wasLastReturn = false;

            foreach (var stmt in data.syntax.Statements)
            {
                wasLastReturn = ILGeneration.WriteStatement(generator, data.store, stmt);
            }

            if (!wasLastReturn)
            {
                generator.Emit(OpCodes.Ret);
            }
        }

        public MethodInfo? GenerateAssembly(IReadOnlyList<(TypeBuilder typeBuilder, TypeDefinitionNode syntax)> types)
        {
            if (types == null)
            {
                throw new ArgumentNullException(nameof(types));
            }

            var legalTypes = types.Select(x => (Type)x.typeBuilder).ToDictionary(x => x.FullName);

            foreach (var bcl in bclTypes)
            {
                legalTypes.Add(bcl.FullName, bcl);
            }

            MethodInfo? entryPoint = null;

            var toCompileMethods = new Dictionary<Type, IReadOnlyList<(MethodBuilder builder, MethodSyntaxNode syntax, GenerationStore store)>>();
            var toCompileConstructors = new Dictionary<Type, IReadOnlyList<(ConstructorBuilder builder, ConstructorSyntaxNode syntax, GenerationStore store)>>();
            var toCompileFields = new Dictionary<Type, IReadOnlyList<FieldBuilder>>();

            var delegates = new List<(DelegateSyntaxNode syntax, Type type, ConstructorBuilder constructor)>();

            foreach (var genTypes in types)
            {
                if (genTypes.syntax is DelegateSyntaxNode delegateNode)
                {
                    var constructor = GenerateDelegateFunctions(genTypes.typeBuilder, delegateNode, legalTypes);
                    delegates.Add((delegateNode, genTypes.typeBuilder, constructor));
                    genTypes.typeBuilder.CreateTypeInfo();
                }
                else if (genTypes.syntax is ClassSyntaxNode classNode)
                {
                    var typeMethods = GenerateClassDefinitions(genTypes.typeBuilder, classNode, legalTypes, delegates, ref entryPoint);
                    toCompileMethods.Add(genTypes.typeBuilder, typeMethods.methods);
                    toCompileConstructors.Add(genTypes.typeBuilder, typeMethods.constructors);
                    toCompileFields.Add(genTypes.typeBuilder, typeMethods.fields);
                }
            }

            foreach (var typeToCompile in toCompileMethods)
            {
                foreach (var methodToCompile in typeToCompile.Value)
                {
                    methodToCompile.store.GettingCompiledTypes = toCompileMethods;
                    methodToCompile.store.GettingCompiledTypeConstructors = toCompileConstructors;
                    methodToCompile.store.GettingCompiledFields = toCompileFields;
                    GenerateMethods(methodToCompile);
                }
            }

            foreach (var typeToCompile in toCompileConstructors)
            {
                foreach (var methodToCompile in typeToCompile.Value)
                {
                    methodToCompile.store.GettingCompiledTypes = toCompileMethods;
                    methodToCompile.store.GettingCompiledTypeConstructors = toCompileConstructors;
                    methodToCompile.store.GettingCompiledFields = toCompileFields;
                    GenerateConstructors(methodToCompile);
                }
            }

            foreach (var genTypes in types)
            {
                genTypes.typeBuilder.CreateTypeInfo();
            }

            return entryPoint;
        }
    }
}
