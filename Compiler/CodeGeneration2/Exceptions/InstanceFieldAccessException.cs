using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.CodeGeneration2.Exceptions
{
    public class InstanceFieldAccessException : Exception
    {
        public InstanceFieldAccessException(string message) : base(message)
        {
        }

        public InstanceFieldAccessException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public InstanceFieldAccessException()
        {
        }
    }
}
