using Compiler.Tokenizer.Exceptions;
using Compiler.Tokenizer.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Compiler.Tokenizer
{
    public class Tokenizer : ITokenizer
    {
        public static readonly char[] AllowedSingleCharacters = new char[]
{
            '[', ']', '{', '}', ';', '(', ')', '.', ',', '='
};

        public static readonly string[] Keywords = new string[]
        {
            "class",
            "namespace",
            "static",
            "return",
            "var"
        };

        public ISingleCharToken ParseSingleCharToken(char token)
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

        public IToken ParseToken(ReadOnlySpan<char> token)
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

        private (char parsed, int toIncrement) ParseCharLiteral(ReadOnlySpan<char> input, int i)
        {
            // Is the start of a char constant
            if (i >= input.Length - 2)
            {
                throw new CharacterConstantException("Not enough characters left in file");
            }

            if (input[i + 1] == '\'')
            {
                throw new CharacterConstantException("Cannot immediately have a '");
            }

            if (input[i + 1] == '\\')
            {
                // Escape character, need to be able to look 3 ahead
                if (i >= input.Length - 3)
                {
                    throw new CharacterConstantException("Not enough characters left in file");
                }
                if (input[i + 3] != '\'')
                {
                    throw new CharacterConstantException("Odd ending character");
                }
                throw new CharacterConstantException("Escape sequences are not supported yet");
                //var unescaped = Regex.Unescape(input.Slice(i + 1, 2).ToString());
                //if (unescaped.Length != 1)
                //{
                //    throw new CharacterConstantException("Weird unescape?");
                //}
                //char constChar = unescaped[0];
                //return (constChar, 3);
            }
            else if (input[i + 2] == '\'')
            {
                // Found end
                char constChar = input[i + 1];
                return (constChar, 2);
            }
            else
            {
                throw new CharacterConstantException("Too long of character constant");
            }
        }

        private (string parsed, int toIncrement) ParseStringLiteral(ReadOnlySpan<char> input, int i)
        {
            throw new NotImplementedException();
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

                if (AllowedSingleCharacters.Contains(currentChar))
                {
                    if (!currentToken.IsEmpty)
                    {
                        tokenStrings.Add(currentToken.ToString());
                        tokens.Add(ParseToken(currentToken));
                        currentToken = ReadOnlySpan<char>.Empty;
                    }

                    tokens.Add(ParseSingleCharToken(currentChar));
                }
                else if (char.IsLetterOrDigit(currentChar))
                {
                    // If its a digit or a letter, just add it to the current token
                    currentToken = input.Slice(i - currentToken.Length, currentToken.Length + 1);
                }
                else if (currentChar == '\'')
                {

                    var (parsed, toIncrement) = ParseCharLiteral(input, i);
                    tokenStrings.Add(parsed.ToString());
                    tokens.Add(new CharacterConstantToken(parsed));
                    i += toIncrement;


                    // Fast forward to find the end. There might be an escape character

                    // Next char

                }
                else if (currentChar == '"')
                {
                    // Could be the start or end of a string constant
                    var (parsed, toIncrement) = ParseStringLiteral(input, i);
                    tokenStrings.Add(parsed.ToString());
                    tokens.Add(new StringConstantToken(parsed));
                    i += toIncrement;
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
