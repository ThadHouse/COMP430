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

        public CharacterConstantException()
        {
        }

        public CharacterConstantException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
