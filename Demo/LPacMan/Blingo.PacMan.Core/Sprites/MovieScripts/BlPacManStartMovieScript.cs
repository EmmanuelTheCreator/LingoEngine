using Blingo.PacMan.Core.Engine;
using Blingo.PacMan.Core.Game;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;

namespace Blingo.PacMan.Core.Sprites.MovieScripts;

internal sealed class BlPacManStartMovieScript : BlingoMovieScript, IHasStartMovieEvent, IHasStopMovieEvent
{
    private readonly GlobalVars _globals;
    private readonly BlPacManRepository _repository;

    public BlPacManStartMovieScript(IBlingoMovieEnvironment env, GlobalVars globals, BlPacManRepository repository)
        : base(env)
    {
        _globals = globals;
        _repository = repository;

    }

    public void StartMovie()
    {
        //_globals.GameModel?.Resume();
    }

    public void StopMovie()
    {
       // _globals.GameModel?.Pause();
    }
   
}
