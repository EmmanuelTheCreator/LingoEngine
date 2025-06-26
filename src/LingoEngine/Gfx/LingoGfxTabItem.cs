namespace LingoEngine.Gfx
{
    /// <summary>
    /// Represents a single tab item with a title and content node.
    /// </summary>
    public class LingoGfxTabItem
    {
        public string Title { get; set; }
        public ILingoGfxNode Content { get; }

        public LingoGfxTabItem(string title, ILingoGfxNode content)
        {
            Title = title;
            Content = content;
        }
    }
}
