using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes
{
    public class NewArrExpression : ExpressionSyntaxNode
    {
        public string Name { get; }
        public ExpressionSyntaxNode Expression { get; }

        public NewArrExpression(ISyntaxNode parent, string name, ExpressionSyntaxNode expression) : base(parent)
        {
            Name = name;
            Expression = expression;
        }
    }
}
