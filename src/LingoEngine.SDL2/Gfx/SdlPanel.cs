using System;
using LingoEngine.Gfx;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlPanel : ILingoFrameworkPanel, IDisposable
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Visibility { get; set; } = true;

        public void AddChild(ILingoFrameworkGfxNode child) { }

        public void Dispose() { }
    }
}
