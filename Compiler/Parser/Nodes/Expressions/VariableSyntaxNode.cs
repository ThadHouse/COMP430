using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes
{
    public class VariableSyntaxNode : ExpressionSyntaxNode
    {
        public string Name { get; }

        public VariableSyntaxNode(ISyntaxNode parent, string name)
            : base(parent)
        {
            Name = name;
        }
    }
}
