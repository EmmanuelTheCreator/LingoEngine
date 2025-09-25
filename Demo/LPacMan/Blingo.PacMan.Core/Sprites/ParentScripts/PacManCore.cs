using System;
using Blingo.PacMan.Core.Game;
using Blingo.PacMan.Core.Sprites.Behaviors;
using BlingoEngine.Core;
using BlingoEngine.Movies;

namespace Blingo.PacMan.Core.Sprites.ParentScripts;

internal sealed class PacManCore : BlingoParentScript
{
    private readonly GlobalVars _globals;

    public PacManCore(IBlingoMovieEnvironment env, GlobalVars globals)
        : base(env)
    {
        _globals = globals ?? throw new ArgumentNullException(nameof(globals));
    }

    public void Resetgame()
    {
        _globals.ClearGlobals();

        _Player.Sound.StopAll();

        var movie = _Movie;
        if (movie is null)
        {
            return;
        }

        movie.GoTo(PacManProjectFactory.MenuLabel);
        movie.SendAllSprites<PacManGameBehavior>(behavior => behavior.ResetToAttract());
    }
}
