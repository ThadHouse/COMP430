using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes.Statements
{
    public class StatementSyntaxNode : ISyntaxNode
    {
        public ISyntaxNode? Parent { get; }

        public IReadOnlyList<StatementSyntaxNode> Statements { get; }

        public StatementSyntaxNode(ISyntaxNode parent, IReadOnlyList<StatementSyntaxNode> statements)
        {
            Parent = parent;
            Statements = statements;
        }
    }
}
