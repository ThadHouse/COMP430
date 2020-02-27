using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.CodeGeneration2.Exceptions
{
    public class UnknownExpressionException : Exception
    {
        public UnknownExpressionException(string message) : base(message)
        {
        }

        public UnknownExpressionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public UnknownExpressionException()
        {
        }
    }
}
