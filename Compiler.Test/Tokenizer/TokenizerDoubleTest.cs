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
        [Fact]
        public void TestDoubleParsing() {
            var input = Math.PI.ToString();
            ITokenizer tokenizer = new SimpleTokenizer();
            var tokens = tokenizer.EnumerateTokens(input.AsSpan());
            Assert.Equal(1, tokens.Length);
            var dctoken = Assert.IsType<DoubleConstantToken>(tokens[0]);
            Assert.Equal(Math.PI, dctoken.Value, 5);
        }
    }
}
