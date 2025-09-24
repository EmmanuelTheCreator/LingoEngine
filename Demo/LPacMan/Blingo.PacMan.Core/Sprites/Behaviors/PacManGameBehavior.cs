using System;
using Blingo.PacMan.Core.Models;
using BlingoEngine.Events;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Events;
using BlingoEngine.Inputs.Events;

namespace Blingo.PacMan.Core.Sprites.Behaviors;

/// <summary>
/// Rough C# port of the <c>JsPacman</c> controller. The original implementation orchestrated
/// DOM elements; this behaviour adapts the flow to the Blingo runtime by relying on the
/// timeline and model abstractions. It keeps track of the game's meta state (start screen,
/// pause, win, game over) and forwards mode updates to the underlying <see cref="IGameModel"/>.
/// </summary>
public sealed class PacManGameBehavior : BlingoSpriteBehavior,
    IHasBeginSpriteEvent,
    IHasEndSpriteEvent,
    IHasExitFrameEvent,
    IHasKeyDownEvent
{
    private readonly IGameModel _model;
    private readonly IBonusesModel _bonusesModel;

    private Map? _map;

    private int _pauseFrames;
    private int _startCountdown;
    private int _soundBackCooldown;

    private bool _initialized;
    private bool _muted;
    private bool _paused;
    private bool _win;
    private bool _gameOver;

    /// <summary>
    /// Creates a new instance of the behaviour that coordinates overall game flow.
    /// </summary>
    public PacManGameBehavior(IBlingoMovieEnvironment env, IGameModel model, IBonusesModel bonusesModel)
        : base(env)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _bonusesModel = bonusesModel ?? throw new ArgumentNullException(nameof(bonusesModel));
    }

    /// <inheritdoc />
    public void BeginSprite()
    {
        if (_initialized)
        {
            return;
        }

        SubscribeModelEvents();
        ResetInternalState(resetScore: false);
        MakeLevel();

        _initialized = true;
    }

    /// <inheritdoc />
    public void EndSprite()
    {
        if (!_initialized)
        {
            return;
        }

        UnsubscribeModelEvents();
        _initialized = false;
    }

    /// <inheritdoc />
    public void ExitFrame()
    {
        if (!_initialized)
        {
            return;
        }

        MainLoop();
    }

    /// <inheritdoc />
    public void KeyDown(BlingoKeyEvent key)
    {
        if (key is null)
        {
            return;
        }

        // 'S' toggles sound as in the JavaScript implementation.
        if (key.KeyPressed(83))
        {
            ToggleSound();
        }
        // 'P' toggles pause mode.
        else if (key.KeyPressed(80))
        {
            TogglePause();
        }
    }

    /// <summary>
    /// Called by the start button sprite to kick off the current level.
    /// </summary>
    public void StartLevel()
    {
        if (!_initialized)
        {
            return;
        }

        if (_win)
        {
            _model.Level++;
            ResetInternalState(resetScore: false);
            MakeLevel();
            _win = false;
            return;
        }

        if (_gameOver)
        {
            _model.Level = 1;
            ResetInternalState(resetScore: true);
            MakeLevel();
            _gameOver = false;
            PacManSounds.SoundPlayIntro(_Player);
            return;
        }

        PacManSounds.SoundPlayIntro(_Player);
        _startCountdown = Math.Max(_startCountdown, 1);
    }

    /// <summary>
    /// Restores the attract screen by mirroring the JavaScript controller's reset routine.
    /// It resets the level, clears transient counters and rebuilds the current map so the
    /// next frame tick behaves like a fresh boot.
    /// </summary>
    public void ResetToAttract()
    {
        if (!_initialized)
        {
            _Movie.GoTo(PacManProjectFactory.IntroLabel);
            return;
        }

        _model.Level = 1;
        ResetInternalState(resetScore: true);
        MakeLevel();
        _Movie.GoTo(PacManProjectFactory.IntroLabel);
    }

    /// <summary>
    /// Equivalent of the JavaScript <c>makeLevel</c> routine. It reads the level settings from
    /// the model and re-initialises counters used by the gameplay loop. Actual sprite spawning
    /// will be implemented as the remaining actors are ported.
    /// </summary>
    private void MakeLevel()
    {
        var settings = _model.GetGameSettings();
        _map = new Map(settings.MapLayout);

        _pauseFrames = 80;
        _soundBackCooldown = 0;

        _bonusesModel.Level = _model.Level;
    }

    private void MainLoop()
    {
        if (_map is null)
        {
            return;
        }

        if (_paused)
        {
            return;
        }

        if (_startCountdown <= 0)
        {
            _model.UpdateMode();
        }

        if (_pauseFrames > 0)
        {
            _pauseFrames--;
            return;
        }

        if (_startCountdown > 0)
        {
            _startCountdown--;
            if (_startCountdown == 0)
            {
                _pauseFrames = 60;
            }

            return;
        }

        if (_win)
        {
            StartLevel();
            return;
        }

        if (_gameOver)
        {
            return;
        }

        if (_soundBackCooldown > 0)
        {
            _soundBackCooldown--;
        }
    }

    private void ToggleSound()
    {
        if (_muted)
        {
            PacManSounds.SoundPlayBack(_Player);
        }
        else
        {
            PacManSounds.SoundStopBack(_Player);
        }

        _muted = !_muted;
    }

    private void TogglePause()
    {
        _paused = !_paused;
        if (_paused)
        {
            Pause();
        }
        else
        {
            Resume();
        }
    }

    private void Pause()
    {
        _model.Pause();
        PacManSounds.SoundStopBack(_Player);
    }

    private void Resume()
    {
        _model.Resume();
        if (!_muted)
        {
            PacManSounds.SoundPlayBack(_Player);
        }
    }

    private void ResetInternalState(bool resetScore)
    {
        _pauseFrames = 0;
        _soundBackCooldown = 0;
        _startCountdown = 2;
        _win = false;
        _gameOver = false;
        _paused = false;

        if (resetScore)
        {
            _model.ResetScore();
        }

        _model.ResetLives(_model.GetGameSettings().DefaultLives + 1);
    }

    private void SubscribeModelEvents()
    {
        _model.ScoreChanged += OnScoreChanged;
        _model.HighScoreChanged += OnHighScoreChanged;
        _model.LivesChanged += OnLivesChanged;
        _model.ExtraLivesChanged += OnExtraLivesChanged;
        _model.ModeChanged += OnModeChanged;
        _model.LevelChanged += OnLevelChanged;
    }

    private void UnsubscribeModelEvents()
    {
        _model.ScoreChanged -= OnScoreChanged;
        _model.HighScoreChanged -= OnHighScoreChanged;
        _model.LivesChanged -= OnLivesChanged;
        _model.ExtraLivesChanged -= OnExtraLivesChanged;
        _model.ModeChanged -= OnModeChanged;
        _model.LevelChanged -= OnLevelChanged;
    }

    private void OnScoreChanged(int score)
    {
        // Score display is handled by dedicated HUD behaviours. We keep the hook for parity with JS code.
    }

    private void OnHighScoreChanged(int score)
    {
        // Mirrors the JavaScript change handler. Rendering will be wired up once the UI sprites are ported.
    }

    private void OnLivesChanged(int lives)
    {
        if (lives == 0)
        {
            _gameOver = true;
            PacManSounds.SoundStopBack(_Player);
            _model.ResetScore();
        }
    }

    private void OnExtraLivesChanged(int _)
    {
        PacManSounds.SoundPlayLife(_Player);
    }

    private void OnModeChanged(GhostMode? mode)
    {
        // Forward mode changes to any ghosts once they are fully ported.
    }

    private void OnLevelChanged(int level)
    {
        _bonusesModel.Level = level;
    }
}
