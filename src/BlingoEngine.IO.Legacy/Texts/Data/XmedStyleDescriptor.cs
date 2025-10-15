using BlingoEngine.IO.Legacy.Core;

namespace BlingoEngine.IO.Legacy.Texts.Data
{
    /// <summary>Style descriptor parsed from XMED.</summary>
    public sealed class XmedStyleDescriptor
    {
        [Flags]
        public enum XmedStyleFlags : byte
        {
            None = 0,
            Bold = 1 << 0,
            Italic = 1 << 1,
            Underline = 1 << 2,
            Strikeout = 1 << 3,
            Subscript = 1 << 4,
            Superscript = 1 << 5,
            TabbedField = 1 << 6
        }
        public ushort StyleId { get; set; }
        public string FontName { get; set; } = string.Empty;
        public byte ColorIndex { get; set; }
        public ushort FontSize { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public bool Underline { get; set; }
        public bool Strikeout { get; set; }
        public bool Subscript { get; set; }
        public bool Superscript { get; set; }
        public XmedAlignment Alignment { get; set; } = XmedAlignment.Center;
       
        public byte StyleFlags { get; set; }
        public int LineSpacing { get; set; }
        public int BaseLineOffset { get; set; }

        public XmedStyleFlags Flags { get; set; }
        public BlLegacyColor Color { get; internal set; }
    }
}
