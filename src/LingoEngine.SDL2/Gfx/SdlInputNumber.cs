using System;
using LingoEngine.Gfx;

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

        public void Dispose() { }
    }
}
