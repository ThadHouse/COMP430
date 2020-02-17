using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Tokenizer.Exceptions
{
    public class UnsupportedSurrogatePairEscapeException : Exception
    {
        public UnsupportedSurrogatePairEscapeException(string message) : base(message)
        {
        }

        public UnsupportedSurrogatePairEscapeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public UnsupportedSurrogatePairEscapeException()
        {
        }
    }
}
