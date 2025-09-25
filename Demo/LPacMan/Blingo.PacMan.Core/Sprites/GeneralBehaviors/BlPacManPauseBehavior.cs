using System;
using Blingo.PacMan.Core.Game;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Events;

namespace Blingo.PacMan.Core.Sprites.GeneralBehaviors;

/// <summary>
/// Handles toggling the pause overlay, muting/unmuting looping audio, and notifying
/// the game model whenever play is paused or resumed.
/// </summary>
internal sealed class BlPacManPauseBehavior : BlingoSpriteBehavior,
    IHasBeginSpriteEvent,
    IHasEndSpriteEvent
{
    private readonly GlobalVars _globals;

    public BlPacManPauseBehavior(IBlingoMovieEnvironment env, GlobalVars globals)
        : base(env)
    {
        _globals = globals ?? throw new ArgumentNullException(nameof(globals));
    }

    public void BeginSprite()
    {
        _globals.PauseBehavior = this;
        Me.Visibility = false;
    }

    public void EndSprite()
    {
        if (_globals.PauseBehavior == this)
        {
            _globals.PauseBehavior = null;
        }
    }

    /// <summary>
    /// Toggles between paused and resumed states.
    /// </summary>
    public void TogglePause()
    {
        if (_globals.IsPaused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    /// <summary>
    /// Forces the overlay into a resumed state without playing audio.
    /// </summary>
    public void Reset()
    {
        _globals.IsPaused = false;
        Me.Visibility = false;
    }

    private void Pause()
    {
        if (_globals.IsPaused)
        {
            return;
        }

        _globals.IsPaused = true;
        Me.Visibility = true;
        _globals.GameModel?.Pause();
        _Player.SoundStopBack();
    }

    private void Resume()
    {
        if (!_globals.IsPaused)
        {
            return;
        }

        _globals.IsPaused = false;
        Me.Visibility = false;
        _globals.GameModel?.Resume();
        if (!_globals.IsMuted)
        {
            _Player.SoundPlayBack();
        }
    }
}
