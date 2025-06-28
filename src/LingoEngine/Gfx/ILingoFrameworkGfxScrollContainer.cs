
namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific scroll container implementation.
    /// </summary>
    public interface ILingoFrameworkGfxScrollContainer : ILingoFrameworkGfxNode
    {
        void AddChild(ILingoFrameworkGfxNode child);
        void RemoveChild(ILingoFrameworkGfxNode lingoFrameworkGfxNode);
        IEnumerable<ILingoFrameworkGfxNode> GetChildren();

        float ScrollHorizontal { get; set; }
        float ScrollVertical { get; set; }
        bool ClipContents { get; set; }
    }
}
