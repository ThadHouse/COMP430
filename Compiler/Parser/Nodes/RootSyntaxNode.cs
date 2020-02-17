using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes
{
    public class RootSyntaxNode : ISyntaxNode
    {
        public IList<ClassSyntaxNode> Classes { get; } = new List<ClassSyntaxNode>();

        public ISyntaxNode? Parent => null;
    }
}
