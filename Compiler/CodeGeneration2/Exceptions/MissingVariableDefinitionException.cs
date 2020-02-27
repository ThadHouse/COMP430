using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.CodeGeneration2.Exceptions
{
    public class MissingVariableDefinitionException : Exception
    {
        public MissingVariableDefinitionException(string message) : base(message)
        {
        }

        public MissingVariableDefinitionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public MissingVariableDefinitionException()
        {
        }
    }
}
