using System;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxPanel : ILingoFrameworkGfxPanel, IDisposable
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Visibility { get; set; } = true;
        public LingoMargin Margin { get; set; } = LingoMargin.Zero;
        public LingoColor BackgroundColor { get; set; }
        public LingoColor BorderColor { get; set; }
        public float BorderWidth { get; set; }

        public void AddChild(ILingoFrameworkGfxNode child) { }

        public void Dispose() { }
    }
}
