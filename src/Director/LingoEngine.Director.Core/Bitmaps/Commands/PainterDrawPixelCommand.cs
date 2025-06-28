using LingoEngine.Commands;

namespace LingoEngine.Director.Core.Pictures.Commands
{
    /// <summary>
    /// Command to draw a single pixel at a given canvas coordinate with the currently selected color.
    /// </summary>
    public sealed record PainterDrawPixelCommand(int X, int Y) : ILingoCommand;
}
