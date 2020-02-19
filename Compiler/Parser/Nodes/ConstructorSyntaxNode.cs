using System;
using System.Collections.Generic;
using System.Text;
using Compiler.Parser.Nodes.Statements;
using Compiler.Tokenizer;

namespace Compiler.Parser.Nodes
{
    public class ConstructorSyntaxNode : ISyntaxNode
    {

        private IReadOnlyList<ParameterDefinitionSyntaxNode> Parameters { get; }

        public IReadOnlyList<StatementSyntaxNode> Statements { get; }

        public ISyntaxNode Parent { get; }

        public ConstructorSyntaxNode(ISyntaxNode parent, IReadOnlyList<ParameterDefinitionSyntaxNode> parameters, IReadOnlyList<StatementSyntaxNode> statements)
        {
            Parent = parent;
            Parameters = parameters;
            Statements = statements;
        }
    }
}
