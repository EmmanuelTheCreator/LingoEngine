using BlingoEngine.IO.Legacy.Core;

namespace BlingoEngine.IO.Legacy.Texts.Data
{
    /// <summary>Single styled text run.</summary>
    public sealed class XmedTextRun
    {
        public int Start { get; set; }
        public int Length { get; set; }
        public string Text { get; set; } = string.Empty;
        public string FontName { get; set; } = string.Empty;
        public ushort FontSize { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public bool Underline { get; set; }
        public BlLegacyColor ForeColor { get; set; }
    }
}
