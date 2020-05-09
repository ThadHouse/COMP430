using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Compiler.Tokenizer;
using Compiler.Tokenizer.Tokens;
using Compiler.Tokenizer.Exceptions;

namespace Compiler.Test.Tokenizer
{
    public class TokenizerTest
    {
        [Fact]
        public void TestInvalidSingleCharacterToken()
        {
            var code = @"
@";

            ITokenizer tokenizer = new Compiler.Tokenizer.SimpleTokenizer();
            Assert.Throws<InvalidTokenParsingException>(() => tokenizer.EnumerateTokens(code.AsSpan()));
        }

        [Fact]
        public void TestInvalidCharacterToken()
        {
            var code = @"
abc@";

            ITokenizer tokenizer = new Compiler.Tokenizer.SimpleTokenizer();
            Assert.Throws<InvalidTokenParsingException>(() => tokenizer.EnumerateTokens(code.AsSpan()));
        }

        [Fact]
        public void TestTokenizerBeginsWithWhitespace()
        {
            var code = @"
    ;";

            ITokenizer tokenizer = new Compiler.Tokenizer.SimpleTokenizer();
            var tokens = tokenizer.EnumerateTokens(code.AsSpan());
            Assert.Equal(1, tokens.Length);
            Assert.IsType<SemiColonToken>(tokens[0]);
        }

        [Fact]
        public void TestTokenizerBeginsAndEndsWithWhitespace()
        {
            var code = @"
    ;   abc  ";

            ITokenizer tokenizer = new Compiler.Tokenizer.SimpleTokenizer();
            var tokens = tokenizer.EnumerateTokens(code.AsSpan());
            Assert.Equal(2, tokens.Length);
            Assert.IsType<SemiColonToken>(tokens[0]);
            Assert.IsType<IdentifierToken>(tokens[1]);
            Assert.Equal("abc", ((IdentifierToken)tokens[1]).Name);
        }

        [Fact]
        public void TestTokenizerEndsWithIdentifier()
        {
            var code = @"
    abc";

            ITokenizer tokenizer = new Compiler.Tokenizer.SimpleTokenizer();
            var tokens = tokenizer.EnumerateTokens(code.AsSpan());
            Assert.Equal(1, tokens.Length);
            Assert.IsType<IdentifierToken>(tokens[0]);
            Assert.Equal("abc", ((IdentifierToken)tokens[0]).Name);
        }

        [Fact]
        public void TestTokenizerIdentifierEndsWithSemiColon()
        {
            var code = @"
    abc;";

            ITokenizer tokenizer = new Compiler.Tokenizer.SimpleTokenizer();
            var tokens = tokenizer.EnumerateTokens(code.AsSpan());
            Assert.Equal(2, tokens.Length);
            Assert.IsType<IdentifierToken>(tokens[0]);
            Assert.Equal("abc", ((IdentifierToken)tokens[0]).Name);
            Assert.IsType<SemiColonToken>(tokens[1]);
        }

        [Fact]
        public void TestTokenizerInvalidSingleCharTokenFails()
        {
            var tokenizer = new Compiler.Tokenizer.SimpleTokenizer();
            Assert.Throws<InvalidTokenParsingException>(() => SimpleTokenizer.ParseCharacterToken('c', null));
        }

        [Theory]
        [InlineData("==", typeof(DoubleEqualsToken))]
        [InlineData("!=", typeof(NotEqualsToken))]
        [InlineData("<=", typeof(LessThenOrEqualToToken))]
        [InlineData(">=", typeof(GreaterThenOrEqualToToken))]
        public void TestTokenizerDoubleCharTokens(string str, Type tokenType)
        {
            ITokenizer tokenizer = new SimpleTokenizer();
            var tokens = tokenizer.EnumerateTokens(str.AsSpan());
            Assert.Equal(1, tokens.Length);
            Assert.IsType(tokenType, tokens[0]);
        }

        [Theory]
        [InlineData("= ", typeof(EqualsToken))]
        [InlineData("! ", typeof(ExclamationPointToken))]
        [InlineData("< ", typeof(LeftArrowToken))]
        [InlineData("> ", typeof(RightArrowToken))]
        public void TestTokenizerDoubleCharTokenCaseWithSpaceAfterFirstChar(string str, Type tokenType)
        {
            ITokenizer tokenizer = new SimpleTokenizer();
            var tokens = tokenizer.EnumerateTokens(str.AsSpan());
            Assert.Equal(1, tokens.Length);
            Assert.IsType(tokenType, tokens[0]);
        }

        [Theory]
        [InlineData("= =", typeof(EqualsToken), typeof(EqualsToken))]
        [InlineData("! =", typeof(ExclamationPointToken), typeof(EqualsToken))]
        [InlineData("< =", typeof(LeftArrowToken), typeof(EqualsToken))]
        [InlineData("> =", typeof(RightArrowToken), typeof(EqualsToken))]
        public void TestTokenizerDoubleCharTokensWithSpace(string str, Type firstToken, Type secondToken)
        {
            ITokenizer tokenizer = new SimpleTokenizer();
            var tokens = tokenizer.EnumerateTokens(str.AsSpan());
            Assert.Equal(2, tokens.Length);
            Assert.IsType(firstToken, tokens[0]);
            Assert.IsType(secondToken, tokens[1]);
        }
    }
}
