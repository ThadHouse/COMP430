using System;
using System.Collections.Generic;
using System.Text;
using Compiler.Tokenizer;
using Compiler.Tokenizer.Tokens;

namespace Compiler.Parser.Nodes
{
    public class FieldSyntaxNode : ISyntaxNode
    {
        public string Type { get; }
        public string Name { get; }

        public ExpressionSyntaxNode? Expression { get; }

        public ISyntaxNode Parent { get; }

        public FieldSyntaxNode(ISyntaxNode parent, string type, string name, ExpressionGetter? expressionGetter, ref ReadOnlySpan<IToken> tokens)
        {
            Parent = parent;
            Type = type;
            Name = name;
            Expression = expressionGetter?.Invoke(ref tokens, this);
        }
    }
}
