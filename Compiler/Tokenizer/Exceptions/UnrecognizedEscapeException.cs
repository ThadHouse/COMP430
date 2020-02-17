using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Tokenizer.Exceptions
{
    public class UnrecognizedEscapeException : Exception
    {
        public UnrecognizedEscapeException(string message) : base(message)
        {
        }

        public UnrecognizedEscapeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public UnrecognizedEscapeException()
        {
        }
    }
}
