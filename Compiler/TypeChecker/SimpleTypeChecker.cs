using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Compiler.Parser.Nodes;

namespace Compiler.TypeChecker
{
    public class SimpleTypeChecker : ITypeChecker
    {
        public static readonly IReadOnlyList<string> BuiltInTypes = new List<string>
        {
            "int",
            "string",
            "bool"
        };

        public static IReadOnlyDictionary<string, TypeDefinitionNode> EnumerateTypes(RootSyntaxNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            var asm = typeof(object).Assembly.GetTypes().Where(x => x.IsPublic).Select(x => x.FullName.Replace(".", "::"));

            var types = new Dictionary<string, TypeDefinitionNode>();

            foreach (var cls in node.Classes)
            {
                if (types.ContainsKey(cls.Name))
                {
                    throw new Exception("Type already exists");
                }

                types.Add(cls.Name, cls);

                // Ensure it doesn't exist in the BCL

                if (asm.Contains(cls.Name))
                {
                    throw new Exception("Type exists in BCL");
                }
            }

            foreach (var cls in node.Delegates)
            {
                if (types.ContainsKey(cls.Name))
                {
                    throw new Exception("Type already exists");
                }

                types.Add(cls.Name, cls);

                // Ensure it doesn't exist in the BCL

                if (asm.Contains(cls.Name))
                {
                    throw new Exception("Type exists in BCL");
                }
            }

            return types;
        }

        public IReadOnlyDictionary<string, TypeDefinitionNode> TypeCheck(RootSyntaxNode typeRoot)
        {
            var types = EnumerateTypes(typeRoot);

            return types;
            ;
        }
    }
}
