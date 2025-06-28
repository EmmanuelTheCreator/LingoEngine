using LingoEngine.Commands;

namespace LingoEngine.Director.Core.Windowing.Commands
{
    public sealed record OpenWindowCommand(string WindowCode) : ILingoCommand;
}
