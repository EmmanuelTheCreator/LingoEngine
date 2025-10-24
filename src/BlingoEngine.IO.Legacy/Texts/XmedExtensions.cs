using System.Buffers.Binary;
using System.Text;

namespace BlingoEngine.IO.Legacy.Texts
{
    public static class XmedExtensions
    {
        //public static uint ReadUInt32Safe(this byte[] buffer, int offset)
        //{
        //    if (buffer == null)
        //    {
        //        return 0;
        //    }

        //    if ((uint)(offset + 4) > (uint)buffer.Length)
        //    {
        //        return 0;
        //    }

        //    return BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(offset, 4));
        //}
        /// <summary>
        /// Converts the supplied <c>STXT</c> payload into a UTF-8 string while tolerating truncated or
        /// padded buffers that appear in early projector versions.
        /// </summary>
        /// <param name="data">Raw bytes copied from the <c>STXT</c> resource.</param>
        /// <returns>The decoded string with trailing null characters trimmed.</returns>
        public static string DecodeSTXT(this ReadOnlySpan<byte> data)
        {
            if (data.Length >= 2)
            {
                var declaredLength = BinaryPrimitives.ReadUInt16BigEndian(data);
                if (declaredLength > 0 && declaredLength <= data.Length - 2)
                {
                    return Encoding.UTF8.GetString(data.Slice(2, declaredLength)).TrimEnd('\0');
                }
            }

            return Encoding.UTF8.GetString(data).TrimEnd('\0');
        }
        //public static string ReadDelimitedAscii(this ReadOnlySpan<byte> span, ref int index)
        //{
        //    int start = index;
        //    while (index < span.Length)
        //    {
        //        byte b = span[index];
        //        if (b == 0x81 || b == 0x82 || b == 0xC1 || b == 0xC2)
        //        {
        //            break;
        //        }

        //        index++;
        //    }

        //    string literal = Encoding.ASCII.GetString(span.Slice(start, index - start));
        //    while (index < span.Length && (span[index] == 0x81 || span[index] == 0x82))
        //    {
        //        index++;
        //    }

        //    return literal;
        //}

        //public static bool TryParseColorComponent(this string literal, out byte component)
        //{
        //    literal = literal.Trim();
        //    component = 0;
        //    if (literal.Length == 0)
        //    {
        //        return false;
        //    }

        //    string hex = literal.Length >= 2 ? literal[..2] : literal;
        //    return byte.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out component);
        //}

        //public static bool TryParseHexInt(this string literal, out int value)
        //{
        //    literal = literal.Trim();
        //    value = 0;
        //    if (literal.Length == 0)
        //    {
        //        return false;
        //    }

        //    bool negative = literal[0] == '-';
        //    if (negative)
        //    {
        //        literal = literal[1..];
        //    }

        //    if (literal.Length == 0)
        //    {
        //        return false;
        //    }

        //    if (ulong.TryParse(literal, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var parsed))
        //    {
        //        long signed = negative ? -(long)parsed : (long)parsed;
        //        if (signed > int.MaxValue)
        //        {
        //            signed = int.MaxValue;
        //        }

        //        if (signed < int.MinValue)
        //        {
        //            signed = int.MinValue;
        //        }

        //        value = (int)signed;
        //        return true;
        //    }

        //    return false;
        //}

        //public static ushort ParseHexUInt16(this ReadOnlySpan<char> span)
        //{
        //    if (ushort.TryParse(span, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value))
        //    {
        //        return value;
        //    }

        //    return 0;
        //}

        //public static bool TryParseRunMapEntry(this string digits, int position, out XmedRunMapEntry entry)
        //{
        //    entry = default!;
        //    if (digits.Length != 20)
        //    {
        //        return false;
        //    }

        //    ReadOnlySpan<char> span = digits.AsSpan();
        //    if (!ushort.TryParse(span[..4], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var type))
        //    {
        //        return false;
        //    }

        //    if (!ushort.TryParse(span.Slice(4, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var f2))
        //    {
        //        return false;
        //    }

        //    if (!ushort.TryParse(span.Slice(8, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var length))
        //    {
        //        return false;
        //    }

        //    if (!ushort.TryParse(span.Slice(12, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var f4))
        //    {
        //        return false;
        //    }

        //    if (!ushort.TryParse(span.Slice(16, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var styleId))
        //    {
        //        return false;
        //    }

        //    entry = new XmedRunMapEntry(type, f2, length, f4, styleId, position);
        //    return true;
        //}

        //public static bool IsDigit(this byte value)
        //{
        //    return value >= (byte)'0' && value <= (byte)'9';
        //}

        //public static BlLegacyColor ResolveRunColor(this XmedStyleDescriptor? style, XmedStyleDescriptor baseStyle,
        //    BlLegacyColor blockColor, bool blockActive)
        //{
        //    if (blockActive)
        //    {
        //        return blockColor;
        //    }

        //    if (style != null && style.ColorIndex != 0)
        //    {
        //        return new BlLegacyColor(style.ColorIndex, style.ColorIndex, style.ColorIndex);
        //    }

        //    if (baseStyle.ColorIndex != 0)
        //    {
        //        return new BlLegacyColor(baseStyle.ColorIndex, baseStyle.ColorIndex, baseStyle.ColorIndex);
        //    }

        //    return blockColor;
        //}

        //public static ushort ResolveFontSize(this ushort blockFontSize, XmedStyleDescriptor? style, ushort defaultSize)
        //{
        //    if (blockFontSize != 0)
        //    {
        //        return blockFontSize;
        //    }

        //    if (style != null && style.FontSize != 0)
        //    {
        //        return style.FontSize;
        //    }

        //    return defaultSize;
        //}



        //public static void MergeAdjacentEqualStyleRuns(this List<XmedTextRun> runs)
        //{
        //    if (runs.Count < 2)
        //    {
        //        return;
        //    }

        //    runs.Sort((a, b) => a.Start.CompareTo(b.Start));
        //    var merged = new List<XmedTextRun>();
        //    var current = runs[0];

        //    for (int i = 1; i < runs.Count; i++)
        //    {
        //        var next = runs[i];
        //        bool adjacent = current.Start + current.Length == next.Start;
        //        bool same =
        //            current.FontName == next.FontName &&
        //            current.FontSize == next.FontSize &&
        //            current.Bold == next.Bold &&
        //            current.Italic == next.Italic &&
        //            current.Underline == next.Underline &&
        //            current.ForeColor.Equals(next.ForeColor);

        //        if (adjacent && same)
        //        {
        //            current.Length += next.Length;
        //            current.Text += next.Text;
        //        }
        //        else
        //        {
        //            merged.Add(current);
        //            current = next;
        //        }
        //    }

        //    merged.Add(current);
        //    runs.Clear();
        //    runs.AddRange(merged);
        //}

        //public static bool TryParseFontDescriptor(this string literal, XmedDocument doc,
        //    ref XmedStyleDescriptor? currentStyle, XmedStyleDescriptor baseStyle,
        //    ref BlLegacyColor activeColor, ref bool colorFromBlock)
        //{
        //    if (literal.StartsWith("40,", StringComparison.Ordinal))
        //    {
        //        int commaIndex = literal.IndexOf(',', 3);
        //        if (commaIndex > 3)
        //        {
        //            string fontName = literal[(commaIndex + 1)..];
        //            byte colorIndex = 0;
        //            if (byte.TryParse(literal.AsSpan(3, commaIndex - 3), NumberStyles.HexNumber, CultureInfo.InvariantCulture,
        //                    out var parsed))
        //            {
        //                colorIndex = parsed;
        //            }

        //            var styleDesc = new XmedStyleDescriptor
        //            {
        //                FontName = fontName,
        //                ColorIndex = colorIndex,
        //                FontSize = baseStyle.FontSize
        //            };

        //            doc.Styles.Add(styleDesc);
        //            currentStyle = styleDesc;
        //            if (colorIndex != 0)
        //            {
        //                activeColor = new BlLegacyColor(colorIndex, colorIndex, colorIndex);
        //                colorFromBlock = true;
        //            }

        //            return true;
        //        }
        //    }

        //    return false;
        //}
    }
}
