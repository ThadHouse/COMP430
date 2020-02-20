using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes
{
    class MethodReferenceExpression : ExpressionSyntaxNode
    {
        public ExpressionSyntaxNode Expression { get; }
        public string Name { get; }

        public MethodReferenceExpression(ISyntaxNode parent, ExpressionSyntaxNode expression, string name) : base(parent)
        {
            Expression = expression;
            Name = name;
        }
    }
}
