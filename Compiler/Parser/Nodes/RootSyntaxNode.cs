using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compiler.Parser.Nodes
{
    public class RootSyntaxNode : ISyntaxNode
    {
        public List<ClassSyntaxNode> Classes { get; } = new List<ClassSyntaxNode>();
        public List<DelegateSyntaxNode> Delegates { get; } = new List<DelegateSyntaxNode>();

        public ISyntaxNode? Parent => null;

        public ImmutableRootSyntaxNode ToImmutableNode()
        {
            return new ImmutableRootSyntaxNode(Classes.Select(x => x.ToImmutableNode()).ToArray(),
                Delegates);
        }
    }
}
