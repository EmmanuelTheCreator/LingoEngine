using System;
using System.Collections.Generic;
using Blingo.PacMan.Core;
using Blingo.PacMan.Core.Datas;
using Blingo.PacMan.Core.Game;
using Blingo.PacMan.Core.Models;
using BlingoEngine.Events;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Events;
using BlingoEngine.Inputs.Events;

namespace Blingo.PacMan.Core.Sprites.GameObjectBehaviors;

/// <summary>
/// Coordinates the overall Pac-Man gameplay state, wiring the models and sprite behaviours into the runtime.
/// It keeps track of the game's meta state (start screen, pause, win, game over) and forwards mode updates to the model.
/// </summary>
internal sealed class BlPacManGameBehavior : BlingoSpriteBehavior,
    IHasBeginSpriteEvent,
    IHasEndSpriteEvent,
    IHasExitFrameEvent,
    IHasKeyDownEvent,
    IHasStepFrameEvent
{
    private readonly GlobalVars _globals;
    private GameModel? _model;
    private BonusesModel? _bonusesModel;
    private BlPacManMapProvider? _mapProvider;
    private readonly List<BlPacManGhostBehavior> _ghosts = new();
    private readonly List<BlPacManConsumableComponent> _consumables = new();
    private readonly List<BlPacManEventSubscription> _modelSubscriptions = new();

    private Map? _map;
    private BlPacManActorBehavior? _pacMan;
    private BlPacManRoamingBonusBehavior? _bonus;
    private GameSettings? _currentGameSettings;
    private PacmanSettings? _currentPacmanSettings;
    private GhostSettings? _currentGhostSettings;
    private bool _isActorRegistered;

    private int _pauseFrames;
    private int _startCountdown;
    private int _soundBackCooldown;
    private int _remainingConsumables;

    private bool _initialized;
    private bool _muted;
    private bool _paused;
    private bool _win;
    private bool _gameOver;

    private GameModel Model => _model ??= _globals.GameModel ?? throw new InvalidOperationException("GameModel was not initialised.");

    private BonusesModel BonusesModel => _bonusesModel ??= _globals.BonusesModel ?? throw new InvalidOperationException("BonusesModel was not initialised.");

    private BlPacManMapProvider MapProvider => _mapProvider ??= _globals.MapProvider ?? throw new InvalidOperationException("Pac-Man map provider was not initialised.");

    public BlPacManGameBehavior(IBlingoMovieEnvironment env, GlobalVars globals)
        : base(env)
    {
        _globals = globals ?? throw new ArgumentNullException(nameof(globals));
    }

    /// <inheritdoc />
    public void BeginSprite()
    {
        _globals.GameBehavior = this;
        if (!_isActorRegistered)
        {
            Me.AddActor(this);
            _isActorRegistered = true;
        }

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
            _globals.GameBehavior = null;
            return;
        }

        _globals.GameBehavior = null;
        _globals.CurrentFieldContext = null;
        _globals.CurrentGameSettings = null;
        _globals.CurrentPacmanSettings = null;
        _globals.CurrentGhostSettings = null;
        _currentGameSettings = null;
        _currentPacmanSettings = null;
        _currentGhostSettings = null;
        UnsubscribeModelEvents();
        _consumables.Clear();
        _ghosts.Clear();
        _initialized = false;
    }

    /// <inheritdoc />
    public void ExitFrame()
    {
        // ExitFrame is unused; gameplay updates run through StepFrame via the actor list.
    }

    public void StepFrame()
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

        // 'S' toggles sound for the attract mode.
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

        _Movie.GoTo(BlPacManProjectFactory.GameRunningLabel);

        if (_win)
        {
            Model.Level++;
            ResetInternalState(resetScore: false);
            MakeLevel();
            _win = false;
            return;
        }

        if (_gameOver)
        {
            Model.Level = 1;
            ResetInternalState(resetScore: true);
            MakeLevel();
            _gameOver = false;
            _Player.SoundPlayIntro();
            return;
        }

        _Player.SoundPlayIntro();
        _startCountdown = Math.Max(_startCountdown, 1);
    }

    /// <summary>
    /// Resets the attract screen by clearing transient counters and rebuilding the current map.
    /// This ensures the next frame behaves like a fresh boot.
    /// </summary>
    public void ResetToAttract()
    {
        if (!_initialized)
        {
            _Movie.GoTo(BlPacManProjectFactory.MenuLabel);
            return;
        }

        Model.Level = 1;
        ResetInternalState(resetScore: true);
        MakeLevel();
        _Movie.GoTo(BlPacManProjectFactory.MenuLabel);
    }

    /// <summary>
    /// Reads the level settings from the model and re-initialises counters used by the gameplay loop.
    /// </summary>
    private void MakeLevel()
    {
        var settings = Model.GetGameSettings();
        _map = new Map(settings.MapLayout);
        MapProvider.CurrentMap = _map;

        _currentGameSettings = settings;
        _currentPacmanSettings = Model.GetPacmanSettings();
        _currentGhostSettings = Model.GetGhostSettings();

        _globals.CurrentGameSettings = _currentGameSettings;
        _globals.CurrentPacmanSettings = _currentPacmanSettings;
        _globals.CurrentGhostSettings = _currentGhostSettings;

        _pauseFrames = 80;
        _soundBackCooldown = 0;
        _remainingConsumables = 0;
        _consumables.Clear();

        BonusesModel.Level = Model.Level;

        if (_map is not null)
        {
            var context = new BlPacManFieldContext(_map, this);
            _globals.CurrentFieldContext = context;
            _globals.ConsumableFieldMediator?.Publish(context);
        }

        if (_pacMan is not null && _currentPacmanSettings is not null)
        {
            _pacMan.Configure(this, _currentPacmanSettings);
        }

        foreach (var ghost in _ghosts)
        {
            if (_currentGhostSettings is not null)
            {
                ghost.Configure(this, _currentGhostSettings);
            }
        }

        _bonus?.Configure(this, settings);
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
            Model.UpdateMode();
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
            _Player.SoundPlayBack();
        }
        else
        {
            _Player.SoundStopBack();
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
        Model.Pause();
        _Player.SoundStopBack();
    }

    private void Resume()
    {
        Model.Resume();
        if (!_muted)
        {
            _Player.SoundPlayBack();
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
        _remainingConsumables = 0;
        _consumables.Clear();

        if (resetScore)
        {
            Model.ResetScore();
        }

        Model.ResetLives(Model.GetGameSettings().DefaultLives + 1);
    }

    private void SubscribeModelEvents()
    {
        ReleaseModelSubscriptions();
        _modelSubscriptions.Add(Model.SubscribeScoreChanged(OnScoreChanged));
        _modelSubscriptions.Add(Model.SubscribeHighScoreChanged(OnHighScoreChanged));
        _modelSubscriptions.Add(Model.SubscribeLivesChanged(OnLivesChanged));
        _modelSubscriptions.Add(Model.SubscribeExtraLivesChanged(OnExtraLivesChanged));
        _modelSubscriptions.Add(Model.SubscribeModeChanged(OnModeChanged));
        _modelSubscriptions.Add(Model.SubscribeLevelChanged(OnLevelChanged));
    }

    private void UnsubscribeModelEvents()
    {
        ReleaseModelSubscriptions();
    }

    private void ReleaseModelSubscriptions()
    {
        if (_modelSubscriptions.Count == 0)
        {
            return;
        }

        foreach (var subscription in _modelSubscriptions)
        {
            subscription.Release();
        }

        _modelSubscriptions.Clear();
    }

    public Map? CurrentMap => _map;

    public GameModel GameModel => Model;

    public void RegisterConsumable(BlPacManConsumableComponent consumable)
    {
        if (consumable is null)
        {
            throw new ArgumentNullException(nameof(consumable));
        }

        _consumables.Add(consumable);
        _remainingConsumables++;
    }

    public void NotifyConsumableEaten(BlPacManConsumableComponent consumable)
    {
        if (consumable is null)
        {
            return;
        }

        if (_remainingConsumables > 0)
        {
            _remainingConsumables--;
        }

        switch (consumable.Type)
        {
            case BlPacManConsumableType.Pellet:
                _Player.SoundPlayDot();
                break;
            case BlPacManConsumableType.PowerPill:
                _Player.SoundPlayEat();
                foreach (var ghost in _ghosts)
                {
                    ghost.SetMode(GhostMode.Frightened);
                }
                break;
            case BlPacManConsumableType.Bonus:
                _Player.SoundPlayBonus();
                break;
        }

        Model.AddScore(consumable.ScoreValue);

        if (_remainingConsumables == 0)
        {
            _win = true;
        }
    }

    public void RegisterPacMan(BlPacManActorBehavior pacMan)
    {
        _pacMan = pacMan ?? throw new ArgumentNullException(nameof(pacMan));
        if (_currentPacmanSettings is not null)
        {
            _pacMan.Configure(this, _currentPacmanSettings);
        }
    }

    public void RegisterGhost(BlPacManGhostBehavior ghost)
    {
        if (ghost is null)
        {
            throw new ArgumentNullException(nameof(ghost));
        }

        if (!_ghosts.Contains(ghost))
        {
            _ghosts.Add(ghost);
        }

        if (_currentGhostSettings is not null)
        {
            ghost.Configure(this, _currentGhostSettings);
        }
    }

    public void RegisterBonus(BlPacManRoamingBonusBehavior bonus)
    {
        _bonus = bonus ?? throw new ArgumentNullException(nameof(bonus));
        if (_currentGameSettings is not null)
        {
            _bonus.Configure(this, _currentGameSettings);
        }
    }

    public void UnregisterConsumable(BlPacManConsumableComponent consumable)
    {
        if (consumable is null)
        {
            return;
        }

        _consumables.Remove(consumable);
    }

    public void UnregisterPacMan(BlPacManActorBehavior pacMan)
    {
        if (_pacMan == pacMan)
        {
            _pacMan = null;
        }
    }

    public void UnregisterGhost(BlPacManGhostBehavior ghost)
    {
        _ghosts.Remove(ghost);
    }

    public void UnregisterBonus(BlPacManRoamingBonusBehavior bonus)
    {
        if (_bonus == bonus)
        {
            _bonus = null;
        }
    }

    private void OnScoreChanged(int score)
    {
        // Score display is handled by dedicated HUD behaviours. Keep the hook for future updates.
    }

    private void OnHighScoreChanged(int score)
    {
        // Update hooks for score and HUD elements will be wired up once the UI sprites are in place.
    }

    private void OnLivesChanged(int lives)
    {
        if (lives == 0)
        {
            _gameOver = true;
            _Player.SoundStopBack();
            Model.ResetScore();
        }
    }

    private void OnExtraLivesChanged(int _)
    {
        _Player.SoundPlayLife();
    }

    private void OnModeChanged(GhostMode? mode)
    {
        if (_ghosts.Count == 0)
        {
            return;
        }

        foreach (var ghost in _ghosts)
        {
            ghost.SetMode(mode);
        }
    }

    private void OnLevelChanged(int level)
    {
        BonusesModel.Level = level;
    }
}
