using System.Collections.Generic;
using Compiler.CodeGeneration2.Builders;
using Compiler.Parser.Nodes;

namespace Compiler.CodeGeneration2
{
    public class GeneratedData
    {
        public Dictionary<ITypeBuilder, ImmutableClassSyntaxNode> Classes { get; } = new Dictionary<ITypeBuilder, ImmutableClassSyntaxNode>();
        public Dictionary<ITypeBuilder, DelegateSyntaxNode> Delegates { get; } = new Dictionary<ITypeBuilder, DelegateSyntaxNode>();

        public Dictionary<IMethodBuilder, MethodSyntaxNode> Methods { get; } = new Dictionary<IMethodBuilder, MethodSyntaxNode>();

        public Dictionary<IConstructorBuilder, ConstructorSyntaxNode> Constructors { get; } = new Dictionary<IConstructorBuilder, ConstructorSyntaxNode>();
    }
}
