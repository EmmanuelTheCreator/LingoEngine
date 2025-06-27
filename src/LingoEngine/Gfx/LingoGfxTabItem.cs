namespace LingoEngine.Gfx
{
    /// <summary>
    /// Represents a single tab item with a title and content node.
    /// </summary>
    public class LingoGfxTabItem : LingoGfxNodeBase<ILingoFrameworkGfxTabItem>
    {
        public string Title { get; set; }
        public ILingoGfxNode? Content { get; set; }

        public LingoGfxTabItem(string title)
        {
            Title = title;
        }
    }
}
