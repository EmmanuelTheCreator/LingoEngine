using System;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxSpinBox : ILingoFrameworkGfxSpinBox, IDisposable
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Visibility { get; set; } = true;
        public string Name { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;
        public float Value { get; set; }
        public float Min { get; set; }
        public float Max { get; set; }
        public LingoMargin Margin { get; set; } = LingoMargin.Zero;
        public event Action? ValueChanged;
        public void Dispose() { }
    }
}
