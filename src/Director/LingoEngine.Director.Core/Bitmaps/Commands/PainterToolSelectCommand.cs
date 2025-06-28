using LingoEngine.Commands;

namespace LingoEngine.Director.Core.Pictures.Commands
{
    /// <summary>
    /// Command to activate a specific painter tool by enum.
    /// </summary>
    public sealed record PainterToolSelectCommand(PainterToolType Tool) : ILingoCommand;
}
