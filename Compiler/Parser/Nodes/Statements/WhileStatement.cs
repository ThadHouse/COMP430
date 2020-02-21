using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes.Statements
{
    public class WhileStatement : StatementSyntaxNode
    {
        public ExpressionSyntaxNode Expression { get; }

        public WhileStatement(ISyntaxNode parent, ExpressionSyntaxNode expression, IReadOnlyList<StatementSyntaxNode> statements)
            : base(parent, statements)
        {
            Expression = expression;
        }
    }
}
