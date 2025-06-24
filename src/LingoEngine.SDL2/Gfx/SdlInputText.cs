using System;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlInputText : ILingoFrameworkInputText, IDisposable
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Visibility { get; set; } = true;
        public string Text { get; set; } = string.Empty;
        public int MaxLength { get; set; }
        public string? Font { get; set; }
        public LingoMargin Margin { get; set; } = LingoMargin.Zero;

        public void Dispose() { }
    }
}
