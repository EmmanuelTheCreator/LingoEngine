namespace BlingoEngine.Legacy.Lingo.Syntax.Expressions;

/// <summary>
/// Lists the binary operators supported by the expression parser.
/// </summary>
public enum BlBinaryOperatorKind
{
    /// <summary>
    /// Indicates that the token was not recognized as a binary operator.
    /// </summary>
    Unknown,

    /// <summary>
    /// Represents the exponentiation operator (<c>^</c>).
    /// </summary>
    Power,

    /// <summary>
    /// Represents the multiplication operator (<c>*</c>).
    /// </summary>
    Multiply,

    /// <summary>
    /// Represents the division operator (<c>/</c>).
    /// </summary>
    Divide,

    /// <summary>
    /// Represents the modulus operator (<c>mod</c>).
    /// </summary>
    Modulus,

    /// <summary>
    /// Represents the addition operator (<c>+</c>).
    /// </summary>
    Add,

    /// <summary>
    /// Represents the subtraction operator (<c>-</c>).
    /// </summary>
    Subtract,

    /// <summary>
    /// Represents the string concatenation operator (<c>&amp;</c>).
    /// </summary>
    Concatenate,

    /// <summary>
    /// Represents the equality comparison operator (<c>=</c>).
    /// </summary>
    Equal,

    /// <summary>
    /// Represents the inequality comparison operator (<c>&lt;&gt;</c>).
    /// </summary>
    NotEqual,

    /// <summary>
    /// Represents the less-than comparison operator (<c>&lt;</c>).
    /// </summary>
    LessThan,

    /// <summary>
    /// Represents the less-than-or-equal comparison operator (<c>&lt;=</c>).
    /// </summary>
    LessThanOrEqual,

    /// <summary>
    /// Represents the greater-than comparison operator (<c>&gt;</c>).
    /// </summary>
    GreaterThan,

    /// <summary>
    /// Represents the greater-than-or-equal comparison operator (<c>&gt;=</c>).
    /// </summary>
    GreaterThanOrEqual,

    /// <summary>
    /// Represents the logical conjunction operator (<c>and</c> or <c>&amp;&amp;</c>).
    /// </summary>
    LogicalAnd,

    /// <summary>
    /// Represents the logical disjunction operator (<c>or</c>).
    /// </summary>
    LogicalOr,
}
