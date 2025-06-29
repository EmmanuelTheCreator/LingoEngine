using LingoEngine.Primitives;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Engine level wrapper for a panel that arranges children with wrapping.
    /// </summary>
    public class LingoGfxWrapPanel : LingoGfxNodeLayoutBase<ILingoFrameworkGfxWrapPanel>
    {

        public LingoOrientation Orientation
        {
            get => _framework.Orientation;
            set => _framework.Orientation = value;
        }

        public LingoMargin ItemMargin
        {
            get => _framework.ItemMargin;
            set => _framework.ItemMargin = value;
        }

        public void AddItem(ILingoGfxNode node) => _framework.AddItem(node.Framework<ILingoFrameworkGfxNode>());
        public void AddItem(ILingoFrameworkGfxNode node) => _framework.AddItem(node);
        public void RemoveItem(ILingoGfxNode node) => _framework.RemoveItem(node.Framework<ILingoFrameworkGfxNode>());
        public void RemoveItem(ILingoFrameworkGfxNode node) => _framework.RemoveItem(node);
        public IEnumerable<ILingoFrameworkGfxNode> GetItems() => _framework.GetItems();

        public ILingoFrameworkGfxNode? GetItem(int index) => _framework.GetItem(index);
    }
}
