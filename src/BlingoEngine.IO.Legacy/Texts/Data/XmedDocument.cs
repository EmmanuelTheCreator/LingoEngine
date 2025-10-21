using BlingoEngine.IO.Legacy.Texts.Data;
using BlingoEngine.IO.Legacy.Texts.Data.Pre10;

using System;

namespace BlingoEngine.IO.Legacy.Texts
{
   
    /// <summary>Parsed XMED document.</summary>
    public sealed class XmedDocument
    {
        public string Text { get; set; } = string.Empty;
        public List<XmedTextRun> Runs { get; set; } = new();
        public List<XmedStyleDescriptor> Styles { get; set; } = new();
        public List<XmedFontDescriptor> Fonts { get; } = new();
        public List<XmedParagraphDescriptor> Paragraphs { get; set; } = new();
        public int Width { get; set; }
        public int Height { get; internal set; }
        public int LineSpacing { get; set; }
        public bool AllowTabs { get; set; }
        public bool IsEditable { get; set; }
        public bool IsWrapOff { get; set; }
        public int TextLength { get; set; }
        public int DirectorVersion { get; set; }
        public XmedRichTextMetadata? RichText { get; set; }

        public void FillParagraphTexts()
        {
            if (Paragraphs.Count == 0 || string.IsNullOrEmpty(Text))
                return;

            foreach (var paragraph in Paragraphs)
            {
                int start = Math.Clamp(paragraph.Start, 0, TextLength);
                int available = Math.Max(0, TextLength - start);
                int length = Math.Clamp(paragraph.Length, 0, available);
                paragraph.Text = length > 0 ? Text.Substring(start, length) : string.Empty;
            }
        }
    }
}
