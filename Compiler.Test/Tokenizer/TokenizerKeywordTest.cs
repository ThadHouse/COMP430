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

        [Fact]
        public void TestKeywordsCantBeArrays()
        {
            Assert.Throws<InvalidTokenParsingException>(() =>
            {
                ReadOnlySpan<char> input = "return[]";
                SimpleTokenizer.ParseIdentifier(ref input);
            });
        }

        public static IEnumerable<object[]> Aliases => SimpleTokenizer.Aliases.Select(x => new object[] { x });

        [Theory]
        [MemberData(nameof(Aliases))]
        public void TestAliasedKeywordsParsing(string alias)
        {
            var tokenizer = new SimpleTokenizer();
            var tokens = tokenizer.EnumerateTokens(alias);
            Assert.Equal(1, tokens.Length);
            var aliasedToken = Assert.IsType<AliasedIdentifierToken>(tokens[0]);

        }

        [Theory]
        [InlineData("abc")]
        public void TestCustomIdentifierArray(string str)
        {
            str = str + "[]";
            var tokenizer = new SimpleTokenizer();
            var tokens = tokenizer.EnumerateTokens(str);
            Assert.Equal(1, tokens.Length);
            var idToken = Assert.IsType<IdentifierToken>(tokens[0]);
            Assert.Equal(str, idToken.Name);
        }

        [Theory]
        [MemberData(nameof(Aliases))]
        public void TestAliasedKeywordsArrayParsing(string alias)
        {
            var tokenizer = new SimpleTokenizer();
            if (alias == "void") return; // skip void
            var tokens = tokenizer.EnumerateTokens(alias + "[]");
            Assert.Equal(1, tokens.Length);
            var aliasedToken = Assert.IsType<AliasedIdentifierToken>(tokens[0]);

        }

        [Fact]
        public void TestParseIdentifierEmptyArray() {
            Assert.Throws<InvalidTokenParsingException>(() => {
               ReadOnlySpan<char> data = "";
               SimpleTokenizer.ParseIdentifier(ref data);
            });
        }

        [Fact]
        public void TestVoidArrayFails()
        {
            var tokenizer = new SimpleTokenizer();
            Assert.Throws<InvalidTokenParsingException>(() => tokenizer.EnumerateTokens("void[]"));
        }
    }
}
