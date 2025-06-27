using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Xml.Linq;
using LingoEngine.Gfx;
using LingoEngine.Primitives;

namespace LingoEngine.SDL2.Gfx
{
    public class SdlGfxTabContainer : ILingoFrameworkGfxTabContainer, IDisposable
    {
        private readonly List<ILingoFrameworkGfxTabItem> _children = new();
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Visibility { get; set; } = true;
        public string Name { get; set; } = string.Empty;
        public LingoMargin Margin { get; set; } = LingoMargin.Zero;

        public void Dispose() { }

        public void AddTab(ILingoFrameworkGfxTabItem content)
        {
            _children.Add(content);
        }

        public void ClearTabs() => _children.Clear();

       

        public IEnumerable<ILingoFrameworkGfxTabItem> GetTabs()
        {
            return _children.ToArray();
        }

        public void RemoveTab(ILingoFrameworkGfxTabItem content)
        {
            _children.Add(content);
        }
    }
    public partial class SdlGfxTabItem : ILingoFrameworkGfxTabItem
    {
        private LingoGfxTabItem tab;

        public SdlGfxTabItem(LingoGfxTabItem tab)
        {
            this.tab = tab;
            tab.Init(this);
        }

        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public bool Visibility { get; set; } = true;
        public string Name { get; set; } = string.Empty;
        public string Title { get; set; } = "";
        public LingoMargin Margin { get; set; }

        public void Dispose()
        {
            
        }
    }
}
