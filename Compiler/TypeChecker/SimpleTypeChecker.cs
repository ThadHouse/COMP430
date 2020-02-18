using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Compiler.Parser.Nodes;

namespace Compiler.TypeChecker
{
    public class SimpleTypeChecker : ITypeChecker
    {
        private readonly Type[] delegateConstructorTypes = new Type[] { typeof(object), typeof(IntPtr) };

        public IReadOnlyList<(TypeBuilder typeBuilder, TypeDefinitionNode syntax)> GenerateTypes(RootSyntaxNode typeRoot, ModuleBuilder moduleBuilder)
        {
            if (typeRoot == null)
            {
                throw new ArgumentNullException(nameof(typeRoot));
            }

            if (moduleBuilder == null)
            {
                throw new ArgumentNullException(nameof(moduleBuilder));
            }

            var generatedTypeStore = new HashSet<string>();

            var generatedTypes = new List<(TypeBuilder typeBuilder, TypeDefinitionNode syntax)>();

            var baseClassTypes = typeof(object).Assembly.GetTypes().Where(x => x.IsPublic).Select(x => x.FullName);

            foreach (var cls in typeRoot.Classes)
            {
                if (generatedTypeStore.Contains(cls.Name))
                {
                    throw new Exception("Type already exists");
                }

                generatedTypeStore.Add(cls.Name);

                var generatedType = moduleBuilder.DefineType(cls.Name, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.AutoLayout);
                generatedTypes.Add((generatedType, cls));

                // Ensure it doesn't exist in the BCL

                if (baseClassTypes.Contains(cls.Name))
                {
                    throw new Exception("Type exists in BCL");
                }
            }

            foreach (var cls in typeRoot.Delegates)
            {
                if (generatedTypeStore.Contains(cls.Name))
                {
                    throw new Exception("Type already exists");
                }

                generatedTypeStore.Add(cls.Name);

                var generatedType = moduleBuilder.DefineType(cls.Name, TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed | TypeAttributes.AutoLayout, typeof(MulticastDelegate));

                // Delegates all have the same constructor, they can be already generated.

                generatedType.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig, CallingConventions.Standard, delegateConstructorTypes)
                .SetImplementationFlags(MethodImplAttributes.Runtime);

                generatedTypes.Add((generatedType, cls));

                // Ensure it doesn't exist in the BCL

                if (baseClassTypes.Contains(cls.Name))
                {
                    throw new Exception("Type exists in BCL");
                }
            }

            return generatedTypes;
        }
    }
}
