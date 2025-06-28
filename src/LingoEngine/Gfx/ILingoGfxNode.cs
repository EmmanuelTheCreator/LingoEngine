using LingoEngine.Primitives;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Engine level interface for UI nodes that can be placed within containers.
    /// </summary>
    public interface ILingoGfxNode
    {
        string Name { get; set; }
        bool Visibility { get; set; }
        T Framework<T>() where T : ILingoFrameworkGfxLayoutNode;
        ILingoFrameworkGfxNode FrameworkObj { get; }
    }
    /// <summary>
    /// Engine level interface for UI nodes that can be placed within containers.
    /// </summary>
    public interface ILingoGfxLayoutNode : ILingoGfxNode
    {
        float X { get; set; }
        float Y { get; set; }
        float Width { get; set; }
        float Height { get; set; }
        LingoMargin Margin { get; set; }
    }
}
