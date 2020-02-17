using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes
{
    public class TypeSyntaxNode : ISyntaxNode
    {
        public string Name { get; }

        public ISyntaxNode? Parent { get; }

        public TypeSyntaxNode(ISyntaxNode parent, string name)
        {
            Parent = parent;
            Name = name;
        }
    }
}
