namespace LingoEngine.Gfx
{
    /// <summary>
    /// Engine level wrapper around a framework scroll container.
    /// </summary>
    public class LingoGfxScrollContainer : LingoGfxNodeBase<ILingoFrameworkGfxScrollContainer>
    {
        public void AddChild(ILingoGfxNode node) => _framework.AddChild(node.Framework<ILingoFrameworkGfxNode>());
        public void AddChild(ILingoFrameworkGfxNode node) => _framework.AddChild(node);
        public void RemoveChild(ILingoGfxNode node) => _framework.RemoveChild(node.Framework<ILingoFrameworkGfxNode>());
        public void RemoveChild(ILingoFrameworkGfxNode node) => _framework.RemoveChild(node);
        public IEnumerable<ILingoFrameworkGfxNode> GetChildren() => _framework.GetChildren();

        public float ScrollHorizontal { get => _framework.ScrollHorizontal; set => _framework.ScrollHorizontal = value; }
        public float ScrollVertical { get => _framework.ScrollVertical; set => _framework.ScrollVertical = value; }
        public bool ClipContents { get => _framework.ClipContents; set => _framework.ClipContents = value; }
    }
}
