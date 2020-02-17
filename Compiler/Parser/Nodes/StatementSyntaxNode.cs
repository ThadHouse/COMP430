using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes
{
    public class StatementSyntaxNode : ExpressionSyntaxNode
    {


        public StatementSyntaxNode(ISyntaxNode parent) : base(parent)
        {
        }
    }
}
