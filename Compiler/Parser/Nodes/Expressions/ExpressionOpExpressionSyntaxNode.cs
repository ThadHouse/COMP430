using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes
{
    public class ExpressionOpExpressionSyntaxNode : ExpressionSyntaxNode
    {
        public ExpressionSyntaxNode Left { get; }
        public OperationSyntaxNode Operation { get; }
        public ExpressionSyntaxNode Right { get; }

        public ExpressionOpExpressionSyntaxNode(ISyntaxNode parent, ExpressionSyntaxNode left, OperationSyntaxNode operation, ExpressionSyntaxNode right)
            : base(parent)
        {
            Left = left;
            Operation = operation;
            Right = right;
        }
    }
}
