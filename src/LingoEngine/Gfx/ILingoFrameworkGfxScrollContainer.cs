
namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific scroll container implementation.
    /// </summary>
    public interface ILingoFrameworkGfxScrollContainer : ILingoFrameworkGfxLayoutNode
    {
        void AddItem(ILingoFrameworkGfxLayoutNode child);
        void RemoveItem(ILingoFrameworkGfxLayoutNode lingoFrameworkGfxNode);
        IEnumerable<ILingoFrameworkGfxLayoutNode> GetItems();

        float ScrollHorizontal { get; set; }
        float ScrollVertical { get; set; }
        bool ClipContents { get; set; }
    }
}
