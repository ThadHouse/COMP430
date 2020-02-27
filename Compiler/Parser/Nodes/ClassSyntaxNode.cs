using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes
{
    public class ClassSyntaxNode : TypeDefinitionNode
    {
        public List<FieldSyntaxNode> Fields { get; } = new List<FieldSyntaxNode>();

        public List<MethodSyntaxNode> Methods { get; } = new List<MethodSyntaxNode>();

        public List<ConstructorSyntaxNode> Constructors { get; } = new List<ConstructorSyntaxNode>();

        public string Name { get; }

        public ClassSyntaxNode(ISyntaxNode parent, string name) : base(parent)
        {
            Name = name;
        }

        public ImmutableClassSyntaxNode ToImmutableNode()
        {
            return new ImmutableClassSyntaxNode(Parent!, Name,
                Fields, Methods, Constructors);
        }
    }
}
