using System;
using LingoEngine.Gfx;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxMenuItem : ILingoFrameworkGfxMenuItem, IDisposable
    {
        public string Name { get; set; }
        public bool Enabled { get; set; } = true;
        public bool CheckMark { get; set; }
        public string? Shortcut { get; set; }
        public event Action? Activated;

        public SdlGfxMenuItem(string name, string? shortcut)
        {
            Name = name;
            Shortcut = shortcut;
        }

        public void Invoke() => Activated?.Invoke();
        public void Dispose() { }
    }
}
