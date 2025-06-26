using System;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlMenu : ILingoFrameworkGfxMenu, IDisposable
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Visibility { get; set; } = true;
        public LingoMargin Margin { get; set; } = LingoMargin.Zero;
        public string Name { get; set; }

        public SdlMenu(string name)
        {
            Name = name;
        }

        public void AddItem(ILingoFrameworkGfxMenuItem item) { }
        public void ClearItems() { }
        public void Dispose() { }
    }
}
