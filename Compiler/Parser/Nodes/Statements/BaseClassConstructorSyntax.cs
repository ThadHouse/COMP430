using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Nodes.Statements
{
    public class BaseClassConstructorSyntax : StatementSyntaxNode
    {
        public BaseClassConstructorSyntax(ISyntaxNode parent) : base(parent, Array.Empty<StatementSyntaxNode>())
        {
        }
    }
}
