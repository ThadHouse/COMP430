using System;
using System.Collections.Generic;
using System.Text;
using Compiler.Tokenizer;

namespace Compiler.Parser.Nodes
{
    public class MethodSyntaxNode : ISyntaxNode
    {
        public bool IsStatic { get; }

        public string Type { get; }
        public string Name { get; }

        public ExpressionSyntaxNode? Expression { get; }

        public ISyntaxNode Parent { get; }

        public MethodSyntaxNode(ISyntaxNode parent, string type, string name, ExpressionGetter? expressionGetter, ref ReadOnlySpan<IToken> tokens, bool isStatic)
        {
            Parent = parent;
            Type = type;
            Name = name;
            IsStatic = isStatic;
            Expression = expressionGetter?.Invoke(ref tokens, this);
        }
    }
}
