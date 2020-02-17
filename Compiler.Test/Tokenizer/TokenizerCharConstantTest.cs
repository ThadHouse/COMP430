using System;
using System.Collections.Generic;
using System.Text;
using Compiler.Tokenizer;
using Compiler.Tokenizer.Exceptions;
using Compiler.Tokenizer.Tokens;
using Xunit;

namespace Compiler.Test.Tokenizer
{
    public class TokenizerCharConstantTest
    {
        [Fact]
        public void TestCharConstantWorksAsOnlyCode()
        {
            var code = @"
'a'";

            ITokenizer tokenizer = new Compiler.Tokenizer.SimpleTokenizer();
            var tokens = tokenizer.EnumerateTokens(code.AsSpan());
            Assert.Equal(1, tokens.Length);
            var charConstToken = Assert.IsType<CharacterConstantToken>(tokens[0]);
            Assert.Equal('a', charConstToken.Value);
        }

        [Fact]
        public void TestCharConstantTooLong()
        {
            var code = @"
'a '";

            ITokenizer tokenizer = new Compiler.Tokenizer.SimpleTokenizer();
            Assert.Throws<CharacterConstantException>(() => tokenizer.EnumerateTokens(code.AsSpan()));
        }

        [Fact]
        public void TestCharConstantTooShort()
        {
            var code = @"
'";

            ITokenizer tokenizer = new Compiler.Tokenizer.SimpleTokenizer();
            Assert.Throws<CharacterConstantException>(() => tokenizer.EnumerateTokens(code.AsSpan()));
        }

        [Fact]
        public void TestCharConstantBadEscape()
        {
            var code = @"
'\'";

            ITokenizer tokenizer = new Compiler.Tokenizer.SimpleTokenizer();
            Assert.Throws<CharacterConstantException>(() => tokenizer.EnumerateTokens(code.AsSpan()));
        }

        [Fact]
        public void TestCharConstantBadEscape2()
        {
            var code = @"
'\;'";

            ITokenizer tokenizer = new Compiler.Tokenizer.SimpleTokenizer();
            Assert.Throws<UnrecognizedEscapeException>(() => tokenizer.EnumerateTokens(code.AsSpan()));
        }

        [Fact]
        public void TestCharConstantBadEscape3()
        {
            var code = @"
'\ab";

            ITokenizer tokenizer = new Compiler.Tokenizer.SimpleTokenizer();
            Assert.Throws<CharacterConstantException>(() => tokenizer.EnumerateTokens(code.AsSpan()));
        }

        [Fact]
        public void TestCharConstantEmpty()
        {
            var code = @"
''";

            ITokenizer tokenizer = new Compiler.Tokenizer.SimpleTokenizer();
            Assert.Throws<CharacterConstantException>(() => tokenizer.EnumerateTokens(code.AsSpan()));
        }

        [Fact]
        public void TestCharConstantEmptyNotLastInFile()
        {
            var code = @"
'';";

            ITokenizer tokenizer = new Compiler.Tokenizer.SimpleTokenizer();
            Assert.Throws<CharacterConstantException>(() => tokenizer.EnumerateTokens(code.AsSpan()));
        }

        [Fact]
        public void TestCharConstantWorksAfterSingleCharTokenNoWhitespace()
        {
            var code = @"
='a'";

            ITokenizer tokenizer = new Compiler.Tokenizer.SimpleTokenizer();
            var tokens = tokenizer.EnumerateTokens(code.AsSpan());
            Assert.Equal(2, tokens.Length);
            Assert.IsType<EqualsToken>(tokens[0]);
            var charConstToken = Assert.IsType<CharacterConstantToken>(tokens[1]);
            Assert.Equal('a', charConstToken.Value);
        }

        [Fact]
        public void TestCharConstantWorksAfterSingleCharTokenWithWhitespace()
        {
            var code = @"
= 'a'";

            ITokenizer tokenizer = new Compiler.Tokenizer.SimpleTokenizer();
            var tokens = tokenizer.EnumerateTokens(code.AsSpan());
            Assert.Equal(2, tokens.Length);
            Assert.IsType<EqualsToken>(tokens[0]);
            var charConstToken = Assert.IsType<CharacterConstantToken>(tokens[1]);
            Assert.Equal('a', charConstToken.Value);
        }

        [Theory]
        [InlineData(@"\'", '\'')]
        [InlineData(@"\""", '\"')]
        [InlineData(@"\\", '\\')]
        [InlineData(@"\0", '\0')]
        [InlineData(@"\a", '\a')]
        [InlineData(@"\b", '\b')]
        [InlineData(@"\f", '\f')]
        [InlineData(@"\n", '\n')]
        [InlineData(@"\r", '\r')]
        [InlineData(@"\t", '\t')]
        [InlineData(@"\v", '\v')]
        public void TestPrimaryEscapeSequencesWork(string input, char output)
        {
            var code = $@"
'{input}'
";
            ITokenizer tokenizer = new Compiler.Tokenizer.SimpleTokenizer();
            var tokens = tokenizer.EnumerateTokens(code.AsSpan());
            Assert.Equal(1, tokens.Length);
            var charConstToken = Assert.IsType<CharacterConstantToken>(tokens[0]);
            Assert.Equal(output, charConstToken.Value);
        }

        [Theory]
        [InlineData(@"\'", '\'')]
        [InlineData(@"\""", '\"')]
        [InlineData(@"\\", '\\')]
        [InlineData(@"\0", '\0')]
        [InlineData(@"\a", '\a')]
        [InlineData(@"\b", '\b')]
        [InlineData(@"\f", '\f')]
        [InlineData(@"\n", '\n')]
        [InlineData(@"\r", '\r')]
        [InlineData(@"\t", '\t')]
        [InlineData(@"\v", '\v')]
        public void TestPrimaryEscapeSequencesWork2(string input, char output)
        {
            var code = $@"
'{input}';
x = 5;
";
            ITokenizer tokenizer = new Compiler.Tokenizer.SimpleTokenizer();
            var tokens = tokenizer.EnumerateTokens(code.AsSpan());
            Assert.Equal(6, tokens.Length);
            var charConstToken = Assert.IsType<CharacterConstantToken>(tokens[0]);
            Assert.Equal(output, charConstToken.Value);
        }

        [Fact]
        public void TestUnicodeEscapeSequencesWork()
        {
            Span<char> code = stackalloc char[]
            {
                '\'',
                '\\',
                'u',
                '0',
                '0',
                '0',
                '0',
                '\''
            };

            ReadOnlySpan<char> format = "X4";
            Span<char> innerStorage = code.Slice(3);

            for (int i = 0; i < char.MaxValue; i++)
            {
                ITokenizer tokenizer = new SimpleTokenizer();
                i.TryFormat(innerStorage, out var charsWritten, format);
                var tokens = tokenizer.EnumerateTokens(code);
                Assert.Equal(1, tokens.Length);
                var charConstToken = Assert.IsType<CharacterConstantToken>(tokens[0]);
                Assert.Equal((char)i, charConstToken.Value);
            }

        }

        [Fact]
        public void TestVariableLengthUnicodeEscapeSequencesWork()
        {
            Span<char> code = stackalloc char[]
            {
                '\'',
                '\\',
                'x',
                '0',
                '0',
                '0',
                '0',
                '\''
            };

            ReadOnlySpan<char> format = "X";
            Span<char> innerStorage = code.Slice(3);

            for (int i = 0; i < char.MaxValue; i++)
            {
                ITokenizer tokenizer = new SimpleTokenizer();
                i.TryFormat(innerStorage, out var charsWritten, format);
                Span<char> codeStorage = code.Slice(0, charsWritten + 4);
                codeStorage[codeStorage.Length - 1] = '\'';
                var tokens = tokenizer.EnumerateTokens(codeStorage);
                Assert.Equal(1, tokens.Length);
                var charConstToken = Assert.IsType<CharacterConstantToken>(tokens[0]);
                Assert.Equal((char)i, charConstToken.Value);
            }

        }
    }
}
