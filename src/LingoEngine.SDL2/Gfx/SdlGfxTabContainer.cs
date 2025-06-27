using System;
using System.Collections.Generic;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.SDL2.Gfx
{
    internal class SdlGfxTabContainer : ILingoFrameworkGfxTabContainer, IDisposable
    {
        private readonly List<ILingoFrameworkGfxNode> _children = new();
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Visibility { get; set; } = true;
        public string Name { get; set; } = string.Empty;
        public LingoMargin Margin { get; set; } = LingoMargin.Zero;

        public void AddTab(string title, ILingoFrameworkGfxNode content)
        {
            _children.Add(content);
        }

        public void ClearTabs() => _children.Clear();

        public void Dispose() { }
    }
}
