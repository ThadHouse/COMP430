using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Xunit;
using Compiler.Tokenizer;
using Compiler.Tokenizer.Tokens;
using Compiler.Tokenizer.Exceptions;
using System.Reflection;

namespace Compiler.Test.Tokenizer
{
    public class TokenizerKeywordTest
    {
        [Fact]
        public void TestKeywordsParseAsSpecialIdentifiers()
        {
            var keywordClasses = typeof(IKeywordToken).Assembly.GetTypes().Where(x => x.GetInterfaces().Contains(typeof(IKeywordToken)));

            foreach (var tokenClass in keywordClasses)
            {
                var tokenCharValueField = tokenClass.GetField("KeywordValue", BindingFlags.Public | BindingFlags.Static);
                var keyword = (string)tokenCharValueField.GetValue(null);
                var tokenizer = new SimpleTokenizer();
                var tokens = tokenizer.EnumerateTokens(keyword);
                Assert.Equal(1, tokens.Length);
                Assert.IsType(tokenClass, tokens[0]);
            }
        }
    }
}
