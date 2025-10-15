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
        public int StyleId { get; set; }
        public string FontName { get; set; } = string.Empty;
        public byte? ColorIndex { get; set; }
        public int? FontSize { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public bool Underline { get; set; }
        public bool Strikeout { get; set; }
        public bool Subscript { get; set; }
        public bool Superscript { get; set; }
        public XmedAlignment Alignment { get; set; } = XmedAlignment.Center;
       
        public int LineSpacing { get; set; }
        public int BaseLineOffset { get; set; }

        public XmedStyleFlags Flags { get; set; }
        public BlLegacyColor Color { get; internal set; }


        public void ApplyStyleInheritanceToChild(XmedStyleDescriptor child)
        {
            if (string.IsNullOrEmpty(child.FontName)) child.FontName = FontName;
            if (child.FontSize == 0) child.FontSize = FontSize;

            if (!child.Bold && Bold) child.Bold = true;
            if (!child.Italic && Italic) child.Italic = true;
            if (!child.Underline && Underline) child.Underline = true;
            if (!child.Strikeout && Strikeout) child.Strikeout = true;
            if (!child.Subscript && Subscript) child.Subscript = true;
            if (!child.Superscript && Superscript) child.Superscript = true;

            if (child.Alignment == XmedAlignment.Center && Alignment != XmedAlignment.Center)
                child.Alignment = Alignment;

            if (child.ColorIndex == 0) { child.ColorIndex = ColorIndex; child.Color = Color; }

            if (child.LineSpacing == 0) child.LineSpacing = LineSpacing;
            if (child.BaseLineOffset == 0) child.BaseLineOffset = BaseLineOffset;

            if (child.Flags == XmedStyleFlags.None && Flags != XmedStyleFlags.None)
                child.Flags = Flags;

        }

        public void ApplyStyleFlag(XmedStyleFlags flag, bool enabled)
        {
            Flags = enabled ? Flags | flag : Flags & ~flag;

            Bold = Flags.HasFlag(XmedStyleFlags.Bold);
            Italic = Flags.HasFlag(XmedStyleFlags.Italic);
            Underline = Flags.HasFlag(XmedStyleFlags.Underline);
            Strikeout = Flags.HasFlag(XmedStyleFlags.Strikeout);
            Subscript = Flags.HasFlag(XmedStyleFlags.Subscript);
            Superscript = Flags.HasFlag(XmedStyleFlags.Superscript);
        }
    }
}
