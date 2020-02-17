using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes
{
    public class IntConstantSyntaxNode : ExpressionSyntaxNode
    {
        public int Value { get; }

        public IntConstantSyntaxNode(ISyntaxNode parent, int value)
            : base(parent)
        {
            Value = value;
        }
    }
}
