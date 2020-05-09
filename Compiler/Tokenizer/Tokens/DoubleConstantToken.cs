using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Tokenizer.Tokens
{
    public class DoubleConstantToken : IToken
    {
        public double Value { get; }

        public DoubleConstantToken(double value)
        {
            Value = value;
        }
    }
}
