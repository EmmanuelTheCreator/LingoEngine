using LingoEngine.Primitives;
using System;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Engine level wrapper for a window element.
    /// </summary>
    public class LingoGfxWindow : LingoGfxNodeLayoutBase<ILingoFrameworkGfxWindow>
    {
        public string Title { get => _framework.Title; set => _framework.Title = value; }
        public LingoColor BackgroundColor { get => _framework.BackgroundColor; set => _framework.BackgroundColor = value; }
        public bool IsPopup { get => _framework.IsPopup; set => _framework.IsPopup = value; }

        public event Action? OnOpen
        {
            add { _framework.OnOpen += value; }
            remove { _framework.OnOpen -= value; }
        }
        public event Action? OnClose
        {
            add { _framework.OnClose += value; }
            remove { _framework.OnClose -= value; }
        }
        public event Action<float, float>? OnResize
        {
            add { _framework.OnResize += value; }
            remove { _framework.OnResize -= value; }
        }

        public LingoGfxWindow AddItem(ILingoGfxNode node)
        {
            _framework.AddItem(node.Framework<ILingoFrameworkGfxLayoutNode>());
            return this;
        }

        public LingoGfxWindow AddItem(ILingoFrameworkGfxLayoutNode node)
        {
            _framework.AddItem(node);
            return this;
        }

        public void RemoveItem(ILingoGfxNode node) => _framework.RemoveItem(node.Framework<ILingoFrameworkGfxLayoutNode>());
        public void RemoveItem(ILingoFrameworkGfxLayoutNode node) => _framework.RemoveItem(node);
        public IEnumerable<ILingoFrameworkGfxLayoutNode> GetItems() => _framework.GetItems();

        public void Popup() => _framework.Popup();
        public void PopupCentered() => _framework.PopupCentered();
        public void Hide() => _framework.Hide();
    }
}
