using Compiler.Tokenizer.Exceptions;
using Compiler.Tokenizer.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compiler.Tokenizer
{
    public class Tokenizer : ITokenizer
    {
        private char[] allowedSingleCharacters = new char[]
{
            '[', ']', '{', '}', ';', '(', ')', '.', ',', '='
};

        private IToken ParseSingleCharToken(char token)
        {
            return token switch
            {
                '[' => new LeftBracketToken(),
                ']' => new RightBracketToken(),
                '{' => new LeftBraceToken(),
                '}' => new RightBraceToken(),
                '(' => new LeftParenthesisToken(),
                ')' => new RightParenthesisToken(),
                ';' => new SemiColonToken(),
                '.' => new DotToken(),
                ',' => new CommaToken(),
                '=' => new EqualsToken(),
                _ => throw new InvalidTokenParsingException(token, ReadOnlySpan<char>.Empty) // This should never be hit
            };
        }

        private IToken ParseToken(ReadOnlySpan<char> token)
        {
            var tokenString = token.ToString();
            return tokenString switch
            {
                "class" => new ClassToken(),
                "namespace" => new NamespaceToken(),
                "static" => new StaticToken(),
                "return" => new ReturnToken(),
                "var" => new VarToken(),
                _ => new IdentifierToken(tokenString)
            };
        }



        public IReadOnlyList<IToken> EnumerateTokens(ReadOnlySpan<char> input)
        {
            // This is not going to be a fast tokenizer

            List<string> tokenStrings = new List<string>();
            List<IToken> tokens = new List<IToken>();

            ReadOnlySpan<char> currentToken = ReadOnlySpan<char>.Empty;

            for (int i = 0; i < input.Length; i++)
            {
                var currentChar = input[i];

                // Handle white space
                if (char.IsWhiteSpace(currentChar))
                {
                    if (currentToken.IsEmpty)
                    {
                        // If it's empty, do nothing
                        continue;
                    }

                    tokenStrings.Add(currentToken.ToString());
                    tokens.Add(ParseToken(currentToken));

                    currentToken = ReadOnlySpan<char>.Empty;
                    continue;
                }

                if (allowedSingleCharacters.Contains(currentChar))
                {
                    if (!currentToken.IsEmpty)
                    {
                        tokenStrings.Add(currentToken.ToString());
                        tokens.Add(ParseToken(currentToken));
                        currentToken = ReadOnlySpan<char>.Empty;
                    }

                    tokens.Add(ParseSingleCharToken(currentChar));
                }
                else if (char.IsLetterOrDigit(currentChar)) {
                    // If its a digit or a letter, just add it to the current token
                    currentToken = input.Slice(i - currentToken.Length, currentToken.Length + 1);
                }
                else if (currentChar == '\'')
                {
                    // Could be the start or end of a character constant
                    
                }
                else if (currentChar == '"')
                {
                    // Could be the start or end of a string constant
                    
                }
                else
                {
                    throw new InvalidTokenParsingException(currentChar, currentToken);
                }
            }

            if (!currentToken.IsEmpty)
            {
                tokenStrings.Add(currentToken.ToString());
                tokens.Add(ParseToken(currentToken));
            }

            return tokens;
        }
    }
}
