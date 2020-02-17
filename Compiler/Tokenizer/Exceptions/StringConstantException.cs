using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Tokenizer.Exceptions
{
    public class StringConstantException : Exception
    {
        public StringConstantException(string message)
            : base(message)
        {

        }

        public StringConstantException()
        {
        }

        public StringConstantException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
