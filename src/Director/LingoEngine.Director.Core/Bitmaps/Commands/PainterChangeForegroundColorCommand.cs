using LingoEngine.Commands;
using LingoEngine.Primitives;

namespace LingoEngine.Director.Core.Pictures.Commands
{
    public sealed record PainterChangeForegroundColorCommand(LingoColor color) : ILingoCommand;
}
