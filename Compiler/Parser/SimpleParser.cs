using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Compiler.Parser.Exceptions;
using Compiler.Parser.Nodes;
using Compiler.Parser.Nodes.Statements;
using Compiler.Tokenizer;
using Compiler.Tokenizer.Tokens;

namespace Compiler.Parser
{
    public class SimpleParser : IParser
    {
        private static ExpressionSyntaxNode? ParseExpression(ref ReadOnlySpan<IToken> tokens, ISyntaxNode parent, ExpressionSyntaxNode? wouldBeLeft)
        {

            while (!tokens.IsEmpty)
            {
                var curToken = tokens[0];
                var prevTokens = tokens;
                tokens = tokens.Slice(1);

                // Special case the supported of token

                if (curToken is ISupportedOperationToken op)
                {
                    if (wouldBeLeft == null)
                    {
                        throw new InvalidTokenException("Left can't be null here");
                    }

                    var couldBeRight = ParseExpression(ref tokens, parent, null);

                    if (couldBeRight == null)
                    {
                        throw new InvalidTokenException("Right can't be null either");
                    }

                    return new ExpressionOpExpressionSyntaxNode(parent, wouldBeLeft, new OperationSyntaxNode(parent, op.Operation), couldBeRight);
                }
                else if (curToken is EqualsToken)
                {
                    if (wouldBeLeft == null)
                    {
                        throw new InvalidTokenException("Left can't be null here");
                    }

                    var couldBeRight = ParseExpression(ref tokens, parent, null);

                    if (couldBeRight == null)
                    {
                        throw new InvalidTokenException("Right can't be null either");
                    }

                    return new ExpressionEqualsExpressionSyntaxNode(parent, wouldBeLeft, couldBeRight);
                }
                else if (curToken is LeftBracketToken)
                {
                    if (tokens.Length < 2)
                    {
                        throw new InvalidTokenException("Not enough tokens left");
                    }

                    if (wouldBeLeft == null)
                    {
                        throw new InvalidTokenException("Left must not be null");
                    }

                    var exp = ParseExpression(ref tokens, parent, null);
                    if (exp == null)
                    {
                        throw new InvalidTokenException("Must have an expression");
                    }

                    if (tokens.IsEmpty)
                    {
                        throw new InvalidTokenException("There must be a token");
                    }

                    if (!(tokens[0] is RightBracketToken))
                    {
                        throw new InvalidTokenException("Must be a right bracket");
                    }

                    tokens = tokens.Slice(1);

                    return new ArrayIndexExpression(parent, wouldBeLeft, exp);
                }
                else if (curToken is DotToken)
                {
                    // Method call or lots of fields
                    // Next token must be an identifier
                    if (tokens.Length < 3)
                    {
                        throw new InvalidTokenException("Not enough tokens left");
                    }

                    if (!(tokens[0] is IdentifierToken id))
                    {
                        throw new InvalidTokenException("An ID token must be next");
                    }

                    if (wouldBeLeft == null)
                    {
                        throw new InvalidTokenException("Left can't be null");
                    }

                    if (tokens[1] is LeftParenthesisToken)
                    {
                        tokens = tokens.Slice(2);
                        var parameters = ParseCallParameters(ref tokens, parent);

                        var methodExpression = new MethodCallExpression(parent, wouldBeLeft, id.Name, parameters);

                        var continuingExpression = ParseExpression(ref tokens, parent, methodExpression);

                        if (continuingExpression != null)
                        {
                            return continuingExpression;
                        }

                        return methodExpression;
                    }
                    else if (tokens[1] is DotToken)
                    {
                        tokens = tokens.Slice(1);
                        return ParseExpression(ref tokens, parent, new VariableAccessExpression(parent, wouldBeLeft, id.Name));
                    }
                    else if (tokens[1] is RightParenthesisToken)
                    {
                        tokens = tokens.Slice(1);
                        return new VariableAccessExpression(parent, wouldBeLeft, id.Name);
                    }
                    else if (tokens[1] is EqualsToken)
                    {
                        tokens = tokens.Slice(2);

                        var couldBeRight = ParseExpression(ref tokens, parent, null);

                        if (couldBeRight == null)
                        {
                            throw new InvalidTokenException("Right cannot be null here");
                        }

                        return new ExpressionEqualsExpressionSyntaxNode(parent, new VariableAccessExpression(parent, wouldBeLeft, id.Name), couldBeRight);
                        ;
                    }
                    else if (tokens[1] is SemiColonToken)
                    {
                        tokens = tokens.Slice(1);
                        return new MethodReferenceExpression(parent, wouldBeLeft, id.Name);
                    }
                    else
                    {
                        throw new InvalidTokenException("A token must be handled here");
                    }




                }

                if (wouldBeLeft != null)
                {
                    tokens = prevTokens;
                    return null;
                }

                ExpressionSyntaxNode? variableNode;

                switch (curToken)
                {
                    case IntegerConstantToken numericConstant:
                        variableNode = new IntConstantSyntaxNode(parent, numericConstant.Value);
                        break;
                    case StringConstantToken stringConstant:
                        variableNode = new StringConstantNode(parent, stringConstant.Value);
                        break;
                    case IdentifierToken { Name: "this" } _:
                        variableNode = new ThisConstantNode(parent);
                        break;
                    case IdentifierToken { Name: "true" } _:
                        variableNode = new TrueConstantNode(parent);
                        break;
                    case IdentifierToken { Name: "false" } _:
                        variableNode = new FalseConstantNode(parent);
                        break;
                    case IdentifierToken { Name: "null" } _:
                        variableNode = new NullConstantNode(parent);
                        break;
                    case NewToken _:
                        {
                            if (tokens.Length < 3)
                            {
                                throw new InvalidTokenException("Need tokens to parse");
                            }
                            if (!(tokens[0] is IdentifierToken idToken))
                            {
                                throw new InvalidTokenException("Next token must be an identifier");
                            }
                            if (!(tokens[1] is LeftParenthesisToken))
                            {
                                throw new InvalidTokenException("Expected a left paranthesis");
                            }

                            tokens = tokens.Slice(2);

                            var parameters = ParseCallParameters(ref tokens, parent);

                            variableNode = new NewConstructorExpression(parent, idToken.Name, parameters);
                            break;
                        }
                    case NewArrToken _:
                        {
                            if (tokens.Length < 3)
                            {
                                throw new InvalidTokenException("Need tokens to parse");
                            }
                            if (!(tokens[0] is IdentifierToken idToken))
                            {
                                throw new InvalidTokenException("Next token must be an identifier");
                            }
                            if (!(tokens[1] is LeftParenthesisToken))
                            {
                                throw new InvalidTokenException("Expected a left paranthesis");
                            }

                            tokens = tokens.Slice(2);

                            var expression = ParseExpression(ref tokens, parent, null);

                            if (expression == null)
                            {
                                throw new InvalidTokenException("Must have an expression for a newarr");
                            }

                            if (tokens.Length < 1)
                            {
                                throw new InvalidTokenException("Need tokens to parse");
                            }
                            if (!(tokens[0] is RightParenthesisToken))
                            {
                                throw new InvalidTokenException("Next token must be a right paranthesis");
                            }
                            tokens = tokens.Slice(1);


                            variableNode = new NewArrExpression(parent, idToken.Name, expression);
                            break;
                        }
                    case IdentifierToken id:
                        variableNode = new VariableSyntaxNode(parent, id.Name);
                        break;
                    default:
                        tokens = prevTokens;
                        return null;
                }

                // If its empty, we're done
                if (tokens.IsEmpty)
                {
                    return variableNode;
                }

                var attemptToParseLower = ParseExpression(ref tokens, parent, variableNode);

                return attemptToParseLower ?? variableNode;
            }

            throw new InvalidTokenException("Weird Tokens");
        }

        private static VariableDeclarationNode? ParseVariableDeclaration(ref ReadOnlySpan<IToken> tokens, ISyntaxNode parent)
        {
            // Field must have at least 3 more tokens
            if (tokens.Length < 3)
            {
                return null;
            }

            string? typeName = null;

            if (tokens[0] is IdentifierToken typeToken)
            {
                typeName = typeToken.Name;
            }
            else if (tokens[0] is AutoToken)
            {
                typeName = null;
            }
            else
            {
                return null;
            }

            if (!(tokens[1] is IdentifierToken nameToken))
            {
                return null;
            }

            tokens = tokens.Slice(2);

            while (!tokens.IsEmpty)
            {
                var curToken = tokens[0];
                tokens = tokens.Slice(1);

                switch (curToken)
                {
                    case SemiColonToken _:
                        if (typeName == null)
                        {
                            throw new InvalidTokenException("Can only use auto with expressions");
                        }
                        return new VariableDeclarationNode(parent, typeName, nameToken.Name, null, ref tokens);
                    case EqualsToken _:
                        return new VariableDeclarationNode(parent, typeName, nameToken.Name, (ref ReadOnlySpan<IToken> tkns, ISyntaxNode pnt) =>
                        {
                            var toRet = ParseExpression(ref tkns, pnt, null);
                            if (tkns.IsEmpty)
                            {
                                throw new InvalidTokenException("Out of tokens, must end with a ;");
                            }
                            if (tkns[0] is SemiColonToken)
                            {
                                tkns = tkns.Slice(1);
                                if (toRet == null)
                                {
                                    throw new InvalidTokenException("Must have an expression here");
                                }
                                return toRet;
                            }
                            else
                            {
                                throw new InvalidTokenException("Expected a ;");
                            }
                        }, ref tokens);
                    default:
                        return null;
                }
            }

            throw new InvalidTokenException("Out of tokens?");
        }

        private static StatementSyntaxNode? ParseStatement(ref ReadOnlySpan<IToken> tokens, ISyntaxNode parent)
        {
            var parsedVarDec = ParseVariableDeclaration(ref tokens, parent);

            if (parsedVarDec != null)
            {
                return parsedVarDec;
            }

            var beforeExpressionToken = tokens;

            var parsedExpression = ParseExpression(ref tokens, parent, null);

            if (parsedExpression != null)
            {
                if (tokens.Length < 1)
                {
                    throw new InvalidTokenException("There must be another token");
                }
                if (!(tokens[0] is SemiColonToken))
                {
                    throw new InvalidTokenException("Expected a semicolon");
                }
                tokens = tokens.Slice(1);
                return parsedExpression;
            }

            while (!tokens.IsEmpty)
            {
                var curToken = tokens[0];
                var prevTokens = tokens;
                tokens = tokens.Slice(1);



                switch (curToken)
                {
                    case RightBraceToken _:
                        tokens = prevTokens;
                        return null;
                    case ReturnToken _:
                        {
                            if (tokens.Length < 1)
                            {
                                throw new InvalidTokenException("Must have tokens");
                            }

                            if (tokens[0] is SemiColonToken)
                            {
                                tokens = tokens.Slice(1);
                                return new ReturnStatementNode(parent, null, ref tokens);
                            }

                            return new ReturnStatementNode(parent, (ref ReadOnlySpan<IToken> tkns, ISyntaxNode pnt) =>
                            {
                                var toRet = ParseExpression(ref tkns, pnt, null);
                                if (tkns.IsEmpty)
                                {
                                    throw new InvalidTokenException("Out of tokens, must end with a ;");
                                }
                                if (tkns[0] is SemiColonToken)
                                {
                                    tkns = tkns.Slice(1);
                                    if (toRet == null)
                                    {
                                        throw new InvalidTokenException("Must have an expression here");
                                    }
                                    return toRet;
                                }
                                else
                                {
                                    throw new InvalidTokenException("Expected a ;");
                                }
                            }, ref tokens);
                        }
                    case IfToken _:
                        {
                            if (tokens.Length < 7)
                            {
                                throw new InvalidTokenException("Must have tokens");
                            }

                            tokens = tokens.Slice(1);

                            var exp = ParseExpression(ref tokens, parent, null);

                            if (exp == null)
                            {
                                throw new InvalidTokenException("Must have an expression");
                            }

                            if (tokens.Length < 6)
                            {
                                throw new InvalidTokenException("Not enough tokens");
                            }

                            if (!(tokens[0] is RightParenthesisToken))
                            {
                                throw new InvalidTokenException("Must have a right paranthesis");
                            }

                            if (!(tokens[1] is LeftBraceToken))
                            {
                                throw new InvalidTokenException("Must have a left brace");
                            }

                            tokens = tokens.Slice(2);

                            var statements = new List<StatementSyntaxNode>();

                            while (true)
                            {
                                var stmt = ParseStatement(ref tokens, parent);

                                if (stmt == null)
                                {
                                    break;
                                }
                                statements.Add(stmt);
                            }

                            if (tokens.Length < 3)
                            {
                                throw new InvalidTokenException("Must have tokens");
                            }

                            if (!(tokens[0] is RightBraceToken))
                            {
                                throw new InvalidTokenException("Must have a right brace");
                            }

                            if (!(tokens[1] is ElseToken))
                            {
                                throw new InvalidTokenException("Must have an else");
                            }

                            if (!(tokens[2] is LeftBraceToken))
                            {
                                throw new InvalidTokenException("Must have a left brace");
                            }

                            tokens = tokens.Slice(3);

                            var elseStatements = new List<StatementSyntaxNode>();

                            while (true)
                            {
                                var stmt = ParseStatement(ref tokens, parent);

                                if (stmt == null)
                                {
                                    break;
                                }
                                elseStatements.Add(stmt);
                            }

                            if (tokens.Length < 1)
                            {
                                throw new InvalidTokenException("Must have tokens");
                            }

                            if (!(tokens[0] is RightBraceToken))
                            {
                                throw new InvalidTokenException("Must have a right brace");
                            }

                            tokens = tokens.Slice(1);

                            return new IfElseStatement(parent, exp, statements, elseStatements);
                        }


                    case WhileToken _:
                        {
                            if (tokens.Length < 4)
                            {
                                throw new InvalidTokenException("Must have tokens");
                            }
                            if (!(tokens[0] is LeftParenthesisToken))
                            {
                                throw new InvalidTokenException("Must have a left paranthesis");
                            }

                            tokens = tokens.Slice(1);

                            var exp = ParseExpression(ref tokens, parent, null);

                            if (exp == null)
                            {
                                throw new InvalidTokenException("Must have an expression");
                            }

                            if (tokens.Length < 3)
                            {
                                throw new InvalidTokenException("Not enough tokens");
                            }

                            if (!(tokens[0] is RightParenthesisToken))
                            {
                                throw new InvalidTokenException("Must have a right paranthesis");
                            }

                            if (!(tokens[1] is LeftBraceToken))
                            {
                                throw new InvalidTokenException("Must have a left brace");
                            }

                            tokens = tokens.Slice(2);

                            var statements = new List<StatementSyntaxNode>();

                            while (true)
                            {
                                var stmt = ParseStatement(ref tokens, parent);

                                if (stmt == null)
                                {
                                    break;
                                }
                                statements.Add(stmt);
                            }

                            if (tokens.Length < 1)
                            {
                                throw new InvalidTokenException("Must have tokens");
                            }

                            if (!(tokens[0] is RightBraceToken))
                            {
                                throw new InvalidTokenException("Must have a right brace");
                            }

                            tokens = tokens.Slice(1);

                            return new WhileStatement(parent, exp, statements);

                            //return new StatementSyntaxNode(parent, stmts);
                        }
                    default:
                        throw new InvalidTokenException("Unknown statement");
                }
            }

            throw new InvalidTokenException("Out of tokens?");
        }

        private static IReadOnlyList<CallParameterSyntaxNode> ParseCallParameters(ref ReadOnlySpan<IToken> tokens, ISyntaxNode parent)
        {
            var parameters = new List<CallParameterSyntaxNode>();

            while (!tokens.IsEmpty)
            {
                var curToken = tokens[0];
                var prevTokens = tokens;
                tokens = tokens.Slice(1);

                switch (curToken)
                {
                    case RightParenthesisToken _:
                        return parameters;

                    case RefToken _:
                        if (tokens.Length < 2)
                        {
                            throw new InvalidTokenException("Not enough tokens");
                        }
                        if (!(tokens[0] is IdentifierToken nameToken))
                        {
                            throw new InvalidTokenException("Must be a name token");
                        }

                        if (tokens[1] is CommaToken)
                        {
                            tokens = tokens.Slice(2);
                            parameters.Add(new CallParameterSyntaxNode(parent, new VariableSyntaxNode(parent, nameToken.Name), true));
                        }
                        else if (tokens[1] is RightParenthesisToken)
                        {
                            tokens = tokens.Slice(2);
                            parameters.Add(new CallParameterSyntaxNode(parent, new VariableSyntaxNode(parent, nameToken.Name), true));
                            return parameters;
                        }
                        else
                        {
                            throw new InvalidTokenException("Invalid token found in character defintion");
                        }
                        break;
                    default:
                        if (tokens.Length < 1)
                        {
                            throw new InvalidTokenException("Not enough tokens");
                        }

                        tokens = prevTokens;

                        var exp = ParseExpression(ref tokens, parent, null);

                        if (exp == null)
                        {
                            throw new InvalidTokenException("Must be an expression here");
                        }

                        if (tokens.IsEmpty)
                        {
                            throw new InvalidTokenException("There must be another token to handle");
                        }

                        if (tokens[0] is CommaToken)
                        {
                            tokens = tokens.Slice(1);
                            parameters.Add(new CallParameterSyntaxNode(parent, exp, false));
                        }
                        else if (tokens[0] is RightParenthesisToken)
                        {
                            tokens = tokens.Slice(1);
                            parameters.Add(new CallParameterSyntaxNode(parent, exp, false));
                            return parameters;
                        }
                        break;
                }
            }

            throw new InvalidTokenException("Out of tokens");
        }

        private static FieldSyntaxNode ParseField(ref ReadOnlySpan<IToken> tokens, ISyntaxNode parent)
        {
            // Field must have at least 3 more tokens
            if (tokens.Length < 3)
            {
                throw new InvalidTokenException("Not enough tokens");
            }

            if (!(tokens[0] is IdentifierToken typeToken))
            {
                throw new InvalidTokenException("First token must be a type token");
            }

            if (!(tokens[1] is IdentifierToken nameToken))
            {
                throw new InvalidTokenException("Name must be the 2nd token");
            }

            tokens = tokens.Slice(2);

            while (!tokens.IsEmpty)
            {
                var curToken = tokens[0];
                tokens = tokens.Slice(1);

                switch (curToken)
                {
                    case SemiColonToken _:
                        return new FieldSyntaxNode(parent, typeToken.Name, nameToken.Name, null, ref tokens);
                    case EqualsToken _:
                        return new FieldSyntaxNode(parent, typeToken.Name, nameToken.Name, (ref ReadOnlySpan<IToken> tkns, ISyntaxNode pnt) =>
                        {
                            var toRet = ParseExpression(ref tkns, pnt, null);
                            if (tkns.IsEmpty)
                            {
                                throw new InvalidTokenException("Out of tokens, must end with a ;");
                            }
                            if (tkns[0] is SemiColonToken)
                            {
                                tkns = tkns.Slice(1);
                                if (toRet == null)
                                {
                                    throw new InvalidTokenException("Must have an expression here");
                                }
                                return toRet;
                            }
                            else
                            {
                                throw new InvalidTokenException("Expected a ;");
                            }
                        }, ref tokens);
                    default:
                        throw new InvalidTokenException("Invalid token");
                }
            }

            throw new InvalidTokenException("Out of tokens?");
        }

        private static MethodSyntaxNode ParseMethod(ref ReadOnlySpan<IToken> tokens, ISyntaxNode parent)
        {
            GC.KeepAlive(parent);

            // Method must have at least 6 more tokens: type name() {}
            if (tokens.Length < 6)
            {
                throw new InvalidTokenException("Not enough tokens");
            }

            bool isStatic = false;
            bool isEntryPoint = false;

            if (tokens[0] is StaticToken)
            {
                isStatic = true;
                tokens = tokens.Slice(1);
            }
            else if (tokens[0] is EntryPointToken)
            {
                isStatic = true;
                isEntryPoint = true;
                tokens = tokens.Slice(1);
            }

            // Do the 6 token check again
            if (tokens.Length < 6)
            {
                throw new InvalidTokenException("Not enough tokens");
            }

            if (!(tokens[0] is IdentifierToken typeToken))
            {
                throw new InvalidTokenException("First token must be a type token");
            }

            if (!(tokens[1] is IdentifierToken nameToken))
            {
                throw new InvalidTokenException("Name must be the 2nd token");
            }

            if (!(tokens[2] is LeftParenthesisToken))
            {
                throw new InvalidTokenException("Must be left paranthesis");
            }

            tokens = tokens.Slice(3);

            (var parameters, var statements) = ParseMethodLike(ref tokens, parent);

            return new MethodSyntaxNode(parent, typeToken.Name, nameToken.Name, parameters, isStatic, isEntryPoint, statements);
        }

        private static (IReadOnlyList<ParameterDefinitionSyntaxNode> parameters, List<StatementSyntaxNode> statements) ParseMethodLike(ref ReadOnlySpan<IToken> tokens, ISyntaxNode parent)
        {
            var parameters = ParseParameters(ref tokens, parent);

            if (tokens.Length < 2)
            {
                throw new InvalidTokenException("Not enough tokens");
            }

            if (!(tokens[0] is LeftBraceToken))
            {
                throw new InvalidTokenException("Must be a left brace");
            }

            tokens = tokens.Slice(1);

            var statements = new List<StatementSyntaxNode>();

            while (true)
            {
                var stmt = ParseStatement(ref tokens, parent);

                if (stmt == null)
                {
                    break;
                }
                statements.Add(stmt);
            }

            if (tokens.Length < 1)
            {
                throw new InvalidTokenException("Must have more tokens");
            }

            if (!(tokens[0] is RightBraceToken))
            {
                throw new InvalidTokenException("Must have a right brace");
            }

            tokens = tokens.Slice(1);

            return (parameters, statements);
        }

        private static ConstructorSyntaxNode ParseConstructor(ref ReadOnlySpan<IToken> tokens, ISyntaxNode parent)
        {
            // Constructor must have at least 4 more tokens () {}
            if (tokens.Length < 4)
            {
                throw new InvalidTokenException("Not enough tokens");
            }

            if (!(tokens[0] is LeftParenthesisToken))
            {
                throw new InvalidTokenException("First token must be an open parenthesis");
            }
            tokens = tokens.Slice(1);


            (var parameters, var statements) = ParseMethodLike(ref tokens, parent);

            return new ConstructorSyntaxNode(parent, parameters, statements);
        }

        public static IReadOnlyList<ParameterDefinitionSyntaxNode> ParseParameters(ref ReadOnlySpan<IToken> tokens, ISyntaxNode parent)
        {
            var parameters = new List<ParameterDefinitionSyntaxNode>();

            while (!tokens.IsEmpty)
            {
                var curToken = tokens[0];
                tokens = tokens.Slice(1);

                switch (curToken)
                {
                    case RightParenthesisToken _:
                        return parameters;

                    case RefToken _:
                        {
                            if (tokens.Length < 3)
                            {
                                throw new InvalidTokenException("Not enough tokens");
                            }
                            if (!(tokens[0] is IdentifierToken typeToken))
                            {
                                throw new InvalidTokenException("Must be a type token");
                            }
                            if (!(tokens[1] is IdentifierToken nameToken))
                            {
                                throw new InvalidTokenException("Must be a name token");
                            }

                            if (tokens[2] is CommaToken)
                            {
                                tokens = tokens.Slice(3);
                                parameters.Add(new ParameterDefinitionSyntaxNode(parent, typeToken.Name, nameToken.Name, true));
                            }
                            else if (tokens[2] is RightParenthesisToken)
                            {
                                tokens = tokens.Slice(3);
                                parameters.Add(new ParameterDefinitionSyntaxNode(parent, typeToken.Name, nameToken.Name, true));
                                return parameters;
                            }
                            else
                            {
                                throw new InvalidTokenException("Invalid token found in character defintion");
                            }


                            break;
                        }
                    case IdentifierToken typeOuterToken:
                        {
                            if (tokens.Length < 2)
                            {
                                throw new InvalidTokenException("Not enough tokens");
                            }
                            if (!(tokens[0] is IdentifierToken nameToken))
                            {
                                throw new InvalidTokenException("Must be a name token");
                            }

                            if (tokens[1] is CommaToken)
                            {
                                tokens = tokens.Slice(2);
                                parameters.Add(new ParameterDefinitionSyntaxNode(parent, typeOuterToken.Name, nameToken.Name, false));
                            }
                            else if (tokens[1] is RightParenthesisToken)
                            {
                                tokens = tokens.Slice(2);
                                parameters.Add(new ParameterDefinitionSyntaxNode(parent, typeOuterToken.Name, nameToken.Name, false));
                                return parameters;
                            }
                            else
                            {
                                throw new InvalidTokenException("Invalid token found in character defintion");
                            }

                            break;
                        }
                    default:
                        throw new InvalidTokenException("Unknown token found");
                }
            }

            throw new InvalidTokenException("Not enough tokens left to parse");
        }

        public static DelegateSyntaxNode ParseDelegate(ref ReadOnlySpan<IToken> tokens, ISyntaxNode parent)
        {
            // Need 5 more tokens: type, name, ();
            if (tokens.Length < 5)
            {
                throw new InvalidTokenException("Not enough tokens");
            }

            if (!(tokens[0] is IdentifierToken typeToken))
            {
                throw new InvalidTokenException("First token must be a type token");
            }

            if (!(tokens[1] is IdentifierToken nameToken))
            {
                throw new InvalidTokenException("Second token must be a name token");
            }

            if (!(tokens[2] is LeftParenthesisToken))
            {
                throw new InvalidTokenException("3rd token must be a left parenthesis");
            }

            tokens = tokens.Slice(3);


            var parameters = ParseParameters(ref tokens, parent);

            if (tokens.IsEmpty || !(tokens[0] is SemiColonToken))
            {
                throw new InvalidTokenException("Must end with a semicolon");
            }
            else
            {
                tokens = tokens.Slice(1);
            }

            return new DelegateSyntaxNode(parent, parameters, typeToken.Name, nameToken.Name);
        }

        public static ClassSyntaxNode ParseClass(ref ReadOnlySpan<IToken> tokens, ISyntaxNode parent)
        {



            // Need 3 more tokens: name {}
            if (tokens.Length < 3)
            {
                throw new InvalidTokenException("Not enough tokens");
            }

            if (!(tokens[0] is IdentifierToken typeToken))
            {
                throw new InvalidTokenException("First token must be a type token");
            }

            if (!(tokens[1] is LeftBraceToken))
            {
                throw new InvalidTokenException("Need a left brace as the first token");
            }

            var classSyntaxNode = new ClassSyntaxNode(parent, typeToken.Name);

            tokens = tokens.Slice(2);

            while (!tokens.IsEmpty)
            {
                var curToken = tokens[0];
                tokens = tokens.Slice(1);

                switch (curToken)
                {
                    case LeftBraceToken _:
                        throw new InvalidTokenException("Too many left braces");
                    case RightBraceToken _:
                        return classSyntaxNode;
                    case ConstructorToken _:
                        classSyntaxNode.Constructors.Add(ParseConstructor(ref tokens, parent));
                        break;
                    case MethodToken _:
                        classSyntaxNode.Methods.Add(ParseMethod(ref tokens, parent));
                        break;
                    case FieldToken _:
                        classSyntaxNode.Fields.Add(ParseField(ref tokens, parent));
                        break;
                    default:
                        throw new InvalidTokenException($"Unexpected token {curToken}");
                }
            }

            throw new InvalidTokenException("Out of tokens?");
        }

        public void ParseTokens(ReadOnlySpan<IToken> tokens, RootSyntaxNode rootNode)
        {
            if (rootNode == null)
            {
                throw new ArgumentNullException(nameof(rootNode));
            }

            while (!tokens.IsEmpty)
            {
                var curToken = tokens[0];
                tokens = tokens.Slice(1);

                switch (curToken)
                {
                    case ClassToken _:
                        rootNode.Classes.Add(ParseClass(ref tokens, rootNode));
                        break;
                    case DelegateToken _:
                        rootNode.Delegates.Add(ParseDelegate(ref tokens, rootNode));
                        break;
                    default:
                        throw new InvalidTokenException("Only expecting a delegate or a class");
                }
            }
        }
    }
}
