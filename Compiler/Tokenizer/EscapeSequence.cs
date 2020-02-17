using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Compiler.Tokenizer.Exceptions;

namespace Compiler.Tokenizer
{
    public static class EscapeSequence
    {
        public static char EscapeUnicodeChar(char c, ReadOnlySpan<char> input, int i, out int adjustment)
        {
            switch (c)
            {
                case 'u':
                    adjustment = 4;
                    if (input.Length < i + 4)
                    {
                        throw new OutOfCharactersException("Not enough character to parse unicode escape sequence");
                    }
                    for (int j = i; j < i + 4; j++)
                    {
                        if (!char.IsLetterOrDigit(input[j]))
                        {
                            throw new UnrecognizedEscapeException("Espace sequence not long enough");
                        }
                    }
                    return (char)short.Parse(input.Slice(i, 4).ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                case 'x':
                    adjustment = 0;
                    int maxLength = i + 4;
                    int origI = i;
                    for (; i < maxLength; i++)
                    {
                        if (i >= input.Length)
                        {
                            if (adjustment == 0)
                            {
                                throw new OutOfCharactersException("Not enough characters to parse variable length unicode escape sequence");
                            }
                            else
                            {
                                break;
                            }
                        }
                        if (char.IsLetterOrDigit(input[i]))
                        {
                            // Is a letter or a digit
                            adjustment++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (adjustment == 0)
                    {
                        throw new UnrecognizedEscapeException("No extra characters for \\x escape");
                    }
                    return (char)short.Parse(input.Slice(origI, adjustment).ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                case 'U':
                    throw new UnsupportedSurrogatePairEscapeException("\\U escapes are not supported");
                default:
                    throw new UnrecognizedEscapeException($"Unrecognized escape \\{c}");
            }
        }

        public static char EscapeChar(char c, ReadOnlySpan<char> input, int i, out int adjustment)
        {
            adjustment = 0;
            return c switch
            {
                '\'' => '\'',
                '\"' => '\"',
                '\\' => '\\',
                '0' => '\0',
                'a' => '\a',
                'b' => '\b',
                'f' => '\f',
                'n' => '\n',
                'r' => '\r',
                't' => '\t',
                'v' => '\v',
                'u' => EscapeUnicodeChar(c, input, i, out adjustment),
                'x' => EscapeUnicodeChar(c, input, i, out adjustment),
                'U' => EscapeUnicodeChar(c, input, i, out adjustment),
                _ => throw new UnrecognizedEscapeException($"Unrecognized escape \\{c}"),
            };
        }
    }
}
