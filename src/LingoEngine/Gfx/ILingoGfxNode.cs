using LingoEngine.Primitives;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Engine level interface for UI nodes that can be placed within containers.
    /// </summary>
    public interface ILingoGfxNode
    {
        float X { get; set; }
        float Y { get; set; }
        float Width { get; set; }
        float Height { get; set; }
        bool Visibility { get; set; }
        LingoMargin Margin { get; set; }
        T Framework<T>() where T : ILingoFrameworkGfxNode;
    }
}
