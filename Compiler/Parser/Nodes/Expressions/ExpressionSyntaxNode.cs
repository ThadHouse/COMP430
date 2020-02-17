using System;
using System.Collections.Generic;
using System.Text;
using Compiler.Tokenizer;

namespace Compiler.Parser.Nodes
{
    public delegate ExpressionSyntaxNode ExpressionGetter(ref ReadOnlySpan<IToken> tokens, ISyntaxNode parent);

    public class ExpressionSyntaxNode : ISyntaxNode
    {
        public ISyntaxNode? Parent { get; }

        public ExpressionSyntaxNode(ISyntaxNode parent)
        {
            Parent = parent;
        }
    }
}
