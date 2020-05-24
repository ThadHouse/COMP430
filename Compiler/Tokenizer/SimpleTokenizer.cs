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
    // This tokenizer is designed to simply handle tokenization of a string.
    // It tokenizes a character at a time, with helper methods to try and tokenize
    // multi character things
    public class SimpleTokenizer : ITokenizer
    {
        // A list of all characters that are allowed to be by themselves. These characters can't appear in identifiers
        public static readonly char[] AllowedSingleCharacters = new char[]
        {
            '[', ']', '{', '}', '(', ')', ';', '.', ',', '-', '+', '&', '^', '%', '!', '/', '<', '>', '*', '='
        };

        // These are type aliases. These are special identifiers that can only appear in types
        // Map to the System.* types in the BCL
        public static readonly string[] Aliases = new string[]
        {
            "int",
            "double",
            "string",
            "bool",
            "object",
            "void",
        };

        // A list of all keywords in the language. Reserved keywords could be added if necessary
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
            "ref", // Not used
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

        // Trys to parse a single character token 
        public static IToken? TryParseCharLikeToken(ref ReadOnlySpan<char> token)
        {
            // If its not in single character list, return, its not a character token
            if (!AllowedSingleCharacters.Contains(token[0]))
            {
                return null;
            }

            // Parsing a character requires the next token for double character tokens like ==
            char firstChar = token[0];
            token = token.Slice(1);

            char? nextChar = null;

            if (!token.IsEmpty)
            {
                nextChar = token[0];
            }

            var tokenToRet = ParseCharacterToken(firstChar, nextChar);
            // Multi char needs to slice the span again to remove the 2nd char
            if (tokenToRet is IMultiCharOperationToken)
            {
                token = token.Slice(1);
            }
            return tokenToRet;
        }

        // Tries to parse a numeric token.
        // This has some issues, and does need to be improved
        // It needs to error, and not return null for something like 123abc, since we don't want to allow 
        // that as an identifier
        // Doubles are semi supported, however not supported in the parser or emitter.
        // Testing more common cases would massively help here.
        public static IToken? TryParseNumericToken(ref ReadOnlySpan<char> token)
        {
            var originalString = token;

            bool isNegative = false;
            int dataCount = 0;
            bool hasDot = false;
            bool wasDotLast = true;

            // We could be a negative number
            if (token[0] == '-')
            {
                // Negative Number (potential)
                isNegative = true;
                token = token.Slice(1);
            }


            // Loop while there is digits or dots to look for
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

            // Try to parse into a double or an int
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

        // Parse an identifier token
        // Identifier tokens have an issue where they don't allow any numeric characters
        // Its in the 1st while, but needs tests to ensure it doesn't cause any other issues
        // Arrays ([]) are part of the identifer too.
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
            if (token.Length >= 2)
            {
                if (token[0] == '[' && token[1] == ']')
                {
                    // Is an array
                    isArray = true;
                    token = token.Slice(2);
                }
            }

            // This checks to see if its a known token.
            // This needs to be extracted into a function so we can handle reserved keywords properly.
            // That could then be tested against the Keyword array at the top of this file.
            // Reflection could also be used for this, and might be a cooler demo.
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

            // Check for things that can be arrays, or aliases, or its just a normal identifier
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
                    "void" => throw new InvalidTokenParsingException("void[] makes no sense"),

                    // In our syntax, :: separates namespaces, but IL needs these to have a .
                    // Easy change
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

        // Try to parse a character literal. Escapes are handled here, but not supporting them makes this much easier
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

        // Try to parse a string literal. This supports escapes, but this is harder to actually do.
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

        // This is our main enumerations function
        public ReadOnlySpan<IToken> EnumerateTokens(ReadOnlySpan<char> input)
        {
            var tokens = new List<IToken>();

            while (!input.IsEmpty)
            {
                // Loop through all white space
                if (char.IsWhiteSpace(input[0]))
                {
                    input = input.Slice(1);
                    continue;
                }

                // Try to parse a number. As said above, 123abc would parse as 123, and then parse as abc next. This 
                // needs to be fixed.
                var potentialNumber = TryParseNumericToken(ref input);
                if (potentialNumber != null)
                {
                    tokens.Add(potentialNumber);
                    continue;
                }

                // Try to parse a single character
                var potentialChar = TryParseCharLikeToken(ref input);
                if (potentialChar != null)
                {
                    tokens.Add(potentialChar);
                    continue;
                }

                // Try to parse a character literal.
                var potentialCharLiteral = TryParseCharLiteral(ref input);
                if (potentialCharLiteral != null)
                {
                    tokens.Add(potentialCharLiteral);
                    continue;
                }

                // Try to parse a string literal
                var potentialStringLiteral = TryParseStringLiteral(ref input);
                if (potentialStringLiteral != null)
                {
                    tokens.Add(potentialStringLiteral);
                    continue;
                }

                //Comment to the end of the line on #

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

                // If all of that is done, we have an identifier
                tokens.Add(ParseIdentifier(ref input));
            }

            return tokens.ToArray();
        }
    }
}
