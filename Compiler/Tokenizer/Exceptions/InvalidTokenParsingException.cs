using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Tokenizer.Exceptions
{
#pragma warning disable CA1032 // Implement standard exception constructors
    public class InvalidTokenParsingException : Exception
#pragma warning restore CA1032 // Implement standard exception constructors
    {
        public char CauseCharacter { get; }

        public string CurrentlyParsedToken { get; }

        public InvalidTokenParsingException(char causeCharacter, ReadOnlySpan<char> currentToken)
            : base($"Invalid character {causeCharacter} with token {currentToken.ToString()}")
        {
            CauseCharacter = causeCharacter;
            CurrentlyParsedToken = currentToken.ToString();
        }
    }
}
