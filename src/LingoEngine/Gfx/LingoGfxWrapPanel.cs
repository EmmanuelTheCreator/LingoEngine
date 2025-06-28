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

        public void AddChild(ILingoGfxNode node) => _framework.AddChild(node.Framework<ILingoFrameworkGfxLayoutNode>());
        public void AddChild(ILingoFrameworkGfxLayoutNode node) => _framework.AddChild(node);
        public void RemoveChild(ILingoGfxNode node) => _framework.RemoveChild(node.Framework<ILingoFrameworkGfxLayoutNode>());
        public void RemoveChild(ILingoFrameworkGfxLayoutNode node) => _framework.RemoveChild(node);
        public IEnumerable<ILingoFrameworkGfxLayoutNode> GetChildren() => _framework.GetChildren();

        public ILingoFrameworkGfxLayoutNode? GetChild(int index) => _framework.GetChild(index);
    }
}
