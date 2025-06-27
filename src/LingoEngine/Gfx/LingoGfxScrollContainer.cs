namespace LingoEngine.Gfx
{
    /// <summary>
    /// Engine level wrapper around a framework scroll container.
    /// </summary>
    public class LingoGfxScrollContainer : LingoGfxNodeBase<ILingoFrameworkGfxScrollContainer>
    {
        public void AddChild(ILingoGfxNode node)
            => _framework.AddChild(node.Framework<ILingoFrameworkGfxNode>());

        public float ScrollHorizontal { get => _framework.ScrollHorizontal; set => _framework.ScrollHorizontal = value; }
        public float ScrollVertical { get => _framework.ScrollVertical; set => _framework.ScrollVertical = value; }
        public bool ClipContents { get => _framework.ClipContents; set => _framework.ClipContents = value; }
    }
}
