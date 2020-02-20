using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes
{
    class VariableAccessExpression : ExpressionSyntaxNode
    {
        public ExpressionSyntaxNode Expression { get; }
        public string Name { get; }

        public VariableAccessExpression(ISyntaxNode parent, ExpressionSyntaxNode expression, string name) : base(parent)
        {
            Expression = expression;
            Name = name;
        }
    }
}
