using System;
using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.Syntax.Expressions;

namespace BlingoEngine.Legacy.Lingo.Syntax;

/// <summary>
/// Parses expressions from a stream of legacy Lingo tokens while honoring operator precedence.
/// </summary>
public sealed class BlLingoExpressionParser
{
    /// <summary>
    /// Parses an expression from the supplied tokens starting at index <c>0</c>.
    /// </summary>
    public BlExpression ParseExpression(IReadOnlyList<BlSyntaxToken> tokens)
    {
        _ = ParseExpression(tokens, 0, out _, out var expression);
        return expression;
    }

    /// <summary>
    /// Parses an expression from the supplied tokens starting at <paramref name="startIndex"/>.
    /// </summary>
    public BlExpression ParseExpression(IReadOnlyList<BlSyntaxToken> tokens, int startIndex, out int endIndex)
    {
        endIndex = ParseExpression(tokens, startIndex, out var lastTokenIndex, out var expression)
            ? Math.Max(lastTokenIndex, startIndex)
            : startIndex;
        return expression;
    }

    private static bool ParseExpression(
        IReadOnlyList<BlSyntaxToken> tokens,
        int startIndex,
        out int lastTokenIndex,
        out BlExpression expression)
    {
        ArgumentNullException.ThrowIfNull(tokens);

        if (tokens.Count == 0)
        {
            lastTokenIndex = -1;
            expression = new BlMissingExpression(null);
            return false;
        }

        var parser = new Parser(tokens, startIndex);
        expression = parser.ParseExpression();
        lastTokenIndex = parser.LastTokenIndex;
        return true;
    }

    /// <summary>
    /// Implements the Pratt-style precedence parser over the token stream.
    /// </summary>
    private sealed class Parser
    {
        private readonly IReadOnlyList<BlSyntaxToken> _tokens;
        private int _position;
        private int _lastTokenIndex;

        internal Parser(IReadOnlyList<BlSyntaxToken> tokens, int startIndex)
        {
            _tokens = tokens;
            _position = Math.Clamp(startIndex, 0, Math.Max(tokens.Count - 1, 0));
            _lastTokenIndex = _position - 1;
        }

        public int LastTokenIndex => _lastTokenIndex;

        public BlExpression ParseExpression(int minimumPrecedence = 0)
        {
            var left = ParseUnary();

            while (true)
            {
                if (IsAtEnd)
                {
                    break;
                }

                var current = Current;
                if (!BlLingoOperatorFacts.TryGetBinaryOperatorInfo(current, out var operatorKind, out var precedence, out var isRightAssociative))
                {
                    break;
                }

                if (precedence < minimumPrecedence)
                {
                    break;
                }

                var operatorToken = Advance();
                var right = ParseExpression(isRightAssociative ? precedence : precedence + 1);
                left = new BlBinaryExpression(operatorKind, operatorToken, left, right);
            }

            return left;
        }

        private BlExpression ParseUnary()
        {
            if (IsAtEnd)
            {
                return new BlMissingExpression(Current);
            }

            var token = Current;
            if (BlLingoOperatorFacts.TryGetUnaryOperatorKind(token, out var operatorKind))
            {
                var operatorToken = Advance();
                var operand = ParseUnary();
                return new BlUnaryExpression(operatorKind, operatorToken, operand);
            }

            return ParsePrimary();
        }

        private BlExpression ParsePrimary()
        {
            var token = Current;

            if (token.Kind == BlSyntaxKind.LeftParenthesisToken)
            {
                var open = Advance();
                var innerExpression = ParseExpression();
                BlSyntaxToken? close = null;
                if (!IsAtEnd && Current.Kind == BlSyntaxKind.RightParenthesisToken)
                {
                    close = Advance();
                }

                return new BlGroupingExpression(open, innerExpression, close);
            }

            if (token.Kind == BlSyntaxKind.IdentifierToken)
            {
                return new BlIdentifierExpression(Advance());
            }

            if (token.Kind == BlSyntaxKind.NumberToken)
            {
                return new BlLiteralExpression(BlExpressionKind.NumberLiteral, Advance());
            }

            if (token.Kind == BlSyntaxKind.StringLiteralToken)
            {
                return new BlLiteralExpression(BlExpressionKind.StringLiteral, Advance());
            }

            if (token.Kind == BlSyntaxKind.SymbolToken)
            {
                return new BlLiteralExpression(BlExpressionKind.SymbolLiteral, Advance());
            }

            if (token.Kind == BlSyntaxKind.KeywordToken
                && !BlLingoOperatorFacts.TryGetUnaryOperatorKind(token, out _)
                && !BlLingoOperatorFacts.TryGetBinaryOperatorInfo(token, out _, out _, out _))
            {
                return new BlIdentifierExpression(Advance());
            }

            if (token.Kind == BlSyntaxKind.EndOfFileToken)
            {
                return new BlMissingExpression(token);
            }

            return new BlMissingExpression(Advance());
        }

        private BlSyntaxToken Advance()
        {
            var current = Current;
            if (!IsAtEnd)
            {
                _position++;
                _lastTokenIndex = _position - 1;
            }

            return current;
        }

        private BlSyntaxToken Current => _tokens[_position];

        private bool IsAtEnd => Current.Kind == BlSyntaxKind.EndOfFileToken;
    }
}
