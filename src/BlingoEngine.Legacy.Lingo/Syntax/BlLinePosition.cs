using System.Globalization;

namespace BlingoEngine.Legacy.Lingo.Syntax;

/// <summary>
/// Represents a specific line and character position within the source text.
/// </summary>
public readonly record struct BlLinePosition(int Line, int Character)
{
    public override string ToString() => FormattableString.Invariant($"{Line}:{Character}");
}

/// <summary>
/// Represents a span of text in terms of line and character positions.
/// </summary>
public readonly record struct BlLinePositionSpan(BlLinePosition Start, BlLinePosition End)
{
    public override string ToString() => FormattableString.Invariant($"{Start}-{End}");
}
