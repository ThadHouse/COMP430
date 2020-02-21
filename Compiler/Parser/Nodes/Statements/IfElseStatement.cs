using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes.Statements
{
    public class IfElseStatement : StatementSyntaxNode
    {
        public ExpressionSyntaxNode Expression { get; }

        public IReadOnlyList<StatementSyntaxNode> ElseStatements { get; }

        public IfElseStatement(ISyntaxNode parent, ExpressionSyntaxNode expression, IReadOnlyList<StatementSyntaxNode> statements,
            IReadOnlyList<StatementSyntaxNode> elseStatements)
            : base(parent, statements)
        {
            Expression = expression;
            ElseStatements = elseStatements;
        }
    }
}
