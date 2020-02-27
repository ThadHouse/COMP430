using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.TypeChecker
{
    public class TypeCheckException : Exception
    {
        public TypeCheckException(string message) : base(message)
        {
        }

        public TypeCheckException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public TypeCheckException()
        {
        }
    }
}
