using LingoEngine.Commands;
using LingoEngine.Director.Core.Bitmaps;

namespace LingoEngine.Director.Core.Stages.Commands;

public sealed record StageToolSelectCommand(StageTool Tool) : ILingoCommand;
