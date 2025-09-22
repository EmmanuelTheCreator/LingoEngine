using System;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.Syntax.Expressions;

/// <summary>
/// Represents an abstract expression in the legacy Lingo syntax tree.
/// </summary>
public abstract class BlExpression
{
    private protected BlExpression(BlExpressionKind kind, BlTextSpan span)
    {
        Kind = kind;
        Span = span;
    }

    /// <summary>
    /// Gets the shape of the expression.
    /// </summary>
    public BlExpressionKind Kind { get; }

    /// <summary>
    /// Gets the span of text covered by the expression.
    /// </summary>
    public BlTextSpan Span { get; }

    /// <summary>
    /// Computes a span that starts with the <paramref name="start"/> span and ends with the
    /// <paramref name="end"/> span.
    /// </summary>
    private protected static BlTextSpan CombineSpans(BlTextSpan start, BlTextSpan end)
    {
        var startPosition = start.Start;
        var endPosition = Math.Max(startPosition, end.End);
        return BlTextSpan.FromBounds(startPosition, endPosition);
    }
}

/// <summary>
/// Represents an identifier lookup expression.
/// </summary>
public sealed class BlIdentifierExpression : BlExpression
{
    /// <summary>
    /// Initializes a new <see cref="BlIdentifierExpression"/> instance.
    /// </summary>
    public BlIdentifierExpression(BlSyntaxToken identifierToken)
        : base(BlExpressionKind.Identifier, identifierToken?.Span ?? default)
    {
        IdentifierToken = identifierToken ?? throw new ArgumentNullException(nameof(identifierToken));
    }

    /// <summary>
    /// Gets the token that provides the identifier text.
    /// </summary>
    public BlSyntaxToken IdentifierToken { get; }
}

/// <summary>
/// Represents a literal value.
/// </summary>
public sealed class BlLiteralExpression : BlExpression
{
    /// <summary>
    /// Initializes a new <see cref="BlLiteralExpression"/> instance.
    /// </summary>
    public BlLiteralExpression(BlExpressionKind literalKind, BlSyntaxToken literalToken)
        : base(literalKind, literalToken?.Span ?? default)
    {
        if (literalKind is not (BlExpressionKind.NumberLiteral or BlExpressionKind.StringLiteral or BlExpressionKind.SymbolLiteral))
        {
            throw new ArgumentOutOfRangeException(nameof(literalKind), literalKind, "The literal kind must represent a literal expression.");
        }

        LiteralToken = literalToken ?? throw new ArgumentNullException(nameof(literalToken));
    }

    /// <summary>
    /// Gets the token that carries the literal text.
    /// </summary>
    public BlSyntaxToken LiteralToken { get; }
}

/// <summary>
/// Represents a unary operator applied to a single operand.
/// </summary>
public sealed class BlUnaryExpression : BlExpression
{
    /// <summary>
    /// Initializes a new <see cref="BlUnaryExpression"/> instance.
    /// </summary>
    public BlUnaryExpression(BlUnaryOperatorKind operatorKind, BlSyntaxToken operatorToken, BlExpression operand)
        : base(BlExpressionKind.Unary, CombineSpans(operatorToken?.Span ?? default, operand?.Span ?? default))
    {
        if (operatorKind == BlUnaryOperatorKind.Unknown)
        {
            throw new ArgumentOutOfRangeException(nameof(operatorKind), operatorKind, "Operator kind must be specified.");
        }

        OperatorKind = operatorKind;
        OperatorToken = operatorToken ?? throw new ArgumentNullException(nameof(operatorToken));
        Operand = operand ?? throw new ArgumentNullException(nameof(operand));
    }

    /// <summary>
    /// Gets the unary operator.
    /// </summary>
    public BlUnaryOperatorKind OperatorKind { get; }

    /// <summary>
    /// Gets the token that spelled the operator.
    /// </summary>
    public BlSyntaxToken OperatorToken { get; }

    /// <summary>
    /// Gets the operand that the operator applies to.
    /// </summary>
    public BlExpression Operand { get; }
}

/// <summary>
/// Represents a binary operator applied to two operands.
/// </summary>
public sealed class BlBinaryExpression : BlExpression
{
    /// <summary>
    /// Initializes a new <see cref="BlBinaryExpression"/> instance.
    /// </summary>
    public BlBinaryExpression(BlBinaryOperatorKind operatorKind, BlSyntaxToken operatorToken, BlExpression left, BlExpression right)
        : base(BlExpressionKind.Binary, CombineSpans(left?.Span ?? default, right?.Span ?? default))
    {
        if (operatorKind == BlBinaryOperatorKind.Unknown)
        {
            throw new ArgumentOutOfRangeException(nameof(operatorKind), operatorKind, "Operator kind must be specified.");
        }

        OperatorKind = operatorKind;
        OperatorToken = operatorToken ?? throw new ArgumentNullException(nameof(operatorToken));
        Left = left ?? throw new ArgumentNullException(nameof(left));
        Right = right ?? throw new ArgumentNullException(nameof(right));
    }

    /// <summary>
    /// Gets the binary operator.
    /// </summary>
    public BlBinaryOperatorKind OperatorKind { get; }

    /// <summary>
    /// Gets the token that spelled the operator.
    /// </summary>
    public BlSyntaxToken OperatorToken { get; }

    /// <summary>
    /// Gets the expression on the left-hand side of the operator.
    /// </summary>
    public BlExpression Left { get; }

    /// <summary>
    /// Gets the expression on the right-hand side of the operator.
    /// </summary>
    public BlExpression Right { get; }
}

/// <summary>
/// Represents an expression enclosed in parentheses.
/// </summary>
public sealed class BlGroupingExpression : BlExpression
{
    /// <summary>
    /// Initializes a new <see cref="BlGroupingExpression"/> instance.
    /// </summary>
    public BlGroupingExpression(BlSyntaxToken openParenthesisToken, BlExpression innerExpression, BlSyntaxToken? closeParenthesisToken)
        : base(
            BlExpressionKind.Grouping,
            closeParenthesisToken is null
                ? CombineSpans(openParenthesisToken?.Span ?? default, innerExpression?.Span ?? default)
                : CombineSpans(openParenthesisToken?.Span ?? default, closeParenthesisToken.Span))
    {
        OpenParenthesisToken = openParenthesisToken ?? throw new ArgumentNullException(nameof(openParenthesisToken));
        InnerExpression = innerExpression ?? throw new ArgumentNullException(nameof(innerExpression));
        CloseParenthesisToken = closeParenthesisToken;
    }

    /// <summary>
    /// Gets the opening parenthesis token.
    /// </summary>
    public BlSyntaxToken OpenParenthesisToken { get; }

    /// <summary>
    /// Gets the closing parenthesis token if it was present in the source.
    /// </summary>
    public BlSyntaxToken? CloseParenthesisToken { get; }

    /// <summary>
    /// Gets the enclosed expression.
    /// </summary>
    public BlExpression InnerExpression { get; }
}

/// <summary>
/// Represents a placeholder when the parser cannot find a valid expression.
/// </summary>
public sealed class BlMissingExpression : BlExpression
{
    /// <summary>
    /// Initializes a new <see cref="BlMissingExpression"/> instance.
    /// </summary>
    public BlMissingExpression(BlSyntaxToken? token)
        : base(BlExpressionKind.Missing, token?.Span ?? default)
    {
        Token = token;
    }

    /// <summary>
    /// Gets the token near which the parser reported the missing expression.
    /// </summary>
    public BlSyntaxToken? Token { get; }
}
