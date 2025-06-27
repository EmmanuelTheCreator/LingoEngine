using System;
using System.Collections.Generic;
using System.Text;

namespace LingoEngine.Director.LGodot.Importer
{
    internal abstract class XmedBlock
    {
        public int Start { get; }
        public int Length { get; }

        public virtual bool IsStyle => false;

        protected XmedBlock(int start, int length)
        {
            Start = start;
            Length = length;
        }

        public abstract string Description { get; }

        public virtual string Detail => Description;
    }

    internal sealed class SimpleBlock : XmedBlock
    {
        private readonly string _name;
        private readonly bool _style;
        public override bool IsStyle => _style;
        public override string Description => _name;

        public SimpleBlock(int start, int length, string name, bool style = false)
            : base(start, length)
        {
            _name = name;
            _style = style;
        }
    }

    internal sealed class TextBlock : XmedBlock
    {
        public string Text { get; }
        public override string Description => "text";
        public override string Detail => $"text: \"{Text}\"";

        public TextBlock(int start, int length, string text)
            : base(start, length)
        {
            Text = text;
        }
    }

    internal sealed class StyleMapEntryBlock : XmedBlock
    {
        public ushort Field1 { get; }
        public ushort Field2 { get; }
        public ushort Field3 { get; }
        public ushort Field4 { get; }
        public string StyleId { get; }

        public override bool IsStyle => true;
        public override string Description => $"map {StyleId}";
        public override string Detail => $"map {StyleId} len {Field3}";

        public StyleMapEntryBlock(int start, ushort f1, ushort f2, ushort f3, ushort f4, string styleId)
            : base(start, 20)
        {
            Field1 = f1;
            Field2 = f2;
            Field3 = f3;
            Field4 = f4;
            StyleId = styleId;
        }
    }

    internal sealed class StyleDescriptorBlock : XmedBlock
    {
        public string StyleId { get; }
        public byte ColorIndex { get; }
        public string FontName { get; }
        public string Alignment { get; }
        public XmedStyle StyleFlags { get; }
        public XmedFlags ExtraFlags { get; }

        public override bool IsStyle => true;
        public override string Description => $"style {StyleId}";
        public override string Detail => $"{FontName} color {ColorIndex} {Alignment} {StyleFlags} {ExtraFlags}".Trim();

        public StyleDescriptorBlock(
            int start,
            int length,
            string styleId,
            byte colorIndex,
            string fontName,
            string alignment,
            XmedStyle styleFlags,
            XmedFlags extraFlags)
            : base(start, length)
        {
            StyleId = styleId;
            ColorIndex = colorIndex;
            FontName = fontName;
            Alignment = alignment;
            StyleFlags = styleFlags;
            ExtraFlags = extraFlags;
        }
    }

    internal sealed class FontEntryBlock : XmedBlock
    {
        public byte ColorIndex { get; }
        public string FontName { get; }

        public override string Description => $"font {FontName}";
        public override string Detail => $"index {ColorIndex}";

        public FontEntryBlock(int start, int length, string fontName, byte index)
            : base(start, length)
        {
            FontName = fontName;
            ColorIndex = index;
        }
    }

    internal sealed class XmedRelation
    {
        public int ByteOffset { get; }
        public XmedBlock Source { get; }
        public XmedBlock Target { get; }
        public string Type { get; }

        public XmedRelation(int byteOffset, XmedBlock source, XmedBlock target, string type)
        {
            ByteOffset = byteOffset;
            Source = source;
            Target = target;
            Type = type;
        }
    }

    internal sealed class XmedInterpretation
    {
        public IReadOnlyList<XmedBlock> Blocks { get; }
        public IReadOnlyList<XmedRelation> Relations { get; }

        public Dictionary<int, string> Offsets { get; } = new();
        public HashSet<int> StyleBlocks { get; } = new();

        public XmedInterpretation(List<XmedBlock> blocks, List<XmedRelation> relations)
        {
            Blocks = blocks;
            Relations = relations;

            foreach (var b in blocks)
            {
                for (int i = 0; i < b.Length; i++)
                {
                    int off = b.Start + i;
                    if (!Offsets.ContainsKey(off))
                        Offsets[off] = b.Description;
                    if (b.IsStyle)
                        StyleBlocks.Add(off);
                }
            }
        }
    }

    [Flags]
    internal enum XmedStyle : byte
    {
        None = 0,
        Bold = 1 << 0,
        Italic = 1 << 1,
        Underline = 1 << 2,
        Strikeout = 1 << 3,
        Subscript = 1 << 4,
        Superscript = 1 << 5,
        Tabbed = 1 << 6,
        Editable = 1 << 7
    }

    [Flags]
    internal enum XmedFlags : byte
    {
        None = 0,
        WrapOff = 1 << 0,
        Tabs = 1 << 4
    }

    internal static class XmedInterpreter
    {
        public static int FindXmedStart(byte[] data)
        {
            for (int i = 0; i <= data.Length - 4; i++)
            {
                if (data[i] == 0x58 && data[i + 1] == 0x4D && data[i + 2] == 0x45 && data[i + 3] == 0x44)
                    return i; // XMED
                if (data[i] == 0x44 && data[i + 1] == 0x45 && data[i + 2] == 0x4D && data[i + 3] == 0x58)
                    return i; // DEMX
            }
            return 0;
        }

        public static XmedInterpretation Interpret(byte[] data, IEnumerable<XmedBlock>? manualBlocks = null)
        {
            var blocks = manualBlocks != null ? new List<XmedBlock>(manualBlocks) : new List<XmedBlock>();
            var relations = new List<XmedRelation>();

            SimpleBlock? AddBlock(int start, int length, string desc, bool isStyle = false)
            {
                if (start >= data.Length || length <= 0)
                    return null;

                int len = Math.Min(length, data.Length - start);
                foreach (var b in blocks)
                {
                    int end = start + len - 1;
                    int bEnd = b.Start + b.Length - 1;
                    if (start <= bEnd && end >= b.Start)
                        return null; // overlap
                }
                var nb = new SimpleBlock(start, len, desc, isStyle);
                blocks.Add(nb);
                return nb;
            }

            bool TryAdd(XmedBlock block)
            {
                if (block.Start >= data.Length || block.Start + block.Length > data.Length)
                    return false;
                foreach (var b in blocks)
                {
                    int end = block.Start + block.Length - 1;
                    int bEnd = b.Start + b.Length - 1;
                    if (block.Start <= bEnd && end >= b.Start)
                        return false;
                }
                blocks.Add(block);
                return true;
            }

            AddBlock(0x04, 2, "header bytes");
            if (data.Length > 0x18)
            {
                AddBlock(0x18, 4, "width value");
            }
            if (data.Length > 0x1C)
            {
                AddBlock(0x1C, 1, $"style byte ({ParseStyleEnum(data[0x1C])})");
            }
            if (data.Length > 0x1D)
            {
                var (al, fl) = ParseFlagEnum(data[0x1D]);
                AddBlock(0x1D, 1, $"flags byte ({al} {fl})");
            }
            AddBlock(0x2C, 4, "header bytes");
            AddBlock(0x3C, 4, "line spacing");
            AddBlock(0x40, 4, "font size");
            AddBlock(0x4C, 4, "text length");
            AddBlock(0x50, 2, "header bytes");
            AddBlock(0x4DA, 4, "left margin");
            AddBlock(0x4DE, 4, "right margin");
            AddBlock(0x4E2, 4, "first indent");
            AddBlock(0x0622, 1, "color table");
            AddBlock(0x0983, 1, "font name");
            AddBlock(0x0CAE, 1, "spacing before");
            AddBlock(0x0EF7, 1, "member name");
            AddBlock(0x1354, 1, "color table");
            AddBlock(0x1970, 1, "spacing after");

            for (int i = 0; i <= data.Length - 4; i++)
            {
                if (data[i] == 0x58 && data[i + 1] == 0x46 && data[i + 2] == 0x49 && data[i + 3] == 0x52)
                {
                    AddBlock(i, 4, "XFIR");
                }
            }

            // Detect style descriptor blocks based on "40," font markers
            for (int i = 0; i < data.Length - 4; i++)
            {
                if (data[i] == 0x34 && data[i + 1] == 0x30 && data[i + 2] == 0x2C)
                {
                    // search backward for a four digit style id
                    int idEnd = i;
                    int idStart = idEnd - 4;
                    while (idStart > 1 && IsDigit(data[idStart - 1]))
                        idStart--;

                    if (idStart - 2 >= 0 && IsDigit(data[idStart]) && IsDigit(data[idStart + 1]) &&
                        IsDigit(data[idStart + 2]) && IsDigit(data[idStart + 3]))
                    {
                        int start = idStart - 2; // include style and flag bytes
                        string id = Encoding.ASCII.GetString(data, idStart, 4);

                        int j = i + 3;
                        while (j < data.Length && IsPrintable(data[j]))
                            j++;
                        if (j < data.Length && data[j] == 0x00)
                            j++;

                        int end = j;
                        while (end < data.Length && data[end] == 0x00)
                            end++;
                        // grab some extra trailing numbers that belong to this descriptor
                        int extra = 0;
                        while (end + extra < data.Length && extra < 16 && !IsPrintable(data[end + extra]))
                            extra++;

                        TryAdd(new SimpleBlock(start, end + extra - start, $"style {id}", true));
                        i = end + extra - 1;
                    }
                }
            }

            // Build lookup of style blocks by ID
            var styleBlocksById = new Dictionary<string, XmedBlock>();
            foreach (var b in blocks)
            {
                if (b.Description.StartsWith("style "))
                {
                    var id = b.Description.Substring(6).Trim();
                    styleBlocksById[id] = b;
                }
            }

            // Look for color table ASCII header followed by color entries
            var colorHeader = Encoding.ASCII.GetBytes("FFFF0000000600040001");
            for (int i = 0; i <= data.Length - colorHeader.Length; i++)
            {
                bool match = true;
                for (int j = 0; j < colorHeader.Length; j++)
                {
                    if (data[i + j] != colorHeader[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                {
                    var hdr = AddBlock(i, colorHeader.Length, "color table");
                    int pos = i + colorHeader.Length;
                    // parse 0x01XXXX sequences
                    while (pos + 5 <= data.Length && data[pos] == 0x01 &&
                           IsHexDigit(data[pos + 1]) && IsHexDigit(data[pos + 2]) &&
                           IsHexDigit(data[pos + 3]) && IsHexDigit(data[pos + 4]))
                    {
                        string hex = Encoding.ASCII.GetString(data, pos + 1, 4);
                        TryAdd(new SimpleBlock(pos, 5, $"color {hex}"));
                        pos += 5;
                    }
                }
            }

            // Detect sequences of digits that form the style mapping table
            for (int i = 0; i < data.Length;)
            {
                int start = i;
                if (data[i] == 0x03 && i + 21 <= data.Length && IsDigit(data[i + 1]))
                {
                    start = i + 1;
                }

                if (IsDigit(data[start]))
                {
                    int j = start;
                    while (j < data.Length && IsDigit(data[j]))
                        j++;
                    int len = j - start;
                    if (len >= 20)
                    {
                        string digits = Encoding.ASCII.GetString(data, start, len);
                        for (int off = 0; off + 20 <= len; off += 20)
                        {
                            string entry = digits.Substring(off, 20);
                            ushort f1 = Convert.ToUInt16(entry.Substring(0, 4));
                            ushort f2 = Convert.ToUInt16(entry.Substring(4, 4));
                            ushort f3 = Convert.ToUInt16(entry.Substring(8, 4));
                            ushort f4 = Convert.ToUInt16(entry.Substring(12, 4));
                            string id = entry.Substring(16, 4);
                            var map = new StyleMapEntryBlock(start + off, f1, f2, f3, f4, id);
                            if (TryAdd(map) && styleBlocksById.TryGetValue(id, out var sb))
                            {
                                relations.Add(new XmedRelation(start + off + 16, map, sb, "styleId"));
                            }
                        }
                        i = j;
                        continue;
                    }
                }
                i++;
            }

            // Detect standalone font entries of the form "40," <index> <font name>
            for (int i = 0; i + 4 < data.Length; i++)
            {
                if (data[i] == 0x34 && data[i + 1] == 0x30 && data[i + 2] == 0x2C)
                {
                    byte idx = data[i + 3];
                    int j = i + 4;
                    while (j < data.Length && IsPrintable(data[j]))
                        j++;
                    if (j > i + 4)
                    {
                        string font = Encoding.ASCII.GetString(data, i + 4, j - (i + 4));
                        var fb = new FontEntryBlock(i, j - i, font, idx);
                        if (TryAdd(fb))
                        {
                            i = j;
                            continue;
                        }
                    }
                }
            }

            // Detect printable ASCII sequences (text content)

            // Detect printable ASCII sequences (text content)
            for (int i = 0; i < data.Length;)
            {
                if (IsPrintable(data[i]))
                {
                    int start = i;
                    bool digitsOnly = true;
                    while (i < data.Length && IsPrintable(data[i]))
                    {
                        if (!IsDigit(data[i]))
                            digitsOnly = false;
                        i++;
                    }
                    int len = i - start;
                    if (len >= 4 && !digitsOnly)
                    {
                        var tb = new TextBlock(start, len, Encoding.ASCII.GetString(data, start, len));
                        TryAdd(tb);
                        continue;
                    }
                }
                i++;
            }

            // Enhance descriptions for style blocks
            var enhanced = new List<XmedBlock>();
            foreach (var b in blocks)
            {
                if (b.IsStyle && b.Description.StartsWith("style"))
                {
                    enhanced.Add(ParseStyleBlock(data, b));
                }
                else
                {
                    enhanced.Add(b);
                }
            }

            return new XmedInterpretation(enhanced, relations);
        }

        private static bool IsPrintable(byte b) => b >= 32 && b <= 126;

        private static bool IsDigit(byte b) => b >= (byte)'0' && b <= (byte)'9';

        private static bool IsHexDigit(byte b)
        {
            return b >= (byte)'0' && b <= (byte)'9' ||
                   b >= (byte)'A' && b <= (byte)'F' ||
                   b >= (byte)'a' && b <= (byte)'f';
        }

        private static XmedBlock ParseStyleBlock(byte[] data, XmedBlock block)
        {
            int start = block.Start;
            int end = start + block.Length;
            byte styleByte = data.Length > start ? data[start] : (byte)0;
            byte flagByte = data.Length > start + 1 ? data[start + 1] : (byte)0;

            string id = block.Description.Length > 6 ? block.Description.Substring(6) : block.Description;
            for (int i = start; i <= end - 4; i++)
            {
                if (IsDigit(data[i]) && IsDigit(data[i + 1]) && IsDigit(data[i + 2]) && IsDigit(data[i + 3]))
                {
                    id = Encoding.ASCII.GetString(data, i, 4);
                    break;
                }
            }

            int fontIndex = -1;
            for (int i = start; i <= end - 3; i++)
            {
                if (data[i] == 0x34 && data[i + 1] == 0x30 && data[i + 2] == 0x2C)
                {
                    fontIndex = i;
                    break;
                }
            }

            byte color = 0;
            string font = string.Empty;
            if (fontIndex >= 0 && fontIndex + 4 < data.Length)
            {
                color = data[fontIndex + 3];
                int j = fontIndex + 4;
                while (j < end && IsPrintable(data[j]))
                    j++;
                font = Encoding.ASCII.GetString(data, fontIndex + 4, Math.Max(0, j - (fontIndex + 4)));
            }

            var styleFlags = ParseStyleEnum(styleByte);
            var (align, extraFlags) = ParseFlagEnum(flagByte);

            return new StyleDescriptorBlock(block.Start, block.Length, id, color, font, align, styleFlags, extraFlags);
        }

        private static XmedStyle ParseStyleEnum(byte b)
        {
            XmedStyle flags = XmedStyle.None;
            if ((b & 0x01) != 0) flags |= XmedStyle.Bold;
            if ((b & 0x02) != 0) flags |= XmedStyle.Italic;
            if ((b & 0x04) != 0) flags |= XmedStyle.Underline;
            if ((b & 0x08) != 0) flags |= XmedStyle.Strikeout;
            if ((b & 0x10) != 0) flags |= XmedStyle.Subscript;
            if ((b & 0x20) != 0) flags |= XmedStyle.Superscript;
            if ((b & 0x40) != 0) flags |= XmedStyle.Tabbed;
            if ((b & 0x80) != 0) flags |= XmedStyle.Editable;
            return flags;
        }

        private static (string align, XmedFlags flags) ParseFlagEnum(byte b)
        {
            string align = b switch
            {
                0x1A => "left",
                0x15 => "right",
                _ => "center"
            };

            XmedFlags flags = XmedFlags.None;
            if ((b & 0x10) != 0) flags |= XmedFlags.Tabs;
            if (b == 0x19) flags |= XmedFlags.WrapOff;

            return (align, flags);
        }
    }
}
