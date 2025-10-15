using BlingoEngine.IO.Legacy.Core;

namespace BlingoEngine.IO.Legacy.Texts.Data.Pre10
{
  

    /// <summary>Legacy rect for old rich text streams.</summary>
    public sealed class XmedRect
    {
        public short Top { get; set; }
        public short Left { get; set; }
        public short Bottom { get; set; }
        public short Right { get; set; }
    }
    /// <summary>Legacy rich text metadata (Director ≤10).</summary>
    public sealed class XmedRichTextMetadata
    {
        public XmedRect InitialRect { get; set; } = new();
        public XmedRect BoundingRect { get; set; } = new();
        public byte AntialiasFlag { get; set; }
        public byte CropFlags { get; set; }
        public ushort ScrollPosition { get; set; }
        public ushort AntialiasFontSize { get; set; }
        public ushort DisplayHeight { get; set; }
        public BlLegacyColor ForegroundColor { get; set; }
        public BlLegacyColor BackgroundColor { get; set; }
    }
}
