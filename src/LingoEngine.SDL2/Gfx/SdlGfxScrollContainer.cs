using System;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxScrollContainer : ILingoFrameworkGfxScrollContainer, IDisposable
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Visibility { get; set; } = true;
        public string Name { get; set; } = string.Empty;
        public LingoMargin Margin { get; set; } = LingoMargin.Zero;
        public float ScrollHorizontal { get; set; }
        public float ScrollVertical { get; set; }
        public bool ClipContents { get; set; }
        private readonly List<ILingoFrameworkGfxLayoutNode> _children = new();
        public object FrameworkNode => this;
        public void AddItem(ILingoFrameworkGfxLayoutNode child) => _children.Add(child);
        public void Dispose() { }

        public IEnumerable<ILingoFrameworkGfxLayoutNode> GetItems() => _children;

        public void RemoveItem(ILingoFrameworkGfxLayoutNode lingoFrameworkGfxNode)
        {
            _children.Remove(lingoFrameworkGfxNode);
        }
    }
}
