using LingoEngine.Commands;
using LingoEngine.Primitives;

namespace LingoEngine.Director.Core.Bitmaps.Commands
{
    public sealed record PainterChangeBackgroundColorCommand(LingoColor color) : ILingoCommand;
}
