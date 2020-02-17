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
            Assert.Equal(1, tokens.Count);
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
            Assert.Throws<CharacterConstantException>(() => tokenizer.EnumerateTokens(code.AsSpan()));
        }

        [Fact]
        public void TestCharConstantBadEscape3()
        {
            var code = @"
'\;b";

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
            Assert.Equal(2, tokens.Count);
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
            Assert.Equal(2, tokens.Count);
            Assert.IsType<EqualsToken>(tokens[0]);
            var charConstToken = Assert.IsType<CharacterConstantToken>(tokens[1]);
            Assert.Equal('a', charConstToken.Value);
        }
    }
}
