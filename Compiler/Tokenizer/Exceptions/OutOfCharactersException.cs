using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Tokenizer.Exceptions
{
    public class OutOfCharactersException : Exception
    {
        public OutOfCharactersException(string message) : base(message)
        {
        }

        public OutOfCharactersException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public OutOfCharactersException()
        {
        }
    }
}
