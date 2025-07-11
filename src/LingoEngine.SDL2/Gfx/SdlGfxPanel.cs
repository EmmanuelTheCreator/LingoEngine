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
        private readonly List<ILingoFrameworkGfxLayoutNode> _children = new();
        public object FrameworkNode => this;

        LingoColor? ILingoFrameworkGfxPanel.BackgroundColor { get => BackgroundColor; set => BackgroundColor = value ?? BackgroundColor; }
        LingoColor? ILingoFrameworkGfxPanel.BorderColor { get => BorderColor; set => BorderColor = value ?? BorderColor; }

        public void AddItem(ILingoFrameworkGfxLayoutNode child) => _children.Add(child);

        public void Dispose() { }

        public IEnumerable<ILingoFrameworkGfxLayoutNode> GetItems() => _children;

        public void RemoveItem(ILingoFrameworkGfxLayoutNode child)
        {
            _children.Remove(child);
        }
    }
}
