using System;
using System.Collections.Generic;

namespace LingoEngine.Director.LGodot.Gfx
{
    internal sealed class XmedInterpretation
    {
        public Dictionary<int,string> Offsets { get; }
        public HashSet<int> StyleBlocks { get; }

        public XmedInterpretation(Dictionary<int,string> offsets, HashSet<int> blocks)
        {
            Offsets = offsets;
            StyleBlocks = blocks;
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
        public static XmedInterpretation Interpret(byte[] data, Dictionary<int,string>? manualOffsets = null, HashSet<int>? manualBlocks = null)
        {
            var offsets = manualOffsets != null ? new Dictionary<int,string>(manualOffsets) : new Dictionary<int,string>();
            var blocks = manualBlocks != null ? new HashSet<int>(manualBlocks) : new HashSet<int>();

            void AddOffset(int off, string desc)
            {
                if (off < data.Length && !offsets.ContainsKey(off))
                    offsets[off] = desc;
            }

            if (data.Length > 0x18)
            {
                AddOffset(0x18, "width value");
            }
            if (data.Length > 0x1C)
            {
                AddOffset(0x1C, $"style byte ({DescribeStyle(data[0x1C])})");
            }
            if (data.Length > 0x1D)
            {
                AddOffset(0x1D, $"flags byte ({DescribeFlags(data[0x1D])})");
            }
            AddOffset(0x3C, "line spacing");
            AddOffset(0x40, "font size");
            AddOffset(0x4C, "text len");
            AddOffset(0x4DA, "left margin");
            AddOffset(0x4DE, "right margin");
            AddOffset(0x4E2, "first indent");
            AddOffset(0x0CAE, "spacing before");
            AddOffset(0x1970, "spacing after");

            for (int i = 0; i <= data.Length - 4; i++)
            {
                if (data[i] == 0x58 && data[i + 1] == 0x46 && data[i + 2] == 0x49 && data[i + 3] == 0x52)
                {
                    AddOffset(i, "XFIR");
                }
            }

            return new XmedInterpretation(offsets, blocks);
        }

        private static string DescribeStyle(byte b)
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
            return flags == XmedStyle.None ? "0x" + b.ToString("X2") : flags.ToString();
        }

        private static string DescribeFlags(byte b)
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

            return flags == XmedFlags.None ? align : $"{align} {flags}";
        }
    }
}
