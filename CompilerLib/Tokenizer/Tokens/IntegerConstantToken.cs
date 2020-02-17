using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Tokenizer.Tokens
{
    public class IntegerConstantToken : IToken
    {
        public int Value { get; }

        public IntegerConstantToken(int value)
        {
            Value = value;
        }
    }
}
