using LingoEngine.Commands;

namespace LingoEngine.Movies.Commands;

public sealed record StepFrameCommand(int Offset) : ILingoCommand;
