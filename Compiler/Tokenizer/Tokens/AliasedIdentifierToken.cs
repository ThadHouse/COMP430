using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Tokenizer.Tokens
{
    public class AliasedIdentifierToken : IdentifierToken
    {
        public string Alias { get; }

        public AliasedIdentifierToken(string name, string alias)
            : base(name)
        {
            Alias = alias;
        }

    }
}
