using LingoEngine.Commands;
using LingoEngine.Primitives;

namespace LingoEngine.Director.Core.Pictures.Commands
{
    public sealed record PainterChangeBackgroundColorCommand(LingoColor color) : ILingoCommand;
}
