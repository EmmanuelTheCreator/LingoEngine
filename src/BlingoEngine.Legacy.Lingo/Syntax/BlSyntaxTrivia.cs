namespace BlingoEngine.Legacy.Lingo.Syntax;

/// <summary>
/// Represents trivia such as whitespace or comments associated with tokens.
/// </summary>
public sealed record class BlSyntaxTrivia
{
    /// <summary>
    /// Initializes a new <see cref="BlSyntaxTrivia"/> instance.
    /// </summary>
    public BlSyntaxTrivia(
        BlSyntaxKind kind,
        string text,
        string valueText,
        BlTextSpan span,
        BlLinePositionSpan lineSpan)
    {
        Kind = kind;
        Text = text ?? string.Empty;
        ValueText = valueText ?? string.Empty;
        Span = span;
        LineSpan = lineSpan;
    }

    /// <summary>
    /// Gets the trivia kind.
    /// </summary>
    public BlSyntaxKind Kind { get; }

    /// <summary>
    /// Gets the literal text of the trivia.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets a decoded value text for the trivia when applicable.
    /// </summary>
    public string ValueText { get; }

    /// <summary>
    /// Gets the span of the trivia in character offsets.
    /// </summary>
    public BlTextSpan Span { get; }

    /// <summary>
    /// Gets the span of the trivia in line and column coordinates.
    /// </summary>
    public BlLinePositionSpan LineSpan { get; }

    public override string ToString() => $"{Kind} \"{Text}\" @ {Span.Start}";
}
