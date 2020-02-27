using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.CodeGeneration2.Exceptions
{
    public class TypeInferrenceException : Exception
    {
        public TypeInferrenceException(string message) : base(message)
        {
        }

        public TypeInferrenceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public TypeInferrenceException()
        {
        }
    }
}
