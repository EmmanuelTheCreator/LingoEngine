using LingoEngine.Primitives;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific layout container with wrapping behaviour.
    /// </summary>
    public interface ILingoFrameworkGfxWrapPanel : ILingoFrameworkGfxNode
    {
        /// <summary>Orientation of child layout.</summary>
        LingoOrientation Orientation { get; set; }

        /// <summary>Margin around each child item.</summary>
        LingoMargin ItemMargin { get; set; }

        /// <summary>Adds a child node to the container.</summary>
        void AddChild(ILingoFrameworkGfxNode child);
        void RemoveChild(ILingoFrameworkGfxNode child);
        IEnumerable<ILingoFrameworkGfxNode> GetChildren();
        ILingoFrameworkGfxNode? GetChild(int index);
    }
}
