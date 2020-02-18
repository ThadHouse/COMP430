using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes
{
    public class MethodCallExpression : ExpressionSyntaxNode
    {
        public ExpressionSyntaxNode Expression { get; }
        public string Name { get; }

        public IReadOnlyList<CallParameterSyntaxNode> Parameters { get; }

        public MethodCallExpression(ISyntaxNode parent, ExpressionSyntaxNode expression, string name, IReadOnlyList<CallParameterSyntaxNode> parameters) : base(parent)
        {
            Expression = expression;
            Name = name;
            Parameters = parameters;
        }
    }
}
