using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Tokenizer.Exceptions
{
    public class InvalidTokenParsingException : Exception
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
