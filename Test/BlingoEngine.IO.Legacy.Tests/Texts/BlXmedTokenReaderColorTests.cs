using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Texts;
using BlingoEngine.IO.Legacy.Texts.Data;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit;

namespace BlingoEngine.IO.Legacy.Tests.Texts;

public class BlXmedTokenReaderColorTests
{
    private static readonly BlXmedTokenizer Tokenizer = new();

    public static IEnumerable<object[]> InlineColorPayloads()
    {
        yield return new object[]
        {
            "C1 04 01 46 46 81 01 30 30 81 01 30 30 82",
            "#FF0000"
        };
        yield return new object[]
        {
            "C1 03 01 46 37 30 30 81 01 32 30 30 30 81 01 34 41 30 30 82",
            "#F7204A"
        };
        yield return new object[]
        {
            "C1 03 01 31 45 30 30 81 01 46 30 30 30 81 01 32 45 30 30 82",
            "#1EF02E"
        };
        yield return new object[]
        {
            "C1 03 01 32 37 30 30 81 01 30 32 30 30 81 01 46 44 30 30 82",
            "#2702FD"
        };
        yield return new object[]
        {
            "82 C1 03 82 C1 04 01 46 46 81 01 30 30 81 01 30 30 82 C1 03 01 46 37 30 30 81 01 32 30 30 30 81 01 34 41 30 30 82",
            "#F7204A"
        };
    }

    [Theory]
    [MemberData(nameof(InlineColorPayloads))]
    public void TryGetColor_should_parse_inline_sequences(string hex, string expectedHex)
    {
        var reader = CreateReader(hex);

        reader.TryGetColor(out var color).Should().BeTrue("inline payload {0}", hex);
        color.Should().NotBeNull();
        color!.Value.ToHex().Should().Be(expectedHex, "inline payload {0}", hex);
    }

    public static IEnumerable<object[]> PaletteCompositePayloads()
    {
        yield return new object[]
        {
            "C1 04 01 46 46 46 46 81 01 43 43 30 30 81 01 36 36 30 30 82",
            "#FFCC66"
        };
        yield return new object[]
        {
            "C1 04 01 46 46 46 46 81 01 30 30 30 30 81 01 46 46 46 46 82",
            "#FF00FF"
        };
        yield return new object[]
        {
            "C1 04 01 30 30 30 30 81 01 46 46 46 46 81 01 30 30 30 30 82",
            "#00FF00"
        };
        yield return new object[]
        {
            "C1 04 01 43 43 30 30 81 01 46 46 46 46 81 01 39 39 30 30 82",
            "#CCFF99"
        };
    }

    [Theory]
    [MemberData(nameof(PaletteCompositePayloads))]
    public void TryGetColor_should_parse_palette_sequences(string hex, string expectedHex)
    {
        var reader = CreateReader(hex);

        reader.TryGetColor(out var color).Should().BeTrue("composite payload {0}", hex);
        color.Should().NotBeNull();
        color!.Value.ToHex().Should().Be(expectedHex, "composite payload {0}", hex);
    }

    [Fact]
    public void TryGetColor_should_fail_when_payload_is_interrupted()
    {
        const string hex = "81 82 01 46 46 30 30 81 01 46 46 46 46";
        var reader = CreateReader(hex);

        reader.TryGetColor(out var color).Should().BeFalse("payload {0}", hex);
        color.Should().BeNull();
    }

    private static BlXmedTokenReader CreateReader(string hex)
    {
        var bytes = ParseHexBytes(hex);
        var tokens = Tokenizer.Tokenize(bytes).Tokens;
        var filtered = tokens.Where(t => t.Type != BlXmedToken.TokenType.Byte).ToList();

        int firstComposite = filtered.FindIndex(t => t.IsCompositeOpen() || t.IsPrefixedHex01());
        if (firstComposite < 0)
            firstComposite = 0;

        return new BlXmedTokenReader(filtered, firstComposite);
    }

    private static byte[] ParseHexBytes(string hex)
    {
        var parts = hex.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var buffer = new byte[parts.Length];
        for (int i = 0; i < parts.Length; i++)
            buffer[i] = byte.Parse(parts[i], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        return buffer;
    }
}
