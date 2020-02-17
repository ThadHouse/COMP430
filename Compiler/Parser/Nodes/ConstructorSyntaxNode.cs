using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes
{
    public class ConstructorSyntaxNode : ISyntaxNode
    {
        public ISyntaxNode Parent { get; }

        public ConstructorSyntaxNode(ISyntaxNode parent)
        {
            Parent = parent;
        }
    }
}
