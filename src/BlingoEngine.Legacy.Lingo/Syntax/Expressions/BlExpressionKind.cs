namespace BlingoEngine.Legacy.Lingo.Syntax.Expressions;

/// <summary>
/// Describes the different kinds of expressions recognized by the legacy Lingo parser.
/// </summary>
public enum BlExpressionKind
{
    /// <summary>
    /// Indicates that the parser could not determine the expression kind.
    /// </summary>
    Unknown,

    /// <summary>
    /// Represents an identifier lookup.
    /// </summary>
    Identifier,

    /// <summary>
    /// Represents a numeric literal.
    /// </summary>
    NumberLiteral,

    /// <summary>
    /// Represents a string literal.
    /// </summary>
    StringLiteral,

    /// <summary>
    /// Represents a symbol literal (for example <c>#foo</c>).
    /// </summary>
    SymbolLiteral,

    /// <summary>
    /// Represents a unary operator applied to a single operand.
    /// </summary>
    Unary,

    /// <summary>
    /// Represents a binary operator applied to a left and right operand.
    /// </summary>
    Binary,

    /// <summary>
    /// Represents an expression enclosed in parentheses.
    /// </summary>
    Grouping,

    /// <summary>
    /// Represents a placeholder produced when the parser cannot recover a primary expression.
    /// </summary>
    Missing,
}
