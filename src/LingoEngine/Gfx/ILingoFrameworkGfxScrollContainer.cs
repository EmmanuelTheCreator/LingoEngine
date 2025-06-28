
namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific scroll container implementation.
    /// </summary>
    public interface ILingoFrameworkGfxScrollContainer : ILingoFrameworkGfxLayoutNode
    {
        void AddChild(ILingoFrameworkGfxLayoutNode child);
        void RemoveChild(ILingoFrameworkGfxLayoutNode lingoFrameworkGfxNode);
        IEnumerable<ILingoFrameworkGfxLayoutNode> GetChildren();

        float ScrollHorizontal { get; set; }
        float ScrollVertical { get; set; }
        bool ClipContents { get; set; }
    }
}
