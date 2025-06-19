namespace LingoEngine.Commands;

public sealed record PlayMovieCommand(int? Frame = null) : ILingoCommand;
