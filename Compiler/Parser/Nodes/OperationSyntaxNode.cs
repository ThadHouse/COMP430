using System;
using System.Collections.Generic;
using System.Text;
using Compiler.Tokenizer.Tokens;

namespace Compiler.Parser.Nodes
{
    public class OperationSyntaxNode : ISyntaxNode
    {
        public SupportedOperation Operation { get; }

        public ISyntaxNode? Parent { get; }

        public OperationSyntaxNode(ISyntaxNode parent, SupportedOperation operation)
        {
            Parent = parent;
            Operation = operation;
        }
    }
}
