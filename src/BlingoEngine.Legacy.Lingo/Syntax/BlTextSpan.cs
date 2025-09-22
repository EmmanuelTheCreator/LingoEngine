namespace BlingoEngine.Legacy.Lingo.Syntax;

/// <summary>
/// Describes a contiguous span of text by offset and length.
/// </summary>
public readonly record struct BlTextSpan(int Start, int Length)
{
    /// <summary>
    /// Gets the exclusive end position of the span.
    /// </summary>
    public int End => Start + Length;

    /// <summary>
    /// Creates a span from the supplied inclusive start and exclusive end positions.
    /// </summary>
    public static BlTextSpan FromBounds(int start, int end) => new(start, end - start);
}
