using System;
using LingoEngine.Primitives;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific layout container with wrapping behaviour.
    /// </summary>
    public interface ILingoFrameworkWrapPanel : IDisposable
    {
        /// <summary>Orientation of child layout.</summary>
        LingoOrientation Orientation { get; set; }

        /// <summary>Margin around each child item.</summary>
        LingoMargin ItemMargin { get; set; }

        /// <summary>Margin around the container itself.</summary>
        LingoMargin Margin { get; set; }

        /// <summary>Adds a child panel to the container.</summary>
        void AddChild(ILingoFrameworkWrapPanel child);
    }
}
