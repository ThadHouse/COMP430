using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes
{
    public class ExpressionEqualsExpressionSyntaxNode : ExpressionSyntaxNode
    {
        public ExpressionSyntaxNode Left { get; }
        public ExpressionSyntaxNode Right { get; }

        public ExpressionEqualsExpressionSyntaxNode(ISyntaxNode parent, ExpressionSyntaxNode left, ExpressionSyntaxNode right)
            : base(parent)
        {
            Left = left;
            Right = right;
        }
    }
}
