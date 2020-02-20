using System;
using System.Collections.Generic;
using System.Text;
using Compiler.Parser.Nodes.Statements;
using Compiler.Tokenizer;

namespace Compiler.Parser.Nodes
{
    public delegate ExpressionSyntaxNode ExpressionGetter(ref ReadOnlySpan<IToken> tokens, ISyntaxNode parent);

    public class ExpressionSyntaxNode : StatementSyntaxNode
    {

        public ExpressionSyntaxNode(ISyntaxNode parent)
            : base(parent, Array.Empty<StatementSyntaxNode>())
        {
        }
    }
}
