namespace LingoEngine.Gfx
{
    /// <summary>
    /// Engine level wrapper around a framework scroll container.
    /// </summary>
    public class LingoGfxScrollContainer : LingoGfxNodeLayoutBase<ILingoFrameworkGfxScrollContainer>
    {
        public void AddChild(ILingoGfxNode node) => _framework.AddItem(node.Framework<ILingoFrameworkGfxLayoutNode>());
        public void AddChild(ILingoFrameworkGfxLayoutNode node) => _framework.AddItem(node);
        public void RemoveChild(ILingoGfxNode node) => _framework.RemoveItem(node.Framework<ILingoFrameworkGfxLayoutNode>());
        public void RemoveChild(ILingoFrameworkGfxLayoutNode node) => _framework.RemoveItem(node);
        public IEnumerable<ILingoFrameworkGfxLayoutNode> GetChildren() => _framework.GetItems();

        public float ScrollHorizontal { get => _framework.ScrollHorizontal; set => _framework.ScrollHorizontal = value; }
        public float ScrollVertical { get => _framework.ScrollVertical; set => _framework.ScrollVertical = value; }
        public bool ClipContents { get => _framework.ClipContents; set => _framework.ClipContents = value; }
    }
}
