using System;
using Blingo.PacMan.Core;
using Blingo.PacMan.Core.Game;
using BlingoEngine.Events;
using BlingoEngine.Inputs.Events;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Events;

namespace Blingo.PacMan.Core.Sprites.GeneralBehaviors;

internal sealed class BlPacManMenuStartBehavior : BlingoSpriteBehavior,
    IHasBeginSpriteEvent,
    IHasEndSpriteEvent,
    IHasKeyDownEvent,
    IHasMouseDownEvent
{
    private bool _consumed;
    private readonly GlobalVars _globals;

    public BlPacManMenuStartBehavior(IBlingoMovieEnvironment env, GlobalVars globals)
        : base(env)
    {
        _globals = globals ?? throw new ArgumentNullException(nameof(globals));
    }

    public void BeginSprite()
    {
        _consumed = false;
    }

    public void EndSprite()
    {
        _consumed = false;
    }

    public void KeyDown(BlingoKeyEvent key)
    {
        if (_consumed)
            return;

        StartGame();
    }

    public void MouseDown(BlingoMouseEvent mouse)
    {
        if (_consumed)
            return;

        StartGame();
    }

    private void StartGame()
    {
        _consumed = true;
        _Movie.GoTo(BlPacManProjectFactory.GameRunningLabel);
        //_globals.GameBehavior?.StartLevel();
    }
}
