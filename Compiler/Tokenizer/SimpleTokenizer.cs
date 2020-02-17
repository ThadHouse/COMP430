using Compiler.Tokenizer.Exceptions;
using Compiler.Tokenizer.Tokens;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Compiler.Tokenizer
{
    public class SimpleTokenizer : ITokenizer
    {
        public static readonly char[] AllowedSingleCharacters = new char[]
{
            '[', ']', '{', '}', ';', '(', ')', '.', ',', '=', '-', '+', '*', '&', '^', '%', '/', '!', '<', '>'
};

        public static readonly string[] Aliases = new string[]
        {
            "int",
            "string",
            "bool",
            "object",
            "void",
        };

        public static readonly string[] Keywords = new string[]
        {
            "class",
            "namespace",
            "static",
            "return",
            "auto",
            "entrypoint",
            "constructor",
            "delegate",
            "field",
            "method",
            "ref",
            "this"
        };

        public static ISingleCharToken ParseSingleCharToken(char token)
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
                '-' => new MinusToken(),
                '+' => new PlusToken(),
                '&' => new AmpersandToken(),
                '^' => new HatToken(),
                '%' => new PercentSignToken(),
                '!' => new ExclamationPointToken(),
                '/' => new ForwardSlashToken(),
                '<' => new LeftArrowToken(),
                '>' => new RightArrowToken(),
                '*' => new StarToken(),
                _ => throw new InvalidTokenParsingException(token, ReadOnlySpan<char>.Empty) // This should never be hit
            };
        }

        public static IToken ParseToken(ReadOnlySpan<char> token, bool isReadingNumber)
        {
            var tokenString = token.ToString();

            if (isReadingNumber)
            {
                var slice = token;
                if (token.StartsWith("0x".AsSpan(), StringComparison.InvariantCultureIgnoreCase))
                {
                    slice = token.Slice(2);
                    if (slice.IsEmpty)
                    {
                        throw new InvalidNumericTokenException("Token cannot just be 0x");
                    }
                }
                foreach (var d in slice)
                {
                    if (!char.IsDigit(d) && d != '.')
                    {
                        throw new InvalidNumericTokenException("Token must contain only numbers");
                    }
                }
                return new NumericConstantToken(tokenString);
            }


            return tokenString switch
            {
                "class" => new ClassToken(),
                "namespace" => new NamespaceToken(),
                "static" => new StaticToken(),
                "return" => new ReturnToken(),
                "auto" => new AutoToken(),
                "entrypoint" => new EntryPointToken(),
                "constructor" => new ConstructorToken(),
                "method" => new MethodToken(),
                "field" => new FieldToken(),
                "delegate" => new DelegateToken(),
                "ref" => new RefToken(),
                "this" => new ThisToken(),

                // Handle aliases
                "int" => new IdentifierToken("System.Int32"),
                "string" => new IdentifierToken("System.String"),
                "bool" => new IdentifierToken("System.Boolean"),
                "object" => new IdentifierToken("System.Object"),
                "void" => new IdentifierToken("System.Void"),

                _ => new IdentifierToken(tokenString.Replace("::", ".")),
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
                char constChar = EscapeSequence.EscapeChar(input[i + 2], input, i + 3, out var adj);
                if (input.Length <= i + 3 + adj)
                {
                    throw new CharacterConstantException("Not enough characters left in file");
                }
                if (input[i + 3 + adj] != '\'')
                {
                    throw new CharacterConstantException("Odd ending character");
                }
                return (constChar, adj + 3);
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
            string toRet = string.Empty;

            // Parse the rest
            int curIndex = i + 1;
            while (true)
            {
                if (curIndex == input.Length)
                {
                    throw new StringConstantException("Not enough characters left to parse");
                }

                if (input[curIndex] == '\\')
                {
                    // Escape
                    // Must have at least 2 characters
                    if (curIndex >= input.Length - 2)
                    {
                        throw new StringConstantException("Not enough characters left in file");
                    }

                    char constChar = EscapeSequence.EscapeChar(input[curIndex + 1], input, curIndex + 2, out var adj);
                    toRet += constChar;
                    curIndex += adj + 1;


                }
                else if (input[curIndex] == '\"')
                {
                    int len = curIndex - i - 1;
                    return (toRet, len + 1);
                }
                else
                {
                    toRet += input[curIndex];
                }
                curIndex++;
            }
        }

        public ReadOnlySpan<IToken> EnumerateTokens(ReadOnlySpan<char> input)
        {
            // This is not going to be a fast tokenizer

            List<string> tokenStrings = new List<string>();
            List<IToken> tokens = new List<IToken>();

            ReadOnlySpan<char> currentToken = ReadOnlySpan<char>.Empty;

            bool isReadingNumber = false;
            bool hasReadADot = false;

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
                    tokens.Add(ParseToken(currentToken, isReadingNumber));

                    currentToken = ReadOnlySpan<char>.Empty;
                    hasReadADot = false;
                    continue;
                }

                if (AllowedSingleCharacters.Contains(currentChar))
                {
                    // If a ., 
                    if (currentChar == '.' && !currentToken.IsEmpty && isReadingNumber)
                    {
                        if (hasReadADot)
                        {
                            throw new InvalidTokenParsingException(currentChar, currentToken);
                        }
                        else
                        {
                            hasReadADot = true;
                            currentToken = input.Slice(i - currentToken.Length, currentToken.Length + 1);
                            continue;
                        }
                    }

                    if (!currentToken.IsEmpty)
                    {
                        tokenStrings.Add(currentToken.ToString());
                        tokens.Add(ParseToken(currentToken, isReadingNumber));
                        currentToken = ReadOnlySpan<char>.Empty;
                        hasReadADot = false;
                    }

                    tokens.Add(ParseSingleCharToken(currentChar));
                }
                else if (char.IsLetterOrDigit(currentChar) || currentChar == '_' || currentChar == ':')
                {
                    if (currentToken.IsEmpty)
                    {
                        isReadingNumber = char.IsDigit(currentChar);
                    }

                    // If its a digit or a letter, just add it to the current token
                    currentToken = input.Slice(i - currentToken.Length, currentToken.Length + 1);
                }
                else if (currentChar == '\'')
                {

                    var (parsed, toIncrement) = ParseCharLiteral(input, i);
                    tokenStrings.Add(parsed.ToString(CultureInfo.InvariantCulture));
                    tokens.Add(new CharacterConstantToken(parsed));
                    i += toIncrement;


                    // Fast forward to find the end. There might be an escape character

                    // Next char

                }
                else if (currentChar == '"')
                {
                    // Could be the start or end of a string constant
                    var (parsed, toIncrement) = ParseStringLiteral(input, i);
                    tokenStrings.Add(parsed.ToString(CultureInfo.InvariantCulture));
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
                tokens.Add(ParseToken(currentToken, isReadingNumber));
            }

            return tokens.ToArray();
        }
    }
}
