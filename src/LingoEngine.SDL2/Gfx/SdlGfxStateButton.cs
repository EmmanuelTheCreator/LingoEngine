using System;
using LingoEngine.Gfx;
using LingoEngine.Primitives;
using LingoEngine.Bitmaps;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxStateButton : ILingoFrameworkGfxStateButton, IDisposable
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Visibility { get; set; } = true;
        public string Name { get; set; } = string.Empty;
        public bool Enabled { get; set; } = true;
        public string Text { get; set; } = string.Empty;
        public Bitmaps.ILingoTexture2D? Texture { get; set; }
        private bool _isOn;
        public bool IsOn
        {
            get => _isOn;
            set
            {
                if (_isOn != value)
                {
                    _isOn = value;
                    ValueChanged?.Invoke();
                }
            }
        }
        public LingoMargin Margin { get; set; } = LingoMargin.Zero;
        public event Action? ValueChanged;
        public object FrameworkNode => this;
        public void Dispose() { }
    }
}
