using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Compiler.CodeGeneration2.Builders;
using Compiler.Parser.Nodes;

namespace Compiler.TypeChecker
{
    public class SimpleTypeChecker : ITypeChecker
    {
        public static IType? VoidType { get; set; }

        public static void CheckCanArithmaticTypeOperations(IType? leftType, IType? rightType)
        {
            if (leftType == null)
            {
                throw new ArgumentNullException(nameof(leftType), "Left type really cannot be null here");
            }

            if (rightType == null)
            {
                throw new ArgumentNullException(nameof(rightType), "Right type really cannot be null here");
            }

            if (rightType.FullName != leftType.FullName)
            {
                throw new InvalidOperationException($"Types must be equal: {leftType.FullName} - {rightType.FullName}");
            }

            if (leftType.FullName != typeof(int).FullName)
            {
                throw new InvalidOperationException($"Cannot perform arithmatic on {leftType.FullName}");
            }
        }

        public static void TypeCheck(IType? leftType, IType? rightType)
        {
            if (leftType == null)
            {
                throw new ArgumentNullException(nameof(leftType), "Left type really cannot be null here");
            }

            // A null right type is void
            if (rightType == null)
            {
                if (VoidType == null)
                {
                    throw new InvalidOperationException("Void type must be set");
                }
                rightType = VoidType;
            }

            if (!leftType.IsAssignableFrom(rightType))
            {
                throw new InvalidOperationException($"Invalid Type Assignment, attempting to assign {rightType.FullName} to {leftType.FullName}");
            }
        }

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
