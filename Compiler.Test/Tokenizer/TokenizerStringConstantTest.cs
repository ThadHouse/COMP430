using System;
using System.Collections.Generic;
using System.Text;
using Compiler.Tokenizer;
using Compiler.Tokenizer.Exceptions;
using Compiler.Tokenizer.Tokens;
using Xunit;

namespace Compiler.Test.Tokenizer
{
    public class TokenizerStringConstantTest
    {
        [Fact]
        public void TestStringConstantWorksAsOnlyCode()
        {
            var code = @"
""abc""";

            ITokenizer tokenizer = new Compiler.Tokenizer.Tokenizer();
            var tokens = tokenizer.EnumerateTokens(code.AsSpan());
            Assert.Equal(1, tokens.Count);
            var charConstToken = Assert.IsType<StringConstantToken>(tokens[0]);
            Assert.Equal("abc", charConstToken.Value);
        }

        [Fact]
        public void TestStringConstantEmptyWorks()
        {
            var code = @"
""""";

            ITokenizer tokenizer = new Compiler.Tokenizer.Tokenizer();
            var tokens = tokenizer.EnumerateTokens(code.AsSpan());
            Assert.Equal(1, tokens.Count);
            var charConstToken = Assert.IsType<StringConstantToken>(tokens[0]);
            Assert.Equal("", charConstToken.Value);
        }

        [Fact]
        public void TestStringConstantTooShort()
        {
            var code = @"
""";

            ITokenizer tokenizer = new Compiler.Tokenizer.Tokenizer();
            Assert.Throws<StringConstantException>(() => tokenizer.EnumerateTokens(code.AsSpan()));
        }
    }
}
