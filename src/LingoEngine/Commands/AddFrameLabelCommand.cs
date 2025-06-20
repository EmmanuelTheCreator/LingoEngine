namespace LingoEngine.Commands;

public sealed record AddFrameLabelCommand(int FrameNumber, string Name) : ILingoCommand;
