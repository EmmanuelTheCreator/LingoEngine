using System;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlInputNumber : ILingoFrameworkInputNumber, IDisposable
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Visibility { get; set; } = true;
        public float Value { get; set; }
        public float Min { get; set; }
        public float Max { get; set; }
        public LingoNumberType NumberType { get; set; } = LingoNumberType.Float;
        public LingoMargin Margin { get; set; } = LingoMargin.Zero;

        public void Dispose() { }
    }
}
