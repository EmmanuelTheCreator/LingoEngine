using System;
using LingoEngine.Gfx;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlInputCheckbox : ILingoFrameworkInputCheckbox, IDisposable
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Visibility { get; set; } = true;
        public bool Checked { get; set; }

        public void Dispose() { }
    }
}
