using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Tokenizer.Tokens
{
    public class IdentifierToken : IToken
    {
        public string Name { get; }

        public IdentifierToken(string name)
        {
            Name = name;
        }
    }
}
