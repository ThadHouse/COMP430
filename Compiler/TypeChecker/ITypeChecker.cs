using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;
using Compiler.Parser.Nodes;

namespace Compiler.TypeChecker
{
    public interface ITypeChecker
    {
        public IReadOnlyList<(TypeBuilder typeBuilder, TypeDefinitionNode syntax)> GenerateTypes(RootSyntaxNode typeRoot, ModuleBuilder moduleBuilder);
    }
}
