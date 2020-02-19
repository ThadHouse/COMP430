using System;
using System.Collections.Generic;
using System.Text;
using Compiler.Tokenizer;

namespace Compiler.Parser.Nodes.Statements
{
    public class VariableDeclarationNode : StatementSyntaxNode
    {
        public string? Type { get; set; }
        public string Name { get; }

        public ExpressionSyntaxNode? Expression { get; }

        public VariableDeclarationNode(ISyntaxNode parent, string? type, string name, ExpressionGetter? expressionGetter, ref ReadOnlySpan<IToken> tokens)
            : base(parent, Array.Empty<StatementSyntaxNode>())
        {
            Type = type;
            Name = name;
            Expression = expressionGetter?.Invoke(ref tokens, this);
        }
    }
}
