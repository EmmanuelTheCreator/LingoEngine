using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace BlingoEngine.IO.Legacy.Tests.Texts;

public class BlXmedTokenReaderColorTests
{
    private static readonly string FixtureRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "TestData", "Legacy", "Texts_Fields"));
    private readonly ITestOutputHelper _output;

    public BlXmedTokenReaderColorTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public static IEnumerable<object[]> ColorSequences()
    {
        yield return new object[]
        {
            "Text_Hallo_col_blue1_13.xmed.bin",
            "#0000FF",
            "01 30 82 82 81 01 46 46 30 30  81 01 30        01 46 46 46 46 81 81 01 30 82 02"
        };
        yield return new object[]
        {
            "Text_Hallo_col_yellow_13.xmed.bin",
            "#FFFF00",
            "01 30 82 82    01 46 46 30 30  81 01 30 81     01 46 46 46 46 81 81 01 30 82 02"
        };
        yield return new object[]
        {
            "Text_Hallo_col_pink_13.xmed.bin",
            "#FF00FF",
            "01 30 82 82    01 46 46 30 30  01 30           01 46 46 30 30 01 30 01 46 46 46 46 81"
        };
        yield return new object[]
        {
            "Text_Hallo_col_lightgreen_13.xmed.bin",
            "#CCFF99",
            "01 30 82 82    01 43 43 30 30  01 46 46 30 30  01 39 39 30 30  01 30 01 46"
        };
        yield return new object[]
        {
            "Text_Hallo_col_orange_13.xmed.bin",
            "#FFCC66",
            "01 30 82 82    01 46 46 30 30  01 43 43 30 30  01 36 36 30 30  01 30 01 46"
        };
        yield return new object[]
        {
            "Text_Hallo_col_bordeau_13.xmed.bin",
            "#880000",
            "01 30 82 82    01 38 38 30 30  01 30 81 81     01 46 46 46 46 81 81 01 30 82"
        };
        yield return new object[]
        {
            "MemberTests/Text_Multi_Style_Size_Color_13.xmed.bin",
            "#F7204A",
            "01 43 01 33 01 30 81 82 82 01 46 37 30 30 01 32 30 30 30 01 34 41 30 30 01 30 01 46 46 46 46 81 81 01 30 82 02 43 30 30 30 30 02 30"
        };
        yield return new object[]
        {
            "MemberTests/Text_Multi_Style_Size_Color_13.xmed.bin",
            "#1EF02E",
            "01 31 01 30 81 01 39 01 32 01 30 81 82 82 01 31 45 30 30 01 46 30 30 30 01 32 45 30 30 01 30 01 46 46 46 46 81 81 01 30 82 02 39 30 30 30 30 02 30"
        };
        yield return new object[]
        {
            "MemberTests/Text_Multi_Style_Size_Color_13.xmed.bin",
            "#2702FD",
            "01 43 01 33 01 30 81 82 82 01 32 37 30 30 01 32 30 30 01 46 44 30 30 01 30 01 46 46 46 46 81 81 01 30 82 02 43 30 30 30 30 02 30"
        };
        yield return new object[]
        {
            "MemberTests/Text_Multi_Style_Size_Color_13.xmed.bin",
            "#2702FD",
            "03 82 01 32 01 30 81 01 31 30 01 32 01 30 81 02 34 30 30 02 30 01 32 37 30 30 01 32 30 30 01 46 44 30 30 01 30 01 46 46 46 46 81 81 01 30 82 02 31 32 30 30 30 30 02 30"
        };
    }

    [Theory]
    [MemberData(nameof(ColorSequences))]
    public void Color_bytes_should_exist_in_fixture(string relativePath, string expectedHex, string patternHex)
    {
        var absolutePath = Path.Combine(FixtureRoot, relativePath);
        File.Exists(absolutePath).Should().BeTrue($"fixture {relativePath} must exist");

        var bytes = File.ReadAllBytes(absolutePath);
        var pattern = ParseHexBytes(patternHex);

        ContainsPattern(bytes, pattern).Should().BeTrue($"pattern for {expectedHex} not found in {relativePath}");
        _output.WriteLine($"Fixture: {relativePath}");
        _output.WriteLine($"Expected color: {expectedHex}");
    }

    private static byte[] ParseHexBytes(string hex)
    {
        var parts = hex.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var buffer = new byte[parts.Length];
        for (int i = 0; i < parts.Length; i++)
            buffer[i] = byte.Parse(parts[i], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        return buffer;
    }

    private static bool ContainsPattern(byte[] haystack, byte[] needle)
    {
        if (needle.Length == 0)
            return true;

        for (int i = 0; i <= haystack.Length - needle.Length; i++)
        {
            bool match = true;
            for (int j = 0; j < needle.Length; j++)
            {
                if (haystack[i + j] != needle[j])
                {
                    match = false;
                    break;
                }
            }

            if (match)
                return true;
        }

        return false;
    }
}
