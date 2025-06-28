using LingoEngine.Primitives;

namespace LingoEngine.Gfx
{
    /// <summary>
    /// Basic framework object that can be positioned and sized on screen.
    /// </summary>
    public interface ILingoFrameworkGfxNode : System.IDisposable
    {
        /// <summary>Name of the node.</summary>
        string Name { get; set; }
        float X { get; set; }
        float Y { get; set; }
        float Width { get; set; }
        float Height { get; set; }
        bool Visibility { get; set; }
        /// <summary>Margin around the node.</summary>
        LingoMargin Margin { get; set; }
    }
}
