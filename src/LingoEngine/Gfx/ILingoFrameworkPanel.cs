using LingoEngine.Primitives;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific container allowing absolute positioning of child nodes.
    /// </summary>
    public interface ILingoFrameworkPanel : ILingoFrameworkGfxNode
    {
        /// <summary>Adds a child node to the panel.</summary>
        void AddChild(ILingoFrameworkGfxNode child);
    }
}
