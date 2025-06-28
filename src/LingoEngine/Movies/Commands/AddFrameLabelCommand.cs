using LingoEngine.Commands;

namespace LingoEngine.Movies.Commands;

public sealed record AddFrameLabelCommand(int FrameNumber, string Name) : ILingoCommand;
