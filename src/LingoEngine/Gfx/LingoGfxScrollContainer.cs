namespace LingoEngine.Gfx
{
    /// <summary>
    /// Engine level wrapper around a framework scroll container.
    /// </summary>
    public class LingoGfxScrollContainer : LingoGfxNodeLayoutBase<ILingoFrameworkGfxScrollContainer>
    {
        public void AddItem(ILingoGfxNode node) => _framework.AddItem(node.Framework<ILingoFrameworkGfxLayoutNode>());
        public void AddItem(ILingoFrameworkGfxLayoutNode node) => _framework.AddItem(node);
        public void RemoveItem(ILingoGfxNode node) => _framework.RemoveItem(node.Framework<ILingoFrameworkGfxLayoutNode>());
        public void RemoveItem(ILingoFrameworkGfxLayoutNode node) => _framework.RemoveItem(node);
        public IEnumerable<ILingoFrameworkGfxLayoutNode> GetItems() => _framework.GetItems();

        public float ScrollHorizontal { get => _framework.ScrollHorizontal; set => _framework.ScrollHorizontal = value; }
        public float ScrollVertical { get => _framework.ScrollVertical; set => _framework.ScrollVertical = value; }
        public bool ClipContents { get => _framework.ClipContents; set => _framework.ClipContents = value; }
    }
}
