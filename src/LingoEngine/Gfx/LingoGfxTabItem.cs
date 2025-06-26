namespace LingoEngine.Gfx
{
    /// <summary>
    /// Represents a single tab item with a title and content node.
    /// </summary>
    public class LingoGfxTabItem
    {
        public string Title { get; set; }
        public LingoGfxNode Content { get; }

        public LingoGfxTabItem(string title, LingoGfxNode content)
        {
            Title = title;
            Content = content;
        }
    }
}
