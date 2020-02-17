using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes
{
    public class DelegateSyntaxNode : ISyntaxNode
    {
        public ISyntaxNode? Parent { get; }

        public DelegateSyntaxNode(ISyntaxNode parent)
        {
            Parent = parent;
        }
    }
}
