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
        private TypeBuilder GenerateDelegateType(ModuleBuilder moduleBuilder, DelegateSyntaxNode delegateToMake)
        {
            var type = moduleBuilder.DefineType(delegateToMake.Name.Replace("::", "."),
                    TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.AutoLayout, typeof(MulticastDelegate));

            type.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig, CallingConventions.Standard, delegateConstructorTypes)
                .SetImplementationFlags(MethodImplAttributes.Runtime);

            return type;
        }

        private readonly Type[] bclTypes = typeof(object).Assembly.GetTypes();
        private readonly Type[] delegateConstructorTypes = new Type[] { typeof(object), typeof(IntPtr) };

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

        public void GenerateAssembly(IReadOnlyDictionary<string, TypeDefinitionNode> types, ModuleBuilder moduleBuilder)
        {
            if (types == null)
            {
                throw new ArgumentNullException(nameof(types));
            }

            if (moduleBuilder == null)
            {
                throw new ArgumentNullException(nameof(moduleBuilder));
            }

            var generatedTypes = new List<(TypeDefinitionNode syntaxNode, TypeBuilder typeBuilder)>();

            foreach (var type in types)
            {
                if (type.Value is DelegateSyntaxNode delegateNode)
                {
                    var typeBuilder = GenerateDelegateType(moduleBuilder, delegateNode);
                    generatedTypes.Add((delegateNode, typeBuilder));
                }
            }

            var legalTypes = generatedTypes.Select(x => (Type)x.typeBuilder).ToDictionary(x => x.FullName);
            foreach (var bcl in bclTypes)
            {
                legalTypes.Add(bcl.FullName, bcl);
            }

            foreach (var genTypes in generatedTypes)
            {
                if (genTypes.syntaxNode is DelegateSyntaxNode delegateNode)
                {
                    GenerateDelegateFunctions(genTypes.typeBuilder, delegateNode, legalTypes);
                    genTypes.typeBuilder.CreateTypeInfo();
                }
            }
        }
    }
}
