using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Compiler.Tokenizer;
using Compiler.Tokenizer.Exceptions;
using Xunit;

namespace Compiler.Test.Tokenizer
{
    public class EscapeSequencesTest
    {
        [Theory]
        [InlineData('\'', '\'')]
        [InlineData('\"', '\"')]
        [InlineData('\\', '\\')]
        [InlineData('0', '\0')]
        [InlineData('a', '\a')]
        [InlineData('b', '\b')]
        [InlineData('f', '\f')]
        [InlineData('n', '\n')]
        [InlineData('r', '\r')]
        [InlineData('t', '\t')]
        [InlineData('v', '\v')]

        public void TestPrimaryEscapeSequencesWork(char input, char output)
        {
            Assert.Equal(output, EscapeSequence.EscapeChar(input, ReadOnlySpan<char>.Empty, 0, out var _));
        }

        [Fact]
        public void TestAllInvalidEscapesFail()
        {
            char[] valid = new char[]
            {
                '\'', '\"', '\\',
                '0', 'a', 'b', 'f', 'n',
                'r', 't', 'v',
                'u', 'x', 'U',
            };
            char i = (char)0;
            // Don't want to N^2 for all characters
            // Just ones we know are in ASCII
            for (; i < 256; i++)
            {
                if (valid.Contains(i)) continue;
                Assert.Throws<UnrecognizedEscapeException>(() =>
                {
                    EscapeSequence.EscapeChar(i, ReadOnlySpan<char>.Empty, 0, out var _);
                });
            }
            for (; i < char.MaxValue; i++)
            {
                Assert.Throws<UnrecognizedEscapeException>(() =>
                {
                    EscapeSequence.EscapeChar(i, ReadOnlySpan<char>.Empty, 0, out var _);
                });
            }
            // Handle max value
            Assert.Throws<UnrecognizedEscapeException>(() =>
            {
                EscapeSequence.EscapeChar(i, ReadOnlySpan<char>.Empty, 0, out var _);
            });
        }

        [Fact]
        public void TestNormalAdjustment()
        {
            EscapeSequence.EscapeChar('a', ReadOnlySpan<char>.Empty, 0, out var adj);
            Assert.Equal(0, adj);
        }

        [Fact]
        public void TestUnicodeAdjustment()
        {

            EscapeSequence.EscapeChar('u', "u1234", 1, out var adj);
            Assert.Equal(4, adj);
        }

        [Fact]
        public void TestAllUnicodeEscapeCharacters()
        {
            Span<char> storage = stackalloc char[5];
            storage[0] = 'u';
            Span<char> innerStorage = storage.Slice(1);
            ReadOnlySpan<char> format = "X4";
            for (int i = 0; i <= char.MaxValue; i++)
            {
                i.TryFormat(innerStorage, out var charsWritten, format);
                Assert.Equal((char)i, EscapeSequence.EscapeChar('u', storage, 1, out var adj));
                ;
            }
        }

        [Fact]
        public void TestVariableLengthUnicodeEscapeCharacters()
        {
            Span<char> storage = stackalloc char[5];
            storage[0] = 'x';
            Span<char> innerStorage = storage.Slice(1);
            ReadOnlySpan<char> format = "X";
            for (int i = 0; i <= char.MaxValue; i++)
            {
                i.TryFormat(innerStorage, out var charsWritten, format);
                Span<char> writeStorage = storage.Slice(0, charsWritten + 1);
                Assert.Equal((char)i, EscapeSequence.EscapeChar('x', writeStorage, 1, out var adj));
                Assert.Equal(charsWritten, adj);
                ;
            }
        }
    }
}
