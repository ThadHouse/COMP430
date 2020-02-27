using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.CodeGeneration2.Exceptions
{
    public class MissingTypeException : Exception
    {
        public MissingTypeException(string message) : base(message)
        {
        }

        public MissingTypeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public MissingTypeException()
        {
        }
    }
}
