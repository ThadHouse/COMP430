using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes
{
    public abstract class TypeDefinitionNode : ISyntaxNode
    {
        public ISyntaxNode? Parent { get; }

        public TypeDefinitionNode(ISyntaxNode parent)
        {
            Parent = parent;
        }
    }
}
