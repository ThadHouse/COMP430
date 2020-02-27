using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.CodeGeneration2.Exceptions
{
    public class InvalidLValueException : Exception
    {
        public InvalidLValueException(string message) : base(message)
        {
        }

        public InvalidLValueException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public InvalidLValueException()
        {
        }
    }
}
