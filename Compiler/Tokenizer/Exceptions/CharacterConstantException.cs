using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Tokenizer.Exceptions
{
    public class CharacterConstantException : Exception
    {
        public CharacterConstantException(string message)
            : base(message)
        {

        }
    }
}
