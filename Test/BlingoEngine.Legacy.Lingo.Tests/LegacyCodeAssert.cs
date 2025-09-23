using System;
using FluentAssertions;

namespace BlingoEngine.Legacy.Lingo.Tests;

public static class LegacyCodeAssert
{
    public static void AreEqual(string expected, string actual)
    {
        Normalize(actual).Should().Be(Normalize(expected));
    }

    private static string Normalize(string value)
    {
        return value
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n')
            .Trim();
    }
}
