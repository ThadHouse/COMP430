using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes
{
    public class ThisConstantNode : ExpressionSyntaxNode
    {
        public ThisConstantNode(ISyntaxNode parent) : base(parent)
        {
        }
    }
}
