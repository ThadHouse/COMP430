using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Compiler.Tokenizer;
using Xunit;

namespace Compiler.Test.Tokenizer
{
    public class TokenizerTypeValidations
    {
        [Fact]
        public void VerifyAllSingleCharTokensHaveACharValue()
        {
            var singleCharTokenClasses = typeof(ISingleCharToken).Assembly.GetTypes().Where(x => x.GetInterfaces().Contains(typeof(ISingleCharToken)));

            foreach (var tokenClass in singleCharTokenClasses)
            {
                var tokenCharValueField = tokenClass.GetField("CharValue", BindingFlags.Public | BindingFlags.Static);
                Assert.NotNull(tokenCharValueField);
            }
        }

        [Fact]
        public void VerifyAllSingleCharTokensHaveACharValueInTheAllowedCharsList()
        {
            var allowedChars = new List<char>(Compiler.Tokenizer.SimpleTokenizer.AllowedSingleCharacters);

            var singleCharTokenClasses = typeof(ISingleCharToken).Assembly.GetTypes().Where(x => x.GetInterfaces().Contains(typeof(ISingleCharToken)));

            foreach (var tokenClass in singleCharTokenClasses)
            {
                var tokenCharValueField = tokenClass.GetField("CharValue", BindingFlags.Public | BindingFlags.Static);
                char c = (char)tokenCharValueField.GetValue(null);
                allowedChars.Remove(c);
            }

            Assert.Empty(allowedChars);
        }

        [Fact]
        public void VerifyAllSingleCharTokensParseCorrectly()
        {
            var tokenizer = new Compiler.Tokenizer.SimpleTokenizer();
            var singleCharTokenClasses = typeof(ISingleCharToken).Assembly.GetTypes().Where(x => x.GetInterfaces().Contains(typeof(ISingleCharToken)));

            foreach (var tokenClass in singleCharTokenClasses)
            {
                var tokenCharValueField = tokenClass.GetField("CharValue", BindingFlags.Public | BindingFlags.Static);
                char c = (char)tokenCharValueField.GetValue(null);

                Assert.IsType(tokenClass, SimpleTokenizer.ParseSingleCharToken(c));
            }
        }

        [Fact]
        public void VerifyAllKeywordTokensHaveACharValue()
        {
            var tokenClasses = typeof(IKeywordToken).Assembly.GetTypes().Where(x => x.GetInterfaces().Contains(typeof(IKeywordToken)));

            foreach (var tokenClass in tokenClasses)
            {
                var tokenCharValueField = tokenClass.GetField("KeywordValue", BindingFlags.Public | BindingFlags.Static);
                Assert.NotNull(tokenCharValueField);
            }
        }

        [Fact]
        public void VerifyAllKeywordTokensHaveACharValueInTheAllowedCharsList()
        {
            var allowedChars = new List<string>(Compiler.Tokenizer.SimpleTokenizer.Keywords);

            var tokenClasses = typeof(IKeywordToken).Assembly.GetTypes().Where(x => x.GetInterfaces().Contains(typeof(IKeywordToken)));

            foreach (var tokenClass in tokenClasses)
            {
                var tokenCharValueField = tokenClass.GetField("KeywordValue", BindingFlags.Public | BindingFlags.Static);
                string c = (string)tokenCharValueField.GetValue(null);
                allowedChars.Remove(c);
            }

            Assert.Empty(allowedChars);
        }

        [Fact]
        public void VerifyAllKeywordTokensParseCorrectly()
        {
            var tokenizer = new Compiler.Tokenizer.SimpleTokenizer();
            var tokenClasses = typeof(IKeywordToken).Assembly.GetTypes().Where(x => x.GetInterfaces().Contains(typeof(IKeywordToken)));

            foreach (var tokenClass in tokenClasses)
            {
                var tokenCharValueField = tokenClass.GetField("KeywordValue", BindingFlags.Public | BindingFlags.Static);
                string c = (string)tokenCharValueField.GetValue(null);

                Assert.IsType(tokenClass, SimpleTokenizer.ParseToken(c.AsSpan(), false));
            }
        }
    }
}
