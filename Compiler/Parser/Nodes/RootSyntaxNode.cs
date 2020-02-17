using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes
{
    public class RootSyntaxNode : ISyntaxNode
    {
        public IList<ClassSyntaxNode> Classes { get; } = new List<ClassSyntaxNode>();
        public IList<DelegateSyntaxNode> Delegates { get; } = new List<DelegateSyntaxNode>();

        public ISyntaxNode? Parent => null;
    }
}
