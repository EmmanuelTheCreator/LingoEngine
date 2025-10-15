using BlingoEngine.IO.Legacy.Texts.Data;
using BlingoEngine.IO.Legacy.Texts.Data.Pre10;

namespace BlingoEngine.IO.Legacy.Texts
{
    public enum XmedEntryKind
    {
        Unknown, Text, TokenList, StyleRuns, Fonts, Sizes, Colors, Weights, Italics, Underlines, Spacing, Align, Justify,
        Index
    }


    /// <summary>Parsed XMED document.</summary>
    public sealed class XmedDocument
    {
        public string Text { get; set; } = string.Empty;
        public List<XmedTextRun> Runs { get; set; } = new();
        public List<XmedStyleDescriptor> Styles { get; set; } = new();
        public List<XmedRunMapEntry> RunMap { get; set; } = new();
        public List<XmedParagraphDescriptor> Paragraphs { get; set; } = new();
        public uint Width { get; set; }
        public uint LineSpacing { get; set; }
        public int TextLength { get; set; }
        public int DirectorVersion { get; set; }
        public XmedRichTextMetadata? RichText { get; set; }
        public uint Height { get; internal set; }
    }
}
