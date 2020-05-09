using System;
using System.Collections.Generic;
using System.Text;
using Compiler.Tokenizer;
using Compiler.Tokenizer.Tokens;
using Xunit;

namespace Compiler.Test.Tokenizer
{
    public class Token
    {
        [Theory]
        [InlineData(Math.PI)]
        [InlineData(-Math.PI)]
        public void TestDoubleParsing(double value)
        {
            var input = value.ToString();
            ITokenizer tokenizer = new SimpleTokenizer();
            var tokens = tokenizer.EnumerateTokens(input.AsSpan());
            Assert.Equal(1, tokens.Length);
            var dctoken = Assert.IsType<DoubleConstantToken>(tokens[0]);
            Assert.Equal(value, dctoken.Value, 5);
        }

        [Fact]
        public void TestSingleDotNotParsingAsNumber()
        {
            ReadOnlySpan<char> input = ".";

            var result = SimpleTokenizer.TryParseNumericToken(ref input);

            Assert.Null(result);
            Assert.Equal(1, input.Length);
        }
    }
}
