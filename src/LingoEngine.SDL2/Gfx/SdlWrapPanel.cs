using System;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlWrapPanel : ILingoFrameworkWrapPanel, IDisposable
    {
        public LingoOrientation Orientation { get; set; }
        public LingoMargin ItemMargin { get; set; }
        public LingoMargin Margin { get; set; }

        public SdlWrapPanel(LingoOrientation orientation)
        {
            Orientation = orientation;
            ItemMargin = LingoMargin.Zero;
            Margin = LingoMargin.Zero;
        }

        public void AddChild(ILingoFrameworkWrapPanel child) { }

        public void Dispose() { }
    }
}
