using LingoEngine.Commands;
using LingoEngine.Director.Core.Menus;

namespace LingoEngine.Director.Core.Commands
{
    public sealed record OpenWindowCommand(string WindowCode) : ILingoCommand;
    public sealed record CloseWindowCommand(string WindowCode) : ILingoCommand;
    public sealed record ExecuteShortCutCommand(DirectorShortCutMap ShortCut) : ILingoCommand;
}
