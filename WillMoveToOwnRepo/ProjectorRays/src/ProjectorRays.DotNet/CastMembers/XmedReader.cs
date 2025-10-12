using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ProjectorRays.Common;
using static ProjectorRays.CastMembers.XmedChunkParser;

namespace ProjectorRays.CastMembers;

public enum XmedAlignment
{
    Center,
    Left,
    Right
}

/// <summary>
/// Represents a parsed XMED styled text chunk.
/// </summary>
public sealed class XmedDocument
{
    /// <summary>Complete plain text contained in the chunk.</summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>Runs of text with basic style information.</summary>
    public List<TextStyleRun> Runs { get; } = new();

    /// <summary>Discovered style declarations such as fonts and colors.</summary>
    public List<XmedStyleDeclaration> Styles { get; } = new();

    /// <summary>Style map entries describing line ranges.</summary>
    public List<XmedStyleMapEntry> MapEntries { get; } = new();

    public uint Width { get; set; }
    public uint LineSpacing { get; set; }
    public uint TextLength { get; set; }
}

/// <summary>Simple style declaration extracted from XMED.</summary>
public sealed class XmedStyleDeclaration
{
    public ushort StyleId { get; set; }
    public ushort BaseStyleId { get; set; }
    public ushort F2 { get; set; }
    public ushort F4 { get; set; }
    public ushort TextLength { get; set; }
    public string FontName { get; set; } = string.Empty;
    public byte ColorIndex { get; set; } = 0;
    public ushort FontSize { get; set; }
    public bool Bold { get; set; }
    public bool Italic { get; set; }
    public bool Underline { get; set; }
    public bool Strikeout { get; set; }
    public bool Subscript { get; set; }
    public bool Superscript { get; set; }
    public bool TabbedField { get; set; }
    public bool EditableField { get; set; }
    public XmedAlignment Alignment { get; set; } = XmedAlignment.Center;
    public bool WrapOff { get; set; }
    public bool HasTabs { get; set; }
    public byte AlignmentRaw { get; set; }
    public byte[] UnknownHeader { get; set; } = Array.Empty<byte>();
}

public sealed class XmedStyleMapEntry
{
    public ushort StyleId { get; set; }
    public ushort F2 { get; set; }
    public ushort TextLength { get; set; }
    public ushort F4 { get; set; }
    public ushort BaseStyleId { get; set; }
}

/// <summary>
/// Basic reader for Director XMED chunks.  This is a best-effort
/// implementation based on observed sample files.  The format is not fully
/// documented, but this reader extracts plain text and the most obvious style
/// information so that callers can experiment with the data.
/// </summary>
public class XmedReader : IXmedReader
{
    public XmedDocument Read(BufferView view)
    {
        var data = view.Data;
        int start = view.Offset;
        int end = start + view.Size;

        if (view.Size < 4 || data[start] != (byte)'D' || data[start + 1] != (byte)'E' ||
            data[start + 2] != (byte)'M' || data[start + 3] != (byte)'X')
            throw new InvalidDataException("Invalid XMED chunk header");

        ushort fontSize = 0;
        if (start >= 0x14)
            fontSize = BinaryPrimitives.ReadUInt16LittleEndian(data.AsSpan(start - 0x14));

        var doc = new XmedDocument();
        var textBuilder = new StringBuilder();

        // Basic style information stored near the start of the chunk.  Offsets
        // are derived from docs/DirDissasembly/XMED_Offsets.md.
        doc.Width = BitConverter.ToUInt32(data, start + 0x18);
        byte styleFlags = data[start + 0x1C];
        byte alignByte = data[start + 0x1D];
        doc.LineSpacing = BitConverter.ToUInt32(data, start + 0x3C);
        if (fontSize == 0)
            fontSize = BitConverter.ToUInt16(data, start + 0x40);
        doc.TextLength = BitConverter.ToUInt32(data, start + 0x4C);

        var baseStyle = new XmedStyleDeclaration
        {
            FontSize = fontSize,
            AlignmentRaw = alignByte
        };
        ApplyStyleFlags(styleFlags, baseStyle);
        ApplyAlignmentFlags(alignByte, baseStyle);
        doc.Styles.Add(baseStyle);

        // Parse sequentially after the header.  The first 4 bytes are "DEMX" so
        // begin scanning at offset+4.
        int bodyOffset = start + 4;
        int bodyLength = end - bodyOffset;
        XmedStyleDeclaration? currentStyle = null;
        var activeColor = new RayColor(baseStyle.ColorIndex, baseStyle.ColorIndex, baseStyle.ColorIndex);
        bool colorFromBlock = false;
        ushort blockFontSize = fontSize;
        var blockAlignment = baseStyle.Alignment;
        bool blockWrapOff = baseStyle.WrapOff;
        bool blockHasTabs = baseStyle.HasTabs;

        ProcessSpan(data, start, bodyOffset, bodyLength, doc, textBuilder, ref currentStyle, baseStyle,
            ref activeColor, ref colorFromBlock, ref blockFontSize,
            ref blockAlignment, ref blockWrapOff, ref blockHasTabs);

        if (doc.Runs.Count > 0 && string.IsNullOrEmpty(doc.Runs[0].FontName) && doc.Styles.Count > 1)
        {
            var style = doc.Styles[^1];
            doc.Runs[0].FontName = style.FontName;
            doc.Runs[0].ForeColor = ResolveRunColor(style, baseStyle, activeColor, colorFromBlock);
        }

        doc.Text = textBuilder.ToString();
        return doc;
    }

    private static void ProcessSpan(byte[] buffer, int chunkStart, int offset, int length, XmedDocument doc,
        StringBuilder textBuilder, ref XmedStyleDeclaration? currentStyle, XmedStyleDeclaration baseStyle,
        ref RayColor activeColor, ref bool colorFromBlock, ref ushort blockFontSize,
        ref XmedAlignment blockAlignment, ref bool blockWrapOff, ref bool blockHasTabs)
    {
        int end = offset + length;
        int i = offset;
        while (i < end)
        {
            if (TryParseControlBlock(buffer, ref i, end, chunkStart, doc, textBuilder,
                ref currentStyle, baseStyle, ref activeColor, ref colorFromBlock,
                ref blockFontSize, ref blockAlignment, ref blockWrapOff, ref blockHasTabs))
            {
                continue;
            }

            byte b = buffer[i];

            if (b == (byte)'4' && i + 3 < end && buffer[i + 1] == (byte)'0' && buffer[i + 2] == (byte)',')
            {
                byte color = buffer[i + 3];
                int j = i + 4;
                while (j < end && IsPrintable(buffer[j])) j++;
                string font = Encoding.Latin1.GetString(buffer, i + 4, j - (i + 4));

                if (font.Length == 0)
                {
                    currentStyle = null;
                }
                else
                {
                    currentStyle = new XmedStyleDeclaration
                    {
                        FontName = font,
                        ColorIndex = color,
                        FontSize = blockFontSize != 0 ? blockFontSize : baseStyle.FontSize
                    };
                    doc.Styles.Add(currentStyle);
                }

                i = j;
                if (i < end && buffer[i] == 0) i++;
                continue;
            }

            if (IsDigit(b))
            {
                int j = i;
                while (j < end && IsDigit(buffer[j])) j++;
                int digitLen = j - i;
                if (digitLen == 20 && j < end && buffer[j] == 0x00)
                {
                    if (j + 3 < end && buffer[j + 1] == (byte)'4' && buffer[j + 2] == (byte)'0' && buffer[j + 3] == (byte)',')
                    {
                        if (i >= chunkStart + 7)
                        {
                            byte sFlags = buffer[i - 7];
                            byte aByte = buffer[i - 6];
                            var header = new byte[5];
                            Array.Copy(buffer, i - 5, header, 0, 5);
                            string digits = Encoding.ASCII.GetString(buffer, i, 20);
                            var styleDecl = new XmedStyleDeclaration
                            {
                                StyleId = ushort.Parse(digits.Substring(0, 4)),
                                F2 = ushort.Parse(digits.Substring(4, 4)),
                                TextLength = ushort.Parse(digits.Substring(8, 4)),
                                F4 = ushort.Parse(digits.Substring(12, 4)),
                                BaseStyleId = ushort.Parse(digits.Substring(16, 4)),
                                ColorIndex = buffer[j + 3],
                                FontSize = blockFontSize != 0 ? blockFontSize : baseStyle.FontSize,
                                AlignmentRaw = aByte,
                                UnknownHeader = header
                            };
                            int fontStart = j + 4;
                            int fontEnd = fontStart;
                            while (fontEnd < end && IsPrintable(buffer[fontEnd])) fontEnd++;
                            styleDecl.FontName = Encoding.Latin1.GetString(buffer, fontStart, fontEnd - fontStart);

                            ApplyStyleFlags(sFlags, styleDecl);
                            ApplyAlignmentFlags(aByte, styleDecl);
                            doc.Styles.Add(styleDecl);
                            currentStyle = styleDecl;

                            i = fontEnd;
                            if (i < end && buffer[i] == 0) i++;
                            continue;
                        }
                    }
                    else
                    {
                        string digits = Encoding.ASCII.GetString(buffer, i, 20);
                        var entry = new XmedStyleMapEntry
                        {
                            StyleId = ushort.Parse(digits.Substring(0, 4)),
                            F2 = ushort.Parse(digits.Substring(4, 4)),
                            TextLength = ushort.Parse(digits.Substring(8, 4)),
                            F4 = ushort.Parse(digits.Substring(12, 4)),
                            BaseStyleId = ushort.Parse(digits.Substring(16, 4))
                        };
                        doc.MapEntries.Add(entry);
                        i = j;
                        if (i < end && buffer[i] == 0) i++;
                        continue;
                    }
                }

                if (j < end && buffer[j] == 0x2C)
                {
                    string num = Encoding.ASCII.GetString(buffer, i, j - i);
                    if (int.TryParse(num, out int len))
                    {
                        int textStart = j + 1;
                        if (textStart + len <= end)
                        {
                            bool printable = true;
                            for (int k = 0; k < len; k++)
                            {
                                if (!IsPrintable(buffer[textStart + k]))
                                {
                                    printable = false;
                                    break;
                                }
                            }

                            var run = new TextStyleRun
                            {
                                Length = len,
                                Start = textBuilder.Length,
                                FontName = currentStyle?.FontName ?? string.Empty,
                                FontSize = ResolveFontSize(blockFontSize, currentStyle, baseStyle.FontSize),
                                ForeColor = ResolveRunColor(currentStyle, baseStyle, activeColor, colorFromBlock),
                                Bold = currentStyle?.Bold ?? baseStyle.Bold,
                                Italic = currentStyle?.Italic ?? baseStyle.Italic,
                                Underline = currentStyle?.Underline ?? baseStyle.Underline
                            };

                            if (printable)
                            {
                                string text = Encoding.Latin1.GetString(buffer, textStart, len);
                                run.Text = text;
                                textBuilder.Append(text);
                            }
                            else
                            {
                                var span = buffer.AsSpan(textStart, len);
                                if (len >= 2)
                                    run.Unknown1 = BinaryPrimitives.ReadUInt16BigEndian(span);
                                if (len >= 6)
                                    run.Unknown2 = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(2));
                                if (len >= 10)
                                    run.Unknown3 = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(6));
                                if (len >= 14)
                                    run.Unknown4 = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(10));
                                if (len >= 18)
                                    run.Unknown5 = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(14));
                                if (len >= 22)
                                    run.Unknown6 = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(18));
                                if (len >= 26)
                                    run.Unknown7 = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(22));
                                if (len >= 30)
                                    run.Unknown8 = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(26));
                                if (len >= 34)
                                    run.Unknown9 = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(30));
                                if (len >= 38)
                                    run.Unknown10 = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(34));
                                if (len >= 42)
                                    run.Unknown11 = BinaryPrimitives.ReadUInt32BigEndian(span.Slice(38));
                            }

                            doc.Runs.Add(run);

                            i = textStart + len;
                            if (i < end && (buffer[i] == 0x00 || buffer[i] == 0x03))
                                i++;
                            continue;
                        }
                    }
                }
            }

            i++;
        }
    }

    private static RayColor ResolveRunColor(XmedStyleDeclaration? style, XmedStyleDeclaration baseStyle, RayColor blockColor, bool blockActive)
    {
        if (blockActive) return blockColor;

        if (style != null && style.ColorIndex != 0)
            return new RayColor(style.ColorIndex, style.ColorIndex, style.ColorIndex);

        return new RayColor(baseStyle.ColorIndex, baseStyle.ColorIndex, baseStyle.ColorIndex);
    }

    private static ushort ResolveFontSize(ushort blockFontSize, XmedStyleDeclaration? style, ushort defaultSize)
    {
        if (blockFontSize != 0) return blockFontSize;
        if (style != null && style.FontSize != 0) return style.FontSize;
        return defaultSize;
    }

    private static bool TryParseControlBlock(byte[] data, ref int index, int end, int chunkStart, XmedDocument doc,
        StringBuilder textBuilder, ref XmedStyleDeclaration? currentStyle, XmedStyleDeclaration baseStyle,
        ref RayColor activeColor, ref bool colorFromBlock, ref ushort blockFontSize,
        ref XmedAlignment blockAlignment, ref bool blockWrapOff, ref bool blockHasTabs)
    {
        if (index >= end - 1 || data[index] != 0xC1)
            return false;

        int typeIndex = index + 1;
        if (typeIndex >= end) return false;

        byte blockType = data[typeIndex];
        int blockStart = typeIndex + 1;
        int closeIndex = FindControlBlockEnd(data, blockStart, end);
        if (closeIndex < 0) return false;

        var span = data.AsSpan(blockStart, closeIndex - blockStart);

        switch (blockType)
        {
            case 0x03:
                ParseStyleRunBlock(span, ref activeColor, ref colorFromBlock, ref blockFontSize);
                if (currentStyle != null && blockFontSize != 0)
                {
                    currentStyle.FontSize = blockFontSize;
                }
                break;
            case 0x04:
                ParseAlignmentBlock(span, ref blockAlignment, ref blockWrapOff, ref blockHasTabs);
                baseStyle.Alignment = blockAlignment;
                baseStyle.WrapOff = blockWrapOff;
                baseStyle.HasTabs = blockHasTabs;
                if (currentStyle != null)
                {
                    currentStyle.Alignment = blockAlignment;
                    currentStyle.WrapOff = blockWrapOff;
                    currentStyle.HasTabs = blockHasTabs;
                }
                break;
            case 0x05:
                // spacing / tab block not yet interpreted
                break;
            case 0x0A:
            case 0x0B:
            case 0x1C:
                ApplyToggleBlock(blockType, span, ref currentStyle, baseStyle);
                break;
            case 0x20:
                colorFromBlock = false;
                activeColor = new RayColor(baseStyle.ColorIndex, baseStyle.ColorIndex, baseStyle.ColorIndex);
                blockFontSize = baseStyle.FontSize;
                blockAlignment = baseStyle.Alignment;
                blockWrapOff = baseStyle.WrapOff;
                blockHasTabs = baseStyle.HasTabs;
                break;
            default:
                break;
        }

        if (span.Length > 0)
        {
            ProcessSpan(data, chunkStart, blockStart, span.Length, doc, textBuilder, ref currentStyle, baseStyle,
                ref activeColor, ref colorFromBlock, ref blockFontSize,
                ref blockAlignment, ref blockWrapOff, ref blockHasTabs);
        }

        index = Math.Min(closeIndex + 2, end);
        return true;
    }

    private static int FindControlBlockEnd(byte[] data, int start, int end)
    {
        int depth = 1;
        int i = start;
        while (i < end)
        {
            if (data[i] == 0xC1)
            {
                i += 2;
                depth++;
                continue;
            }
            if (data[i] == 0xC2)
            {
                int closeIndex = i;
                i += 2;
                depth--;
                if (depth == 0)
                    return closeIndex;
                continue;
            }

            i++;
        }

        return -1;
    }

    private static void ParseStyleRunBlock(ReadOnlySpan<byte> span, ref RayColor color, ref bool colorFromBlock, ref ushort fontSize)
    {
        var components = new List<byte>(3);
        bool fontAssigned = false;
        int i = 0;
        while (i < span.Length)
        {
            byte token = span[i];
            switch (token)
            {
                case 0x01:
                    i++;
                    var literal = ReadDelimitedAscii(span, ref i);
                    if (literal.Length > 0 && components.Count < 3 && TryParseColorComponent(literal, out var comp))
                    {
                        components.Add(comp);
                    }
                    break;
                case 0x02:
                    i++;
                    literal = ReadDelimitedAscii(span, ref i);
                    if (!fontAssigned && TryParseHexInt(literal, out var value))
                    {
                        fontSize = (ushort)Math.Clamp(value, 0, ushort.MaxValue);
                        fontAssigned = true;
                    }
                    break;
                default:
                    i++;
                    break;
            }
        }

        if (components.Count == 3)
        {
            color = new RayColor(components[0], components[1], components[2]);
            colorFromBlock = true;
        }
    }

    private static void ParseAlignmentBlock(ReadOnlySpan<byte> span, ref XmedAlignment alignment, ref bool wrapOff, ref bool hasTabs)
    {
        int i = 0;
        bool alignmentSet = false;
        while (i < span.Length)
        {
            byte token = span[i];
            switch (token)
            {
                case 0x01:
                    i++;
                    var literal = ReadDelimitedAscii(span, ref i);
                    if (literal.Length == 1)
                    {
                        if (literal[0] == '0') wrapOff = false;
                        else if (literal[0] == '1') wrapOff = true;
                        else if (literal[0] == '2') hasTabs = true;
                    }
                    break;
                case 0x02:
                    i++;
                    literal = ReadDelimitedAscii(span, ref i);
                    if (!alignmentSet && TryParseHexInt(literal, out var value))
                    {
                        alignment = value switch
                        {
                            1 => XmedAlignment.Right,
                            2 => XmedAlignment.Left,
                            3 => XmedAlignment.Left,
                            _ => XmedAlignment.Center
                        };
                        alignmentSet = true;
                    }
                    break;
                default:
                    i++;
                    break;
            }
        }
    }

    private static void ApplyToggleBlock(byte blockType, ReadOnlySpan<byte> span, ref XmedStyleDeclaration? currentStyle, XmedStyleDeclaration baseStyle)
    {
        bool? state = null;
        int i = 0;
        while (i < span.Length)
        {
            byte token = span[i];
            if (token == 0x01)
            {
                i++;
                var literal = ReadDelimitedAscii(span, ref i);
                if (literal == "33") state = true;
                else if (literal == "30") state = false;
            }
            else
            {
                i++;
            }
        }

        if (state == null) return;

        switch (blockType)
        {
            case 0x0A:
                if (currentStyle != null) currentStyle.Superscript = state.Value;
                baseStyle.Superscript = state.Value;
                break;
            case 0x0B:
                if (currentStyle != null) currentStyle.Subscript = state.Value;
                baseStyle.Subscript = state.Value;
                break;
            case 0x1C:
                if (currentStyle != null) currentStyle.Underline = state.Value;
                baseStyle.Underline = state.Value;
                break;
        }
    }

    private static string ReadDelimitedAscii(ReadOnlySpan<byte> span, ref int index)
    {
        int start = index;
        while (index < span.Length)
        {
            byte b = span[index];
            if (b == 0x81 || b == 0x82 || b == 0xC1 || b == 0xC2)
                break;
            index++;
        }

        var literal = Encoding.ASCII.GetString(span.Slice(start, index - start));
        while (index < span.Length && (span[index] == 0x81 || span[index] == 0x82))
            index++;

        return literal;
    }

    private static bool TryParseColorComponent(string literal, out byte component)
    {
        literal = literal.Trim();
        component = 0;
        if (literal.Length == 0) return false;

        var hex = literal.Length >= 2 ? literal[..2] : literal;
        return byte.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out component);
    }

    private static bool TryParseHexInt(string literal, out int value)
    {
        literal = literal.Trim();
        value = 0;
        if (literal.Length == 0) return false;

        bool negative = literal[0] == '-';
        if (negative) literal = literal[1..];
        if (literal.Length == 0) return false;

        if (ulong.TryParse(literal, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var parsed))
        {
            long signed = negative ? -(long)parsed : (long)parsed;
            if (signed > int.MaxValue) signed = int.MaxValue;
            if (signed < int.MinValue) signed = int.MinValue;
            value = (int)signed;
            return true;
        }

        return false;
    }

    private static void ApplyStyleFlags(byte flags, XmedStyleDeclaration style)
    {
        style.Bold = (flags & 0x01) != 0;
        style.Italic = (flags & 0x02) != 0;
        style.Underline = (flags & 0x04) != 0;
        style.Strikeout = (flags & 0x08) != 0;
        style.Subscript = (flags & 0x10) != 0;
        style.Superscript = (flags & 0x20) != 0;
        style.TabbedField = (flags & 0x40) != 0;
        style.EditableField = (flags & 0x80) != 0;
    }

    private static void ApplyAlignmentFlags(byte b, XmedStyleDeclaration style)
    {
        style.WrapOff = b == 0x19;
        style.HasTabs = (b & 0x10) != 0;
        style.Alignment = b switch
        {
            0x1A => XmedAlignment.Left,
            0x15 => XmedAlignment.Right,
            _ => XmedAlignment.Center
        };
    }

    private static bool IsDigit(byte b) => b >= (byte)'0' && b <= (byte)'9';

    private static bool IsPrintable(byte b) 
    {
        // Allow standard ASCII printable characters (32-126)
        if (b >= 32 && b <= 126) return true;
        
        // Allow common whitespace characters that should be preserved in text
        if (b == 9 || b == 10 || b == 13) return true; // Tab, LF, CR
        
        // Allow extended ASCII characters (128-255) for international text
        if (b >= 128) return true;
        
        // Reject control characters (0-8, 11-12, 14-31)
        return false;
    }
}

