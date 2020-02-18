using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes
{
    public class NewConstructorExpression : ExpressionSyntaxNode
    {
        public string Name { get; }
        public IReadOnlyList<CallParameterSyntaxNode> Parameters { get; }

        public NewConstructorExpression(ISyntaxNode parent, string name, IReadOnlyList<CallParameterSyntaxNode> parameters) : base(parent)
        {
            Name = name;
            Parameters = parameters;
        }
    }
}
