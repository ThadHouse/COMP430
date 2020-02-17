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

            ITokenizer tokenizer = new Compiler.Tokenizer.SimpleTokenizer();
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

            ITokenizer tokenizer = new Compiler.Tokenizer.SimpleTokenizer();
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

            ITokenizer tokenizer = new Compiler.Tokenizer.SimpleTokenizer();
            Assert.Throws<StringConstantException>(() => tokenizer.EnumerateTokens(code.AsSpan()));
        }

        [Theory]
        [InlineData(@"\tabc", "\tabc")]
        [InlineData(@"\ta\nbc", "\ta\nbc")]
        [InlineData(@"\tabc\a", "\tabc\a")]
        [InlineData(@"\ta\u1234bc\a", "\ta\u1234bc\a")]
        [InlineData(@"\ta\x123 bc\a", "\ta\x123 bc\a")]
        public void TestEscapedString(string input, string result)
        {
            var code = $@"
""{input}""";

            ITokenizer tokenizer = new Compiler.Tokenizer.SimpleTokenizer();
            var tokens = tokenizer.EnumerateTokens(code.AsSpan());
            Assert.Equal(1, tokens.Count);
            var charConstToken = Assert.IsType<StringConstantToken>(tokens[0]);
            Assert.Equal(result, charConstToken.Value);
        }
    }
}
