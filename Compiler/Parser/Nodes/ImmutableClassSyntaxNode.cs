using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes
{
    public class ImmutableClassSyntaxNode : TypeDefinitionNode
    {
        public IReadOnlyList<FieldSyntaxNode> Fields { get; }

        public IReadOnlyList<MethodSyntaxNode> Methods { get; }

        public IReadOnlyList<ConstructorSyntaxNode> Constructors { get; }

        public string Name { get; }

        public ImmutableClassSyntaxNode(ISyntaxNode parent, string name,
            IReadOnlyList<FieldSyntaxNode> fields,
            IReadOnlyList<MethodSyntaxNode> methods,
            IReadOnlyList<ConstructorSyntaxNode> constructors) : base(parent)
        {
            Name = name;
            Fields = fields;
            Methods = methods;
            Constructors = constructors;
        }

        public ImmutableClassSyntaxNode MutateConstructors(IReadOnlyList<ConstructorSyntaxNode> newConstructors)
        {
            return new ImmutableClassSyntaxNode(Parent!, Name, Fields, Methods, newConstructors);
        }
    }
}
