using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Compiler.Tokenizer;
using Compiler.Tokenizer.Tokens;

namespace Compiler.Test.Tokenizer
{
    public class TokenizerTest
    {
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
    }
}
