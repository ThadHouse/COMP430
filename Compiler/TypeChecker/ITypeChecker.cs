using System;
using System.Collections.Generic;
using System.Text;
using Compiler.Parser.Nodes;

namespace Compiler.TypeChecker
{
    public interface ITypeChecker
    {
        public IReadOnlyDictionary<string, TypeDefinitionNode> TypeCheck(RootSyntaxNode typeRoot);
    }
}
