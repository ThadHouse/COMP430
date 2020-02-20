using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Compiler.Parser.Nodes;

namespace Compiler.CodeGeneration
{
    public class CodeGenerator : ICodeGenerator
    {
        private readonly Type[] bclTypes = typeof(object).Assembly.GetTypes();


        private void GenerateDelegateFunctions(TypeBuilder type, DelegateSyntaxNode syntaxNode, Dictionary<string, Type> legalTypes)
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
        }

        private IReadOnlyList<(MethodBuilder builder, MethodSyntaxNode syntax, GenerationStore store)>
            GenerateClassDefinitions(TypeBuilder type, ClassSyntaxNode syntaxNode, IReadOnlyDictionary<string, Type> legalTypes,
            ref MethodInfo? entryPoint)
        {
            var fields = new Dictionary<string, FieldBuilder>();
            var fieldsToConstructInConstructor = new List<(FieldBuilder fieldBuilder, ExpressionSyntaxNode expression)>();

            var toCompileMethodList = new List<(MethodBuilder builder, MethodSyntaxNode syntax, GenerationStore store)>();

            foreach (var field in syntaxNode.Fields)
            {
                var fieldType = legalTypes[field.Type];
                var fieldBuilder = type.DefineField(field.Name, fieldType, FieldAttributes.Public);

                fields.Add(field.Name, fieldBuilder);

                if (field.Expression != null)
                {
                    fieldsToConstructInConstructor.Add((fieldBuilder, field.Expression));
                }
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

                var genStore = new GenerationStore(methodNode.IsStatic, fields, parametersInfo, legalTypes, parameterTypes, returnType);
                toCompileMethodList.Add((method, methodNode, genStore));
            }

            return toCompileMethodList;
        }

        private void GenerateMethods((MethodBuilder builder, MethodSyntaxNode syntax, GenerationStore store) data,
            IReadOnlyDictionary<Type, IReadOnlyList<(MethodBuilder builder, MethodSyntaxNode syntax, GenerationStore store)>> methodsBeingCompiled)
        {
            var generator = data.builder.GetILGenerator();

            bool wasLastReturn = false;

            data.store.GettingCompiledTypes = methodsBeingCompiled;

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

            foreach (var genTypes in types)
            {
                if (genTypes.syntax is DelegateSyntaxNode delegateNode)
                {
                    GenerateDelegateFunctions(genTypes.typeBuilder, delegateNode, legalTypes);
                    genTypes.typeBuilder.CreateTypeInfo();
                }
                else if (genTypes.syntax is ClassSyntaxNode classNode)
                {
                    var typeMethods = GenerateClassDefinitions(genTypes.typeBuilder, classNode, legalTypes, ref entryPoint);
                    toCompileMethods.Add(genTypes.typeBuilder, typeMethods);
                }
            }

            foreach (var typeToCompile in toCompileMethods)
            {
                foreach (var methodToCompile in typeToCompile.Value)
                {
                    GenerateMethods(methodToCompile, toCompileMethods);
                }
                ((TypeBuilder)typeToCompile.Key).CreateTypeInfo();
            }

            return entryPoint;
        }
    }
}
