using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.CodeGeneration2.Exceptions
{
    public class MethodReferenceException : Exception
    {
        public MethodReferenceException(string message) : base(message)
        {
        }

        public MethodReferenceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public MethodReferenceException()
        {
        }
    }
}
