using System;
using System.Collections.Generic;
using System.Text;
using Compiler.Tokenizer;
using Compiler.Tokenizer.Tokens;
using Xunit;

namespace Compiler.Test.Tokenizer
{
    public class TokenizerIntTest
    {
        //        [Fact]
        //        public void TestIntTokenWorksCorrectly()
        //        {
        //            var code = @"
        //123";

        //            ITokenizer tokenizer = new SimpleTokenizer();
        //            var tokens = tokenizer.EnumerateTokens(code);

        //            Assert.Equal(1, tokens.Length);
        //            var numberToken = Assert.IsType<NumericConstantToken>(tokens[0]);
        //            Assert.Equal("123", numberToken.Value);
        //        }

        //        [Fact]
        //        public void TestDoubleTokenWorksCorrectly()
        //        {
        //            var code = @"
        //12.3";

        //            ITokenizer tokenizer = new SimpleTokenizer();
        //            var tokens = tokenizer.EnumerateTokens(code);

        //            Assert.Equal(1, tokens.Length);
        //            var numberToken = Assert.IsType<NumericConstantToken>(tokens[0]);
        //            Assert.Equal("12.3", numberToken.Value);
        //        }

        //        [Fact]
        //        public void TestHexTokenWorksCorrectly()
        //        {
        //            var code = @"
        //0x123";

        //            ITokenizer tokenizer = new SimpleTokenizer();
        //            var tokens = tokenizer.EnumerateTokens(code);

        //            Assert.Equal(1, tokens.Length);
        //            var numberToken = Assert.IsType<NumericConstantToken>(tokens[0]);
        //            Assert.Equal("0x123", numberToken.Value);
        //        }


        [Theory]
        [InlineData("123", 123)]
        [InlineData("-123", -123)]
        public void TestTryParseNumericToken(string input, int expected)
        {
            var span = input.AsSpan();
            var token = SimpleTokenizer.TryParseNumericToken(ref span);
            Assert.NotNull(token);
            Assert.True(span.IsEmpty);

            var numberToken = Assert.IsType<IntegerConstantToken>(token);
            Assert.Equal(expected, numberToken.Value);
        }
    }
}
