using System;
using System.Collections.Generic;
using System.Text;
using Compiler.Parser.Exceptions;
using Compiler.Parser.Nodes;
using Compiler.Tokenizer;
using Compiler.Tokenizer.Tokens;

namespace Compiler.Parser
{
    public class SimpleParser : IParser
    {
        private static ExpressionSyntaxNode ParseExpression(ref ReadOnlySpan<IToken> tokens, ISyntaxNode parent)
        {

            while (!tokens.IsEmpty)
            {
                var curToken = tokens[0];
                tokens = tokens.Slice(1);

                switch (curToken)
                {
                    // This is a terminal
                    case ThisToken _:
                        break;



                    // Identifier is a terminal
                    case IdentifierToken id:
                        if (tokens.IsEmpty)
                        {
                            throw new InvalidTokenException("Must be able to end with a semicolon");
                        }
                        if (tokens[0] is SemiColonToken)
                        {
                            //return 
                        }
                        else
                        {
                            throw new InvalidTokenException("Must end with semicolor");
                        }
                        break;
                }
            }

            throw new InvalidTokenException("Weird Tokens");
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
                        return new FieldSyntaxNode(parent, typeToken.Name, nameToken.Name, ParseExpression, ref tokens);
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

            if (tokens[0] is StaticToken)
            {
                isStatic = true;
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

            tokens = tokens.Slice(2);

            while (!tokens.IsEmpty)
            {
                var curToken = tokens[0];
                tokens = tokens.Slice(1);

                GC.KeepAlive(isStatic);

                switch (curToken)
                {
                    default:
                        throw new InvalidTokenException("Invalid token");
                }
            }

            throw new InvalidTokenException("Out of tokens?");
        }

        private static ConstructorSyntaxNode ParseConstructor(ref ReadOnlySpan<IToken> tokens, ISyntaxNode parent)
        {
            GC.KeepAlive(parent);

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

            while (!tokens.IsEmpty)
            {
                var curToken = tokens[0];
                tokens = tokens.Slice(1);

                switch (curToken)
                {
                    default:
                        throw new InvalidTokenException("Invalid token");
                }
            }

            throw new InvalidTokenException("Out of tokens?");
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

        public RootSyntaxNode ParseTokens(ReadOnlySpan<IToken> tokens)
        {

            var rootNode = new RootSyntaxNode();


            if (tokens.Length == 0)
            {
                return rootNode;
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

            return rootNode;
        }
    }
}
