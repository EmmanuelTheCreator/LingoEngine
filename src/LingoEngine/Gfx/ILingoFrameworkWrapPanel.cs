using LingoEngine.Primitives;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific layout container with wrapping behaviour.
    /// </summary>
    public interface ILingoFrameworkWrapPanel : ILingoFrameworkGfxNode
    {
        /// <summary>Orientation of child layout.</summary>
        LingoOrientation Orientation { get; set; }

        /// <summary>Margin around each child item.</summary>
        LingoMargin ItemMargin { get; set; }

        /// <summary>Margin around the container itself.</summary>
        LingoMargin Margin { get; set; }

        /// <summary>Adds a child node to the container.</summary>
        void AddChild(ILingoFrameworkGfxNode child);
    }
}
