using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Tokenizer.Exceptions
{
    public class InvalidNumericTokenException : Exception
    {
        public InvalidNumericTokenException(string message) : base(message)
        {
        }

        public InvalidNumericTokenException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public InvalidNumericTokenException()
        {
        }
    }
}
