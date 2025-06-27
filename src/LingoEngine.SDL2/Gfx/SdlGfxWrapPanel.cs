using System;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxWrapPanel : ILingoFrameworkGfxWrapPanel, IDisposable
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Visibility { get; set; } = true;
        public string Name { get; set; } = string.Empty;
        public LingoOrientation Orientation { get; set; }
        public LingoMargin ItemMargin { get; set; }
        public LingoMargin Margin { get; set; }

        public SdlGfxWrapPanel(LingoOrientation orientation)
        {
            Orientation = orientation;
            ItemMargin = LingoMargin.Zero;
            Margin = LingoMargin.Zero;
        }

        public void AddChild(ILingoFrameworkGfxNode child) { }

        public void Dispose() { }

        public void RemoveChild(ILingoFrameworkGfxNode child)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ILingoFrameworkGfxNode> GetChildren()
        {
            throw new NotImplementedException();
        }

        public ILingoFrameworkGfxNode? GetChild(int index)
        {
            throw new NotImplementedException();
        }
    }
}
