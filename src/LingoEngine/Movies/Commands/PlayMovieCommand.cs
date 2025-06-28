using LingoEngine.Commands;

namespace LingoEngine.Movies.Commands;

public sealed record PlayMovieCommand(int? Frame = null) : ILingoCommand;
