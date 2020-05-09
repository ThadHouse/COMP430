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
{           '[', ']', '{', '}', '(', ')', ';', '.', ',', '-', '+', '&', '^', '%', '!', '/', '<', '>', '*', '='
};

        public static readonly string[] Aliases = new string[]
        {
            "int",
            "double",
            "string",
            "bool",
            "object",
            "void",
        };

        public static readonly string[] Keywords = new string[]
        {
            "class",
            "namespace", // not used
            "static",
            "return",
            "auto",
            "if",
            "else",
            "entrypoint",
            "constructor",
            "delegate",
            "field",
            "method",
            "ref",
            "new",
            "newarr",
            "while"
        };

        // Parse a single character token
        public static IToken ParseCharacterToken(char token, char? nextToken)
        {
            // The nullable nextToken must not be used in the if compaison directly
            // or 100% coverage cannot be obtained because of bad code gen
            // https://github.com/dotnet/roslyn/issues/44109

            char nonNullNextToken = nextToken.GetValueOrDefault();

            // Any double character tokens are special cased with a lookahead
            if (token == '=')
            {
                if (nextToken != null && nonNullNextToken == '=')
                {
                    return new DoubleEqualsToken();
                }
                return new EqualsToken();
            }

            if (token == '!')
            {
                if (nextToken != null && nonNullNextToken == '=')
                {
                    return new NotEqualsToken();
                }
                return new ExclamationPointToken();
            }

            if (token == '<')
            {
                if (nextToken != null && nonNullNextToken == '=')
                {
                    return new LessThenOrEqualToToken();
                }
                return new LeftArrowToken();
            }

            if (token == '>')
            {
                if (nextToken != null && nonNullNextToken == '=')
                {
                    return new GreaterThenOrEqualToToken();
                }
                return new RightArrowToken();
            }

            // If it wasn't a double token, just handle it normally
            // Do not need to check for the tokens handled above
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
                '-' => new MinusToken(),
                '+' => new PlusToken(),
                '&' => new AmpersandToken(),
                '^' => new HatToken(),
                '%' => new PercentSignToken(),
                '/' => new ForwardSlashToken(),
                '*' => new StarToken(),
                _ => throw new InvalidTokenParsingException("Assertion here should never be hit") // This should never be hit
            };
        }

        public static IToken? TryParseCharLikeToken(ref ReadOnlySpan<char> token)
        {
            if (!AllowedSingleCharacters.Contains(token[0]))
            {
                return null;
            }

            char firstChar = token[0];
            token = token.Slice(1);

            char? nextChar = null;

            if (!token.IsEmpty)
            {
                nextChar = token[0];
            }

            var tokenToRet = ParseCharacterToken(firstChar, nextChar);
            if (tokenToRet is IMultiCharOperationToken)
            {
                token = token.Slice(1);
            }
            return tokenToRet;
        }

        public static IToken? TryParseNumericToken(ref ReadOnlySpan<char> token)
        {
            var originalString = token;

            bool isNegative = false;
            int dataCount = 0;
            bool hasDot = false;
            bool wasDotLast = true;

            if (token[0] == '-')
            {
                // Negative Number (potentiall)
                isNegative = true;
                token = token.Slice(1);
            }

            while (!token.IsEmpty && (char.IsDigit(token[0]) || token[0] == '.'))
            {
                if (token[0] == '.')
                {
                    if (hasDot == true)
                    {
                        break;
                    }
                    hasDot = true;
                    wasDotLast = true;
                }
                else
                {
                    wasDotLast = false;
                }
                dataCount++;
                token = token.Slice(1);
            }

            if (dataCount == 0)
            {
                token = originalString;
                return null;
            }

            if (dataCount == 1 && originalString[0] == '.')
            {
                token = originalString;
                return null;
            }

            if (isNegative)
            {
                dataCount++;
            }

            var toParse = originalString.Slice(0, dataCount).ToString();

            if (hasDot && !wasDotLast)
            {
                double iResult = double.Parse(toParse, CultureInfo.InvariantCulture);
                return new DoubleConstantToken(iResult);
            }
            else
            {
                int iResult = int.Parse(toParse, CultureInfo.InvariantCulture);
                return new IntegerConstantToken(iResult);
            }
        }

        public static IToken ParseIdentifier(ref ReadOnlySpan<char> token)
        {
            var origString = token;
            var tokenRef = ReadOnlySpan<char>.Empty;

            while (!token.IsEmpty && (char.IsLetter(token[0]) || token[0] == '_' || token[0] == ':'))
            {
                tokenRef = origString.Slice(0, tokenRef.Length + 1);
                token = token.Slice(1);
            }

            var tokenString = tokenRef.ToString();

            if (tokenRef.IsEmpty)
            {
                if (token.Length == 0)
                {
                    throw new InvalidTokenParsingException("Error being out of characters here");
                }
                throw new InvalidTokenParsingException($"Invalid character {token[0]}");
            }

            bool isArray = false;

            // See if we are an array
            if (token.Length > 2)
            {
                if (token[0] == '[' && token[1] == ']')
                {
                    // Is an array
                    isArray = true;
                    token = token.Slice(2);
                }
            }

            var knownToken = tokenString switch
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
                "new" => new NewToken(),
                "newarr" => new NewArrToken(),
                "while" => new WhileToken(),
                "if" => new IfToken(),
                "else" => new ElseToken(),
                _ => (IToken?)null,
            };

            if (knownToken != null)
            {
                if (isArray)
                {
                    throw new InvalidTokenParsingException("Cannot have an array of keywords");
                }
                else
                {
                    return knownToken;
                }
            }

            if (isArray)
            {
                return tokenString switch
                {
                    "int" => new AliasedIdentifierToken("System.Int32[]", tokenString),
                    "double" => new AliasedIdentifierToken("System.Double[]", tokenString),
                    "string" => new AliasedIdentifierToken("System.String[]", tokenString),
                    "bool" => new AliasedIdentifierToken("System.Boolean[]", tokenString),
                    "object" => new AliasedIdentifierToken("System.Object[]", tokenString),
                    "void" => new AliasedIdentifierToken("System.Void[]", tokenString),

                    _ => new IdentifierToken(tokenString.Replace("::", ".") + "[]"),
                };
            }
            else
            {
                return tokenString switch
                {
                    "int" => new AliasedIdentifierToken("System.Int32", tokenString),
                    "double" => new AliasedIdentifierToken("System.Double", tokenString),
                    "string" => new AliasedIdentifierToken("System.String", tokenString),
                    "bool" => new AliasedIdentifierToken("System.Boolean", tokenString),
                    "object" => new AliasedIdentifierToken("System.Object", tokenString),
                    "void" => new AliasedIdentifierToken("System.Void", tokenString),

                    _ => new IdentifierToken(tokenString.Replace("::", ".")),
                };
            }

        }

        private IToken? TryParseCharLiteral(ref ReadOnlySpan<char> input)
        {
            if (input[0] != '\'')
            {
                return null;
            }

            var (parsed, toIncrement) = ParseCharLiteral(input, 0);
            input = input.Slice(toIncrement);
            return new CharacterConstantToken(parsed);
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
                return (constChar, adj + 4);
            }
            else if (input[i + 2] == '\'')
            {
                // Found end
                char constChar = input[i + 1];
                return (constChar, 3);
            }
            else
            {
                throw new CharacterConstantException("Too long of character constant");
            }
        }

        private IToken? TryParseStringLiteral(ref ReadOnlySpan<char> input)
        {
            if (input[0] != '"')
            {
                return null;
            }

            var (parsed, toIncrement) = ParseStringLiteral(input, 0);
            input = input.Slice(toIncrement);
            return new StringConstantToken(parsed);
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
                    int len = curIndex - i;
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
            var tokens = new List<IToken>();

            while (!input.IsEmpty)
            {
                if (char.IsWhiteSpace(input[0]))
                {
                    input = input.Slice(1);
                    continue;
                }

                var potentialNumber = TryParseNumericToken(ref input);
                if (potentialNumber != null)
                {
                    tokens.Add(potentialNumber);
                    continue;
                }

                var potentialChar = TryParseCharLikeToken(ref input);
                if (potentialChar != null)
                {
                    tokens.Add(potentialChar);
                    continue;
                }

                var potentialCharLiteral = TryParseCharLiteral(ref input);
                if (potentialCharLiteral != null)
                {
                    tokens.Add(potentialCharLiteral);
                    continue;
                }

                var potentialStringLiteral = TryParseStringLiteral(ref input);
                if (potentialStringLiteral != null)
                {
                    tokens.Add(potentialStringLiteral);
                    continue;
                }

                if (input[0] == '#')
                {
                    // Comment
                    while (!input.IsEmpty)
                    {
                        if (input[0] == '\n')
                        {
                            break;
                        }
                        input = input.Slice(1);
                    }
                    continue;
                }

                tokens.Add(ParseIdentifier(ref input));
            }

            return tokens.ToArray();
        }
    }
}
