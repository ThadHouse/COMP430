using System;
using System.Collections.Generic;
using System.Text;
using Compiler.Tokenizer;

namespace Compiler.Parser.Nodes.Statements
{
    public class ReturnStatementNode : StatementSyntaxNode
    {
        public ExpressionSyntaxNode? Expression { get; }

        public ReturnStatementNode(ISyntaxNode parent, ExpressionGetter? expressionGetter, ref ReadOnlySpan<IToken> tokens)
            : base(parent, Array.Empty<StatementSyntaxNode>())
        {
            Expression = expressionGetter?.Invoke(ref tokens, this);
        }
    }
}
