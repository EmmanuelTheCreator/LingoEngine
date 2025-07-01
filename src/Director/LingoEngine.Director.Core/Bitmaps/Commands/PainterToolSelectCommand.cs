using LingoEngine.Commands;
using LingoEngine.Director.Core.Bitmaps;

namespace LingoEngine.Director.Core.Bitmaps.Commands
{
    /// <summary>
    /// Command to activate a specific painter tool by enum.
    /// </summary>
    public sealed record PainterToolSelectCommand(PainterToolType Tool) : ILingoCommand;
    
}
