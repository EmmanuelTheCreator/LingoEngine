using LingoEngine.Primitives;

namespace LingoEngine.Gfx
{
    public interface ILingoFrameworkGfxNode : IDisposable
    {
        /// <summary>Name of the node.</summary>
        string Name { get; set; }
        bool Visibility { get; set; }
        float Width { get; set; }
        float Height { get; set; }

        /// <summary>Margin around the node.</summary>
        LingoMargin Margin { get; set; }
        object FrameworkNode { get; }
    }
    /// <summary>
    /// Basic framework object that can be positioned and sized on screen.
    /// </summary>
    public interface ILingoFrameworkGfxLayoutNode : ILingoFrameworkGfxNode
    {
        
        float X { get; set; }
        float Y { get; set; }
     
    }
    
}
