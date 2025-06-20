namespace LingoEngine.Commands;

public sealed record UpdateFrameLabelCommand(int PreviousFrame, int NewFrame, string Name) : ILingoCommand;
