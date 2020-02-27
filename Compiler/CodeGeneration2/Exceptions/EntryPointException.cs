using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.CodeGeneration2.Exceptions
{
    public class EntryPointException : Exception
    {
        public EntryPointException(string message) : base(message)
        {
        }

        public EntryPointException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public EntryPointException()
        {
        }
    }
}
