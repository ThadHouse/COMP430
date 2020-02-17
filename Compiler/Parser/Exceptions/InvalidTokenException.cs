using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Parser.Exceptions
{
    public class InvalidTokenException : Exception
    {
        public InvalidTokenException(string message) : base(message)
        {
        }

        public InvalidTokenException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public InvalidTokenException()
        {
        }
    }
}
