using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.CodeGeneration2.Exceptions
{
    public class LValueOperationException : Exception
    {
        public LValueOperationException(string message) : base(message)
        {
        }

        public LValueOperationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public LValueOperationException()
        {
        }
    }
}
