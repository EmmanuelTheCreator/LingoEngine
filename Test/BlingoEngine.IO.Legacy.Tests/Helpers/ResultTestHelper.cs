using System;
using FluentAssertions;

#nullable enable

namespace BlingoEngine.IO.Legacy.Tests.Helpers;

internal static class ResultTestHelper
{
    public static void ShouldMatchNormalized(this string? actual, string expected)
    {
        NormalizeLineEndings(actual).Should().Be(NormalizeLineEndings(expected));
    }

    public static string NormalizeLineEndings(string? value)
    {
        return (value ?? string.Empty)
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal)
            .Trim();
    }
}
