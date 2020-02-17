using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes
{
    public class NullConstantNode : ExpressionSyntaxNode
    {
        public NullConstantNode(ISyntaxNode parent) : base(parent)
        {
        }
    }
}
