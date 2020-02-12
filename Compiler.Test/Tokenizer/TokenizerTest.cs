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

            ITokenizer tokenizer = new Compiler.Tokenizer.Tokenizer();
            var tokenParsingException = Assert.Throws<InvalidTokenParsingException>(() => tokenizer.EnumerateTokens(code));
            Assert.Empty(tokenParsingException.CurrentlyParsedToken);
            Assert.Equal('@', tokenParsingException.CauseCharacter);
        }

        [Fact]
        public void TestInvalidCharacterToken()
        {
            var code = @"
abc@";

            ITokenizer tokenizer = new Compiler.Tokenizer.Tokenizer();
            var tokenParsingException = Assert.Throws<InvalidTokenParsingException>(() => tokenizer.EnumerateTokens(code));
            Assert.Equal("abc", tokenParsingException.CurrentlyParsedToken);
            Assert.Equal('@', tokenParsingException.CauseCharacter);
        }

        [Fact]
        public void TestTokenizerBeginsWithWhitespace()
        {
            var code = @"
    ;";

            ITokenizer tokenizer = new Compiler.Tokenizer.Tokenizer();
            var tokens = tokenizer.EnumerateTokens(code);
            Assert.Equal(1, tokens.Count);
            Assert.IsType<SemiColonToken>(tokens[0]);
        }

        [Fact]
        public void TestTokenizerBeginsAndEndsWithWhitespace()
        {
            var code = @"
    ;   abc  ";

            ITokenizer tokenizer = new Compiler.Tokenizer.Tokenizer();
            var tokens = tokenizer.EnumerateTokens(code);
            Assert.Equal(2, tokens.Count);
            Assert.IsType<SemiColonToken>(tokens[0]);
            Assert.IsType<IdentifierToken>(tokens[1]);
            Assert.Equal("abc", ((IdentifierToken)tokens[1]).Name);
        }

        [Fact]
        public void TestTokenizerEndsWithIdentifier()
        {
            var code = @"
    abc";

            ITokenizer tokenizer = new Compiler.Tokenizer.Tokenizer();
            var tokens = tokenizer.EnumerateTokens(code);
            Assert.Equal(1, tokens.Count);
            Assert.IsType<IdentifierToken>(tokens[0]);
            Assert.Equal("abc", ((IdentifierToken)tokens[0]).Name);
        }

        [Fact]
        public void TestTokenizerIdentifierEndsWithSemiColon()
        {
            var code = @"
    abc;";

            ITokenizer tokenizer = new Compiler.Tokenizer.Tokenizer();
            var tokens = tokenizer.EnumerateTokens(code);
            Assert.Equal(2, tokens.Count);
            Assert.IsType<IdentifierToken>(tokens[0]);
            Assert.Equal("abc", ((IdentifierToken)tokens[0]).Name);
            Assert.IsType<SemiColonToken>(tokens[1]);
        }

        [Fact]
        public void TestTokenizerInvalidSingleCharTokenFails()
        {
            var tokenizer = new Compiler.Tokenizer.Tokenizer();
            Assert.Throws<InvalidTokenParsingException>(() => tokenizer.ParseSingleCharToken('c'));
        }
    }
}
