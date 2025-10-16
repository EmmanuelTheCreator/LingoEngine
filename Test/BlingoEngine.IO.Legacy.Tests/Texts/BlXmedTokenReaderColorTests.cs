using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Texts;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit;

namespace BlingoEngine.IO.Legacy.Tests.Texts;

public class BlXmedTokenReaderColorTests
{
    private static readonly BlXmedTokenizer Tokenizer = new();

    public static IEnumerable<object[]> InlineColorPayloads()
    {
        yield return new object[]
        {
            "C1 04 02 FF 02 00 02 00 82",
            "#FF0000"
        };
        yield return new object[]
        {
            "C1 03 02 F7 00 02 20 00 02 4A 00 82",
            "#F7204A"
        };
        yield return new object[]
        {
            "C1 03 02 1E 00 02 F0 00 02 2E 00 82",
            "#1EF02E"
        };
        yield return new object[]
        {
            "C1 03 02 27 00 02 02 00 02 FD 00 82",
            "#2702FD"
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

    [Fact]
    public void TryGetColor_should_parse_sequential_inline_composites()
    {
        const string hex = "C1 04 02 FF 02 00 02 00 82 C1 03 02 F7 00 02 20 00 02 4A 00 82";
        var reader = CreateReader(hex);

        reader.TryGetColor(out var first).Should().BeTrue();
        first.Should().NotBeNull();
        first!.Value.ToHex().Should().Be("#FF0000");

        reader.TryGetColor(out var second).Should().BeTrue();
        second.Should().NotBeNull();
        second!.Value.ToHex().Should().Be("#F7204A");
    }

    public static IEnumerable<object[]> PaletteCompositePayloads()
    {
        yield return new object[]
        {
            "C1 03 01 FF 00 01 CC 00 01 66 00 82",
            "#FFCC66"
        };
        yield return new object[]
        {
            "C1 03 01 FF 00 01 00 00 01 FF 00 82",
            "#FF00FF"
        };
        yield return new object[]
        {
            "C1 03 01 00 00 01 FF 00 01 00 00 82",
            "#00FF00"
        };
        yield return new object[]
        {
            "C1 03 01 CC 00 01 FF 00 01 99 00 82",
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
        const string hex = "C1 03 81 82 01 FF 00 01 FF 00";
        var reader = CreateReader(hex);

        reader.TryGetColor(out var color).Should().BeFalse("payload {0}", hex);
        color.Should().BeNull();
    }

    private static BlXmedTokenReader CreateReader(string hex)
    {
        var bytes = ParseHexBytes(hex);
        var tokens = Tokenizer.Tokenize(bytes).Tokens;

        int firstComposite = tokens.FindIndex(t => t.IsCompositeOpen());
        if (firstComposite < 0)
            firstComposite = 0;

        return new BlXmedTokenReader(tokens, firstComposite);
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
