using System;
using System.Collections.Generic;
using System.Text;
using Compiler.Parser.Nodes.Statements;
using Compiler.Tokenizer;

namespace Compiler.Parser.Nodes
{
    public class MethodSyntaxNode : ISyntaxNode
    {
        public bool IsStatic { get; }

        public bool IsEntryPoint { get; }

        public string ReturnType { get; }

        public IReadOnlyList<ParameterDefinitionSyntaxNode> Parameters { get; }

        public string Name { get; }

        public IReadOnlyList<StatementSyntaxNode> Statements { get; }

        public ISyntaxNode Parent { get; }

        public MethodSyntaxNode(ISyntaxNode parent, string returnType, string name, IReadOnlyList<ParameterDefinitionSyntaxNode> parameters, bool isStatic, bool isEntryPoint, IReadOnlyList<StatementSyntaxNode> statements)
        {
            Parent = parent;
            ReturnType = returnType;
            Parameters = parameters;
            Statements = statements;
            Name = name;
            IsStatic = isStatic;
            IsEntryPoint = isEntryPoint;
        }
    }
}
