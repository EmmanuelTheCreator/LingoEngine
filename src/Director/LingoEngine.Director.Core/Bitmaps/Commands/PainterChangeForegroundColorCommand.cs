using LingoEngine.Commands;
using LingoEngine.Primitives;

namespace LingoEngine.Director.Core.Bitmaps.Commands
{
    public sealed record PainterChangeForegroundColorCommand(LingoColor color) : ILingoCommand;
}
