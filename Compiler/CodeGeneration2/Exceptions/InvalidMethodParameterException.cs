using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.CodeGeneration2.Exceptions
{
    public class InvalidMethodParameterException : Exception
    {
        public InvalidMethodParameterException(string message) : base(message)
        {
        }

        public InvalidMethodParameterException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public InvalidMethodParameterException()
        {
        }
    }
}
