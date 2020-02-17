using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Tokenizer.Tokens
{
    public class NumericConstantToken : IConstantToken
    {
        public string Value { get; }

        public NumericConstantToken(string value)
        {
            Value = value;
        }
    }
}
