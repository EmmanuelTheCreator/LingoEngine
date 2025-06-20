namespace LingoEngine.Commands;

public sealed record SetScoreLabelCommand(int FrameNumber, string? Name) : ILingoCommand;
