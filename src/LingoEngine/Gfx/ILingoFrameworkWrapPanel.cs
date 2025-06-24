using System;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific layout container with wrapping behaviour.
    /// </summary>
    public interface ILingoFrameworkWrapPanel : IDisposable
    {
        /// <summary>Orientation of child layout.</summary>
        LingoOrientation Orientation { get; set; }

        /// <summary>Adds a child panel to the container.</summary>
        void AddChild(ILingoFrameworkWrapPanel child);
    }
}
