using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes
{
    public class TrueConstantNode : ExpressionSyntaxNode
    {
        public TrueConstantNode(ISyntaxNode parent) : base(parent)
        {
        }
    }
}
