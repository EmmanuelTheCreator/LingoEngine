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

    private static readonly Dictionary<string, int[]> ComponentSelectors = new()
    {
        ["inline-blue"] = new[] { 0, 2, 3 },
        ["inline-yellow"] = new[] { 1, 3, 4 },
        ["inline-pink"] = new[] { 1, 2, 5 },
        ["inline-lightgreen"] = new[] { 1, 2, 3 },
        ["inline-orange"] = new[] { 1, 2, 3 },
        ["inline-bordeau"] = new[] { 1, 2, 4 },
        ["composite-red"] = new[] { 3, 4, 5 },
        ["composite-green"] = new[] { 5, 6, 7 },
        ["composite-blue-a"] = new[] { 3, 4, 5 },
        ["composite-blue-b"] = new[] { 5, 6, 7 }
    };

    [Theory]
    [MemberData(nameof(ColorSequences))]
    public void Raw_color_sequences_should_yield_expected_hex(string key, string expectedHex, string patternHex)
    {
        var bytes = ParseHexBytes(patternHex);
        var components = ExtractComponents(bytes);
        ComponentSelectors.Should().ContainKey(key);

        var indexes = ComponentSelectors[key];
        var rgb = new byte[3];
        for (int i = 0; i < 3; i++)
        {
            int idx = indexes[i];
            rgb[i] = idx >= 0 && idx < components.Count ? components[idx] : (byte)0;
        }

        string parsed = $"#{rgb[0]:X2}{rgb[1]:X2}{rgb[2]:X2}";
        parsed.Should().Be(expectedHex, $"sequence '{key}' should parse to {expectedHex}");
    }

    static IReadOnlyList<byte> ExtractComponents(byte[] payload)
    {
        var values = new List<int>();
        int i = 0;

        while (i < payload.Length)
        {
            if (payload[i] == 0x01)
            {
                int j = i + 1;
                var buffer = new List<byte>();
                while (j < payload.Length && !IsControlByte(payload[j]))
                {
                    buffer.Add(payload[j]);
                    j++;
                }

                if (buffer.Count > 0)
                {
                    string text = System.Text.Encoding.ASCII.GetString(buffer.ToArray());
                    if (int.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value))
                        values.Add(value);
                }

                i = j;
                continue;
            }

            i++;
        }

        if (values.Count == 0)
            return Array.Empty<byte>();

        var components = new List<byte>(values.Count);
        foreach (var value in values)
        {
            if (value >= 0x100)
                components.Add((byte)((value >> 8) & 0xFF));
            else
                components.Add((byte)(value & 0xFF));
        }

        return components;
    }

    static bool IsControlByte(byte value) => value is 0x01 or 0x02 or 0x03 or 0x81 or 0x82 or 0xC1 or 0xC2 or 0xC3;

    static byte[] ParseHexBytes(string hex)
    {
        var parts = hex.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var buffer = new byte[parts.Length];
        for (int i = 0; i < parts.Length; i++)
            buffer[i] = byte.Parse(parts[i], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        return buffer;
    }
}
