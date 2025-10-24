using BlingoEngine.IO.Legacy.Core;

namespace BlingoEngine.IO.Legacy.Texts.Data
{
    /// <summary>Single styled text run.</summary>
    public sealed class XmedTextRun
    {
        public int Start { get; set; }
        public int Length { get; set; }
        public string Text { get; set; } = string.Empty;
        public int StyleId { get; set; }
        public string FontName { get; set; } = string.Empty;
        public int? FontSize { get; set; }
        public int LetterSpacing { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public bool Underline { get; set; }
        public bool Strikeout { get; set; }
        public bool Subscript { get; set; }
        public bool Superscript { get; set; }
        public bool TabbedField { get; set; }
        public XmedAlignment Alignment { get; set; }
        public BlLegacyColor ForeColor { get; set; }
        public BlLegacyColor BackgroundColor { get; set; }
    }
}
