using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes
{
    public interface ISyntaxNode
    {
        ISyntaxNode? Parent { get; }
    }
}
