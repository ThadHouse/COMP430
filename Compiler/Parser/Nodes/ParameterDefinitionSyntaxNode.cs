using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes
{
    public class ParameterDefinitionSyntaxNode : ISyntaxNode
    {
        public string Type { get; }
        public string Name { get; }
        public bool IsRef { get; }

        public ISyntaxNode? Parent { get; }

        public ParameterDefinitionSyntaxNode(ISyntaxNode parent, string type, string name, bool isRef)
        {
            Parent = parent;
            Type = type;
            Name = name;
            IsRef = isRef;
        }
    }
}
