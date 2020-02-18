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

        private void GenerateClass(TypeBuilder type, ClassSyntaxNode syntaxNode, Dictionary<string, Type> legalTypes)
        {
            var fieldsToConstructInConstructor = new List<(FieldBuilder fieldBuilder, ExpressionSyntaxNode expression)>();

            foreach (var field in syntaxNode.Fields)
            {
                var fieldType = legalTypes[field.Type];
                var fieldBuilder = type.DefineField(field.Name, fieldType, FieldAttributes.Public);

                if (field.Expression != null)
                {
                    fieldsToConstructInConstructor.Add((fieldBuilder, field.Expression));
                }
            }

            if (syntaxNode.Constructors.Count == 0)
            {
            }
        }

        public void GenerateAssembly(IReadOnlyList<(TypeBuilder typeBuilder, TypeDefinitionNode syntax)> types)
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

            foreach (var genTypes in types)
            {
                if (genTypes.syntax is DelegateSyntaxNode delegateNode)
                {
                    GenerateDelegateFunctions(genTypes.typeBuilder, delegateNode, legalTypes);
                }
                else if (genTypes.syntax is ClassSyntaxNode classNode)
                {
                    GenerateClass(genTypes.typeBuilder, classNode, legalTypes);
                }
                genTypes.typeBuilder.CreateTypeInfo();
            }
        }
    }
}
