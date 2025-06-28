namespace LingoEngine.Gfx
{
    /// <summary>
    /// Represents a single tab item with a title and content node.
    /// </summary>
    public class LingoGfxTabItem : LingoGfxNodeBase<ILingoFrameworkGfxTabItem>
    {
        public string Title { get => _framework.Title; set => _framework.Title = value; }
        public ILingoGfxNode? Content { get => _framework.Content; set => _framework.Content = value;  }

        public LingoGfxTabItem()
        {
        }
    }
}
