using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Tokenizer.Tokens
{
    public class StringConstantToken : IToken
    {
        public string Value { get; }

        public StringConstantToken(string value)
        {
            Value = value;
        }
    }
}
