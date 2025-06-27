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
        public void AddChild(ILingoFrameworkGfxNode child) { }
        public void Dispose() { }

        public IEnumerable<ILingoFrameworkGfxNode> GetChildren()
        {
            throw new NotImplementedException();
        }

        public void RemoveChild(ILingoFrameworkGfxNode lingoFrameworkGfxNode)
        {
            throw new NotImplementedException();
        }
    }
}
