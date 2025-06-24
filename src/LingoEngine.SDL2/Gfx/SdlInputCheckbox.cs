using System;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlInputCheckbox : ILingoFrameworkGfxInputCheckbox, IDisposable
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Visibility { get; set; } = true;
        public bool Enabled { get; set; } = true;
        private bool _checked;
        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked != value)
                {
                    _checked = value;
                    ValueChanged?.Invoke();
                }
            }
        }
        public LingoMargin Margin { get; set; } = LingoMargin.Zero;
        public event Action? ValueChanged;

        public void Dispose() { }
    }
}
