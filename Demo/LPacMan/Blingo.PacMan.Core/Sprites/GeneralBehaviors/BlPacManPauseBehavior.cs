using System;
using Blingo.PacMan.Core.Game;
using BlingoEngine.Events;
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

    public bool IsPaused { get; private set; }

    public BlPacManPauseBehavior(IBlingoMovieEnvironment env, GlobalVars globals)
        : base(env)
    {
        _globals = globals;
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

    public void KeyDown(BlingoKeyEvent key)
    {
        // 'P' toggles pause mode.
        if (key.KeyPressed(80))
            TogglePause();
    }


    /// <summary>
    /// Toggles between paused and resumed states.
    /// </summary>
    public void TogglePause()
    {
        if (IsPaused)
            Resume();
        else
            Pause();
    }

    /// <summary>
    /// Forces the overlay into a resumed state without playing audio.
    /// </summary>
    public void Reset()
    {
        IsPaused = false;
        Me.Visibility = false;
    }

    private void Pause()
    {
        if (_globals.State.IsPaused)
            return;

        IsPaused = true;
        Me.Visibility = true;
        _Player.SoundStopBack();
        _globals.GhostManager.Pause();
    }

    private void Resume()
    {
        if (!_globals.State.IsPaused)
            return;

        IsPaused = false;
        Me.Visibility = false;
        if (!_globals.State.IsMuted)
            _Player.SoundPlayBack();
        _globals.GhostManager.Resume();
    }
}
