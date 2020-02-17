using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes
{
    public class OperationSyntaxNode : ISyntaxNode
    {
        public char Operation { get; }

        public ISyntaxNode? Parent { get; }

        public OperationSyntaxNode(ISyntaxNode parent, char operation)
        {
            Parent = parent;
            Operation = operation;
        }
    }
}
