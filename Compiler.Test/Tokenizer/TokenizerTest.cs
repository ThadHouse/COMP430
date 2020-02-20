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
            var tokenParsingException = Assert.Throws<InvalidTokenParsingException>(() => tokenizer.EnumerateTokens(code.AsSpan()));
            Assert.Empty(tokenParsingException.CurrentlyParsedToken);
            Assert.Equal('@', tokenParsingException.CauseCharacter);
        }

        [Fact]
        public void TestInvalidCharacterToken()
        {
            var code = @"
abc@";

            ITokenizer tokenizer = new Compiler.Tokenizer.SimpleTokenizer();
            var tokenParsingException = Assert.Throws<InvalidTokenParsingException>(() => tokenizer.EnumerateTokens(code.AsSpan()));
            Assert.Equal("abc", tokenParsingException.CurrentlyParsedToken);
            Assert.Equal('@', tokenParsingException.CauseCharacter);
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
            Assert.Throws<InvalidTokenParsingException>(() => SimpleTokenizer.ParseSingleCharToken('c', null));
        }
    }
}
