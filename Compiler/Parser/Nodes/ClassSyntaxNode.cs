using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes
{
    public class ClassSyntaxNode : ISyntaxNode
    {
        public IList<FieldSyntaxNode> Fields { get; } = new List<FieldSyntaxNode>();

        public IList<MethodSyntaxNode> Methods { get; } = new List<MethodSyntaxNode>();

        public IList<ConstructorSyntaxNode> Constructors { get; } = new List<ConstructorSyntaxNode>();

        public ISyntaxNode Parent { get; }

        public string Name { get; }

        public ClassSyntaxNode(ISyntaxNode parent, string name)
        {
            Parent = parent;
            Name = name;
        }
    }
}
