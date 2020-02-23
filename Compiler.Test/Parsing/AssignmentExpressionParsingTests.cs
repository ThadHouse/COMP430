using System;
using System.Collections.Generic;
using System.Text;
using Compiler.Parser;
using Compiler.Parser.Nodes;
using Compiler.Parser.Nodes.Statements;
using Compiler.Tokenizer;
using Compiler.Tokenizer.Tokens;
using Xunit;

namespace Compiler.Test.Parsing
{
    public class AssignmentExpressionParsingTests
    {
        private readonly IReadOnlyList<IToken> startingTokens = new List<IToken>()
        {
            new ClassToken(),
            new IdentifierToken("MyClass"),
            new LeftBraceToken(),
            new MethodToken(),
            new AliasedIdentifierToken("System.Void", "void"),
            new IdentifierToken("MyFunction"),
            new LeftParenthesisToken(),
            new RightParenthesisToken(),
            new LeftBraceToken(),
        };

        private readonly IReadOnlyList<IToken> endingTokens = new List<IToken>()
        {
            new RightBraceToken(),
            new RightBraceToken(),
        };

        private readonly IParser parser = new SimpleParser();
        private readonly RootSyntaxNode rootNode = new RootSyntaxNode();

        private ReadOnlySpan<IToken> CombineTokens(IReadOnlyList<IToken> tokens)
        {
            var list = new List<IToken>(startingTokens);
            list.AddRange(tokens);
            list.AddRange(endingTokens);
            return list.ToArray();
        }

        private StatementSyntaxNode GetStatement()
        {
            Assert.Equal(1, rootNode.Classes.Count);
            Assert.Equal(0, rootNode.Delegates.Count);
            var cls = rootNode.Classes[0];
            Assert.Equal(0, cls.Constructors.Count);
            Assert.Equal(0, cls.Fields.Count);
            Assert.Equal("MyClass", cls.Name);
            Assert.Equal(1, cls.Methods.Count);
            var method = cls.Methods[0];
            Assert.Equal("MyFunction", method.Name);
            Assert.Equal("System.Void", method.ReturnType);
            Assert.False(method.IsEntryPoint);
            Assert.False(method.IsStatic);
            Assert.Equal(0, method.Parameters.Count);
            Assert.Equal(1, method.Statements.Count);
            Assert.Equal(0, method.Statements[0].Statements.Count);
            return method.Statements[0];
        }

        [Fact]
        public void TestAssignArrayToVariable()
        {
            var list = new List<IToken>()
            {
                new IdentifierToken("a"),
                new EqualsToken(),
                new IdentifierToken("b"),
                new LeftBracketToken(),
                new IntegerConstantToken(42),
                new RightBracketToken(),
                new SemiColonToken(),
            };

            var combined = CombineTokens(list);
            parser.ParseTokens(combined, rootNode);
            var statement = GetStatement();
            var equals = Assert.IsType<ExpressionEqualsExpressionSyntaxNode>(statement);
            var lhs = Assert.IsType<VariableSyntaxNode>(equals.Left);
            var rhs = Assert.IsType<ArrayIndexExpression>(equals.Right);
            Assert.Equal("a", lhs.Name);
            var arrGetterExpression = Assert.IsType<VariableSyntaxNode>(rhs.Expression);
            var arrIdxExpression = Assert.IsType<IntConstantSyntaxNode>(rhs.LengthExpression);
            Assert.Equal("b", arrGetterExpression.Name);
            Assert.Equal(42, arrIdxExpression.Value);
        }

        [Fact]
        public void TestAssignVariableToArray()
        {
            var list = new List<IToken>()
            {
                new IdentifierToken("b"),
                new LeftBracketToken(),
                new IntegerConstantToken(42),
                new RightBracketToken(),
                new EqualsToken(),
                new IdentifierToken("a"),
                new SemiColonToken(),
            };

            var combined = CombineTokens(list);
            parser.ParseTokens(combined, rootNode);
            var statement = GetStatement();
            var equals = Assert.IsType<ExpressionEqualsExpressionSyntaxNode>(statement);
            var lhs = Assert.IsType<ArrayIndexExpression>(equals.Left);
            var rhs = Assert.IsType<VariableSyntaxNode>(equals.Right);
            Assert.Equal("a", rhs.Name);
            var arrGetterExpression = Assert.IsType<VariableSyntaxNode>(lhs.Expression);
            var arrIdxExpression = Assert.IsType<IntConstantSyntaxNode>(lhs.LengthExpression);
            Assert.Equal("b", arrGetterExpression.Name);
            Assert.Equal(42, arrIdxExpression.Value);
        }
    }
}
