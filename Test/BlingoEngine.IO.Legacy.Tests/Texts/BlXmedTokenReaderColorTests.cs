using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using Xunit;

namespace BlingoEngine.IO.Legacy.Tests.Texts;

public class BlXmedTokenReaderColorTests
{
    public static IEnumerable<object[]> ColorSequences()
    {
        yield return new object[] { "inline-blue", "#0000FF", "01 30 82 82 81 01 46 46 30 30 81 01 30 01 46 46 46 46 81 81 01 30 82 02" };
        yield return new object[] { "inline-yellow", "#FFFF00", "01 30 82 82    01 46 46 30 30  81 01 30 81     01 46 46 46 46 81 81 01 30 82 02" };
        yield return new object[] { "inline-pink", "#FF00FF", "01 30 82 82    01 46 46 30 30  01 30           01 46 46 30 30 01 30 01 46 46 46 46 81" };
        yield return new object[] { "inline-lightgreen", "#CCFF99", "01 30 82 82    01 43 43 30 30  01 46 46 30 30  01 39 39 30 30  01 30 01 46" };
        yield return new object[] { "inline-orange", "#FFCC66", "01 30 82 82    01 46 46 30 30  01 43 43 30 30  01 36 36 30 30  01 30 01 46" };
        yield return new object[] { "inline-bordeau", "#880000", "01 30 82 82    01 38 38 30 30  01 30 81 81     01 46 46 46 46 81 81 01 30 82" };
        yield return new object[] { "composite-red", "#F7204A", "01 43 01 33 01 30 81 82 82 01 46 37 30 30 01 32 30 30 30 01 34 41 30 30 01 30 01 46 46 46 46 81 81 01 30 82 02 43 30 30 30 30 02 30" };
        yield return new object[] { "composite-green", "#1EF02E", "82 01 31 01 30 81 01 39 01 32 01 30 81 82 82 01 31 45 30 30 01 46 30 30 30 01 32 45 30 30 01 30 01 46 46 46 46 81 81 01 30 82 02 39 30 30 30 30 02 30" };
        yield return new object[] { "composite-blue-a", "#2702FD", "01 43 01 33 01 30 81 82 82 01 32 37 30 30 01 32 30 30 01 46 44 30 30 01 30 01 46 46 46 46 81 81 01 30 82 02 43 30 30 30 30 02 30" };
        yield return new object[] { "composite-blue-b", "#2702FD", "03 82 01 32 01 30 81 01 31 30 01 32 01 30 81 02 34 30 30 02 30 01 32 37 30 30 01 32 30 30 01 46 44 30 30 01 30 01 46 46 46 46 81 81 01 30 82 02 31 32 30 30 30 30 02 30" };
    }

  

    [Theory]
    [MemberData(nameof(ColorSequences))]
    public void Raw_color_sequences_should_yield_expected_hex(string key, string expectedHex, string patternHex)
    {
        var bytes = ParseHexBytes(patternHex);
        // todo : use BlXmedTokenReader to parse the bytes and compare the expected color.
    }



    static byte[] ParseHexBytes(string hex)
    {
        var parts = hex.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var buffer = new byte[parts.Length];
        for (int i = 0; i < parts.Length; i++)
            buffer[i] = byte.Parse(parts[i], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        return buffer;
    }
}
