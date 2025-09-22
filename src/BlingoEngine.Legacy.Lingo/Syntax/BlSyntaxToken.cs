using System;
using System.Collections.Generic;

namespace BlingoEngine.Legacy.Lingo.Syntax;

/// <summary>
/// Represents a single token produced by the tokenizer.
/// </summary>
public sealed record class BlSyntaxToken
{
    /// <summary>
    /// Initializes a new <see cref="BlSyntaxToken"/> instance.
    /// </summary>
    public BlSyntaxToken(
        BlSyntaxKind kind,
        string text,
        string valueText,
        BlTextSpan span,
        BlLinePositionSpan lineSpan,
        IReadOnlyList<BlSyntaxTrivia>? leadingTrivia = null,
        IReadOnlyList<BlSyntaxTrivia>? trailingTrivia = null)
    {
        Kind = kind;
        Text = text ?? string.Empty;
        ValueText = valueText ?? string.Empty;
        Span = span;
        LineSpan = lineSpan;
        LeadingTrivia = leadingTrivia ?? Array.Empty<BlSyntaxTrivia>();
        TrailingTrivia = trailingTrivia ?? Array.Empty<BlSyntaxTrivia>();
    }

    /// <summary>
    /// Gets the syntactic kind of the token.
    /// </summary>
    public BlSyntaxKind Kind { get; }

    /// <summary>
    /// Gets the exact text as it appeared in the source.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the decoded value text of the token.
    /// </summary>
    public string ValueText { get; }

    /// <summary>
    /// Gets the span of the token in character offsets.
    /// </summary>
    public BlTextSpan Span { get; }

    /// <summary>
    /// Gets the span of the token in line and column coordinates.
    /// </summary>
    public BlLinePositionSpan LineSpan { get; }

    /// <summary>
    /// Gets trivia that precedes the token.
    /// </summary>
    public IReadOnlyList<BlSyntaxTrivia> LeadingTrivia { get; }

    /// <summary>
    /// Gets trivia that trails the token on the same line.
    /// </summary>
    public IReadOnlyList<BlSyntaxTrivia> TrailingTrivia { get; }

    public override string ToString() => $"{Kind} \"{Text}\" @ {Span.Start}";
}
