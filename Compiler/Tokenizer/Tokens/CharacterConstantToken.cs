using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Tokenizer.Tokens
{
    public class CharacterConstantToken : IConstantToken
    {
        public char Value { get; }
        public CharacterConstantToken(char value)
        {
            Value = value;
        }
    }
}
