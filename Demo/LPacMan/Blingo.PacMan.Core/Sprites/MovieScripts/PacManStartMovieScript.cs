using System;
using Blingo.PacMan.Core.Game;
using Blingo.PacMan.Core.Models;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;

namespace Blingo.PacMan.Core.Sprites.MovieScripts;

internal sealed class PacManStartMovieScript : BlingoMovieScript, IHasStartMovieEvent, IHasStopMovieEvent
{
    private readonly GlobalVars _globals;

    public PacManStartMovieScript(IBlingoMovieEnvironment env, GlobalVars globals)
        : base(env)
    {
        _globals = globals ?? throw new ArgumentNullException(nameof(globals));
    }

    public void StartMovie()
    {
        EnsureGlobals();
        _globals.GameModel?.Resume();
    }

    public void StopMovie()
    {
        _globals.GameModel?.Pause();
    }

    private void EnsureGlobals()
    {
        if (_globals.MapProvider is null)
        {
            _globals.MapProvider = new PacManMapProvider();
        }

        if (_globals.ConsumableFieldMediator is null)
        {
            _globals.ConsumableFieldMediator = new PacManEventMediator<PacManFieldContext>();
        }

        if (_globals.BonusesModel is null)
        {
            _globals.BonusesModel = new BonusesModel();
        }

        if (_globals.GameModel is null)
        {
            var repository = _env.GetRequiredService<GameModelRepository>();
            _globals.GameModel = new GameModel(repository);
        }
    }
}
