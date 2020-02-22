using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Tokenizer.Exceptions
{
#pragma warning disable CA1032 // Implement standard exception constructors
    public class InvalidTokenParsingException : Exception
#pragma warning restore CA1032 // Implement standard exception constructors
    {

        public InvalidTokenParsingException(string message)
            : base(message)
        {
        }
    }
}
