using System;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlWrapPanel : ILingoFrameworkWrapPanel, IDisposable
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Visibility { get; set; } = true;
        public LingoOrientation Orientation { get; set; }
        public LingoMargin ItemMargin { get; set; }
        public LingoMargin Margin { get; set; }

        public SdlWrapPanel(LingoOrientation orientation)
        {
            Orientation = orientation;
            ItemMargin = LingoMargin.Zero;
            Margin = LingoMargin.Zero;
        }

        public void AddChild(ILingoFrameworkGfxNode child) { }

        public void Dispose() { }
    }
}
