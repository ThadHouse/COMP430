using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes
{
    public class ImmutableRootSyntaxNode
    {
        public IReadOnlyList<ImmutableClassSyntaxNode> Classes { get; }
        public IReadOnlyList<DelegateSyntaxNode> Delegates { get; }

        public ISyntaxNode? Parent => null;

        public ImmutableRootSyntaxNode(IReadOnlyList<ImmutableClassSyntaxNode> classes,
            IReadOnlyList<DelegateSyntaxNode> delegates)
        {
            Classes = classes;
            Delegates = delegates;
        }
    }
}
