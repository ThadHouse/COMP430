using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes
{
    public class StringConstantNode : ExpressionSyntaxNode
    {
        public string Value { get; }

        public StringConstantNode(ISyntaxNode parent, string value)
            : base(parent)
        {
            Value = value;
        }
    }
}
