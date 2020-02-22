using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes
{
    public class ArrayIndexExpression : ExpressionSyntaxNode
    {
        public ExpressionSyntaxNode Expression { get; }

        public ExpressionSyntaxNode LengthExpression { get; }

        public ArrayIndexExpression(ISyntaxNode parent, ExpressionSyntaxNode expression, ExpressionSyntaxNode lengthExpression) : base(parent)
        {
            Expression = expression;
            LengthExpression = lengthExpression;
        }
    }
}
