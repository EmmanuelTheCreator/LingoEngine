using LingoEngine.Core;
using LingoEngine.Movies;

namespace LingoEngine.Commands.Handlers;

internal sealed class PlayMovieCommandHandler : ICommandHandler<PlayMovieCommand>
{
    private readonly ILingoPlayer _player;
    public PlayMovieCommandHandler(ILingoPlayer player) => _player = player;

    public bool CanExecute(PlayMovieCommand command) => _player.ActiveMovie is LingoMovie;

    public void Handle(PlayMovieCommand command)
    {
        if (_player.ActiveMovie is not LingoMovie movie) return;
        if (command.Frame.HasValue)
            movie.GoTo(command.Frame.Value);
        if (movie.IsPlaying)
            movie.Halt();
        else
            movie.Play();
    }
}
