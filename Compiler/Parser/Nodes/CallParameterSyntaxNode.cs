using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes
{
    public class CallParameterSyntaxNode : ISyntaxNode
    {
        public ExpressionSyntaxNode Expression { get; }
        public bool IsRef { get; }

        public ISyntaxNode? Parent { get; }

        public CallParameterSyntaxNode(ISyntaxNode parent, ExpressionSyntaxNode expression, bool isRef)
        {
            Parent = parent;
            Expression = expression;
            IsRef = isRef;
        }
    }
}
