using System.Collections.Generic;

namespace BlingoEngine.Legacy.Lingo.Syntax;

/// <summary>
/// Provides information about reserved keywords in the Lingo language.
/// </summary>
internal static class BlLingoKeywordFacts
{
    private static readonly HashSet<string> s_keywords = new(
        new[]
        {
            "after",
            "before",
            "case",
            "cast",
            "channel",
            "continue",
            "down",
            "else",
            "end",
            "exit",
            "for",
            "from",
            "global",
            "go",
            "if",
            "in",
            "into",
            "local",
            "me",
            "movie",
            "next",
            "of",
            "on",
            "property",
            "put",
            "repeat",
            "return",
            "script",
            "set",
            "tell",
            "then",
            "the",
            "to",
            "when",
            "while",
            "with",
        },
        StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether the supplied text corresponds to a reserved keyword.
    /// </summary>
    public static bool IsKeyword(string text) => s_keywords.Contains(text);
}
