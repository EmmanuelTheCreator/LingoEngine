using LingoEngine.Core;
using LingoEngine.Movies;

namespace LingoEngine.Commands.Handlers;

internal sealed class RewindMovieCommandHandler : ICommandHandler<RewindMovieCommand>
{
    private readonly ILingoPlayer _player;
    public RewindMovieCommandHandler(ILingoPlayer player) => _player = player;

    public bool CanExecute(RewindMovieCommand command) => _player.ActiveMovie is LingoMovie;

    public bool Handle(RewindMovieCommand command)
    {
        if (_player.ActiveMovie is LingoMovie movie)
            movie.GoTo(1);
        return true;
    }
}
