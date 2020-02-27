using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.CodeGeneration2.Exceptions
{
    public class RefTypeException : Exception
    {
        public RefTypeException(string message) : base(message)
        {
        }

        public RefTypeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public RefTypeException()
        {
        }
    }
}
