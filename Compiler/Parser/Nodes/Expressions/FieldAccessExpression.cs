using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes
{
    public class FieldAccessExpression : ExpressionSyntaxNode
    {
        public ExpressionSyntaxNode Expression { get; }
        public string Name { get; }

        public FieldAccessExpression(ISyntaxNode parent, ExpressionSyntaxNode expression, string name) : base(parent)
        {
            Expression = expression;
            Name = name;
        }
    }
}
