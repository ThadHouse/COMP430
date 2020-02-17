using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes
{
    public class DelegateSyntaxNode : ISyntaxNode
    {
        public ISyntaxNode? Parent { get; }

        public IReadOnlyList<ParameterDefinitionSyntaxNode> Parameters { get; }

        public string Name { get; }
        public string ReturnType { get; }

        public DelegateSyntaxNode(ISyntaxNode parent, IReadOnlyList<ParameterDefinitionSyntaxNode> parameters, string returnType, string name)
        {
            Name = name;
            ReturnType = returnType;
            Parent = parent;
            Parameters = parameters;
        }
    }
}
