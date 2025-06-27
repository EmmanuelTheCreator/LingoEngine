using LingoEngine.Commands;

namespace LingoEngine.Movies.Commands;

public sealed record SetFrameLabelCommand(int FrameNumber, string? Name) : ILingoCommand;
