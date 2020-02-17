using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes
{
    public class DelegateSyntaxNode : TypeDefinitionNode
    {

        public IReadOnlyList<ParameterDefinitionSyntaxNode> Parameters { get; }

        public string Name { get; }
        public string ReturnType { get; }

        public DelegateSyntaxNode(ISyntaxNode parent, IReadOnlyList<ParameterDefinitionSyntaxNode> parameters, string returnType, string name)
            : base(parent)
        {
            Name = name;
            ReturnType = returnType;
            Parameters = parameters;
        }
    }
}
