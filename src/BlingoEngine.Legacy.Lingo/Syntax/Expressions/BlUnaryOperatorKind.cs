namespace BlingoEngine.Legacy.Lingo.Syntax.Expressions;

/// <summary>
/// Lists the unary operators supported by the expression parser.
/// </summary>
public enum BlUnaryOperatorKind
{
    /// <summary>
    /// Indicates that the token was not recognized as a unary operator.
    /// </summary>
    Unknown,

    /// <summary>
    /// Represents the logical negation operator (<c>not</c>).
    /// </summary>
    LogicalNot,

    /// <summary>
    /// Represents the unary plus operator (<c>+value</c>).
    /// </summary>
    Positive,

    /// <summary>
    /// Represents the unary minus operator (<c>-value</c>).
    /// </summary>
    Negative,
}
