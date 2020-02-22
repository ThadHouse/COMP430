using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Tokenizer.Tokens
{
    class DoubleConstantToken : IToken
    {
        public double Value { get; }

        public DoubleConstantToken(double value)
        {
            Value = value;
        }
    }
}
