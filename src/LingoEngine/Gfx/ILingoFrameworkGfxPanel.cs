using LingoEngine.Primitives;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Framework specific container allowing absolute positioning of child nodes.
    /// </summary>
    public interface ILingoFrameworkGfxPanel : ILingoFrameworkGfxNode
    {
        /// <summary>Adds a child node to the panel.</summary>
        void AddChild(ILingoFrameworkGfxNode child);
        void RemoveChild(ILingoFrameworkGfxNode child);
        IEnumerable<ILingoFrameworkGfxNode> GetChildren();

        /// <summary>Background color of the panel.</summary>
        LingoColor BackgroundColor { get; set; }
        /// <summary>Border color of the panel.</summary>
        LingoColor BorderColor { get; set; }
        /// <summary>Border width around the panel.</summary>
        float BorderWidth { get; set; }
    }
}
