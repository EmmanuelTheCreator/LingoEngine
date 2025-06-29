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

        public void AddChild(ILingoGfxNode node) => _framework.AddItem(node.Framework<ILingoFrameworkGfxNode>());
        public void AddChild(ILingoFrameworkGfxNode node) => _framework.AddItem(node);
        public void RemoveChild(ILingoGfxNode node) => _framework.RemoveItem(node.Framework<ILingoFrameworkGfxNode>());
        public void RemoveChild(ILingoFrameworkGfxNode node) => _framework.RemoveItem(node);
        public IEnumerable<ILingoFrameworkGfxNode> GetChildren() => _framework.GetItems();

        public ILingoFrameworkGfxNode? GetChild(int index) => _framework.GetItem(index);
    }
}
