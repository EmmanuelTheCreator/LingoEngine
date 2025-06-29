using System;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxPanel : ILingoFrameworkGfxPanel, IDisposable
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Visibility { get; set; } = true;
        public string Name { get; set; } = string.Empty;
        public LingoMargin Margin { get; set; } = LingoMargin.Zero;
        public LingoColor BackgroundColor { get; set; }
        public LingoColor BorderColor { get; set; }
        public float BorderWidth { get; set; }
        public object FrameworkNode => this;
        public void AddItem(ILingoFrameworkGfxLayoutNode child) { }

        public void Dispose() { }

        public IEnumerable<ILingoFrameworkGfxLayoutNode> GetItems()
        {
            throw new NotImplementedException();
        }

        public void RemoveItem(ILingoFrameworkGfxLayoutNode child)
        {
            throw new NotImplementedException();
        }
    }
}
