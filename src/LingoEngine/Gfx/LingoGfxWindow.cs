using System.Collections.Generic;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Engine level wrapper for a window element.
    /// </summary>
    public class LingoGfxWindow : LingoGfxNodeLayoutBase<ILingoFrameworkGfxWindow>
    {
        public string Title { get => _framework.Title; set => _framework.Title = value; }

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
