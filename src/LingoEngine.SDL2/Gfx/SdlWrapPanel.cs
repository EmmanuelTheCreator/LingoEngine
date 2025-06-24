using System;
using LingoEngine.Gfx;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlWrapPanel : ILingoFrameworkWrapPanel, IDisposable
    {
        public LingoOrientation Orientation { get; set; }

        public SdlWrapPanel(LingoOrientation orientation)
        {
            Orientation = orientation;
        }

        public void AddChild(ILingoFrameworkWrapPanel child) { }

        public void Dispose() { }
    }
}
