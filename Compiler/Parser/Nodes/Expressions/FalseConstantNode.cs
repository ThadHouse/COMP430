using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes
{
    public class FalseConstantNode : ExpressionSyntaxNode
    {
        public FalseConstantNode(ISyntaxNode parent) : base(parent)
        {
        }
    }
}
