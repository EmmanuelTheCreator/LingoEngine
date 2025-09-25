using System;
using System.Collections.Generic;
using System.Linq;
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
    private readonly List<BlPacManEventSubscription> _modelSubscriptions = new();

    private Map? _map;
    private GameSettings? _currentGameSettings;
    private PacmanSettings? _currentPacmanSettings;
    private GhostSettings? _currentGhostSettings;
    private bool _isActorRegistered;
    private bool _initialized;

    private GameModel Model => _model ??= _globals.GameModel ?? throw new InvalidOperationException("GameModel was not initialised.");

    private BonusesModel BonusesModel => _bonusesModel ??= _globals.BonusesModel ?? throw new InvalidOperationException("BonusesModel was not initialised.");

    private BlPacManMapProvider MapProvider => _mapProvider ??= _globals.MapProvider ?? throw new InvalidOperationException("Pac-Man map provider was not initialised.");

    private BlPacManAssetContainer Assets => _globals.Assets;

    private BlPacManGameState State => _globals.State;

    private BlPacManBonusManager BonusManager => _globals.BonusManager;

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

        Assets.Reset();
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
        Assets.Reset();
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

        if (State.Win)
        {
            Model.Level++;
            ResetInternalState(resetScore: false);
            MakeLevel();
            State.Win = false;
            return;
        }

        if (State.GameOver)
        {
            Model.Level = 1;
            ResetInternalState(resetScore: true);
            MakeLevel();
            State.GameOver = false;
            _Player.SoundPlayIntro();
            return;
        }

        _Player.SoundPlayIntro();
        State.StartCountdown = Math.Max(State.StartCountdown, 1);
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

        State.ResetForNewLevel(settings, Model);
        BonusManager.Configure(settings);
        BonusManager.ResetForLevel();

        BonusesModel.Level = Model.Level;

        if (_map is not null)
        {
            var context = new BlPacManFieldContext(_map, this);
            _globals.CurrentFieldContext = context;
            _globals.ConsumableFieldMediator?.Publish(context);
        }

        if (Assets.PacMan is { } pacMan && _currentPacmanSettings is not null)
        {
            pacMan.Configure(this, _currentPacmanSettings);
        }

        if (_currentGhostSettings is not null)
        {
            foreach (var ghost in Assets.Ghosts)
            {
                ghost.Configure(this, _currentGhostSettings);
            }
        }

        if (Assets.Bonus is { } bonus)
        {
            bonus.Configure(settings);
            bonus.ResetForLife();
        }
    }

    /// <summary>
    /// Runs the central gameplay loop for each frame, coordinating timers, audio, and state checks.
    /// </summary>
    private void MainLoop()
    {
        if (_map is null)
        {
            return;
        }

        var state = State;

        if (state.Paused)
        {
            return;
        }

        if (state.StartCountdown <= 0)
        {
            Model.UpdateMode();
        }

        if (state.PauseFrames > 0)
        {
            state.PauseFrames--;
            return;
        }

        if (state.StartCountdown > 0)
        {
            state.StartCountdown--;
            if (state.StartCountdown == 0)
            {
                state.PauseFrames = Math.Max(state.PauseFrames, 60);
            }

            return;
        }

        if (state.Win)
        {
            StartLevel();
            return;
        }

        if (state.GameOver)
        {
            return;
        }

        if (state.PacManEatenPending)
        {
            if (state.Lives == 0)
            {
                return;
            }

            ResumeAfterPacManEaten();
            return;
        }

        BonusManager.Update(Model);
        UpdateGhostAudioState();
    }

    /// <summary>
    /// Toggles the attract music playback while updating the muted flag.
    /// </summary>
    private void ToggleSound()
    {
        var muted = !State.Muted;
        State.Muted = muted;

        if (muted)
        {
            _Player.SoundStopBack();
        }
        else
        {
            _Player.SoundPlayBack();
        }
    }

    /// <summary>
    /// Switches between paused and resumed gameplay states, invoking the appropriate handlers.
    /// </summary>
    private void TogglePause()
    {
        _globals.PauseBehavior?.TogglePause();
    }

    /// <summary>
    /// Clears transient counters and optionally resets the score when restarting gameplay.
    /// </summary>
    /// <param name="resetScore">Whether to wipe the player's score.</param>
    private void ResetInternalState(bool resetScore)
    {
        State.Reset();
        State.ApplyModelSnapshot(Model);
        _globals.PauseBehavior?.Reset();

        if (resetScore)
        {
            Model.ResetScore();
        }

        Model.ResetLives(Model.GetGameSettings().DefaultLives + 1);
        State.ApplyModelSnapshot(Model);
    }

    /// <summary>
    /// Subscribes to model change notifications so the behaviour can react to score, lives, and mode updates.
    /// </summary>
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

    /// <summary>
    /// Removes all subscriptions registered by <see cref="SubscribeModelEvents"/>.
    /// </summary>
    private void UnsubscribeModelEvents()
    {
        ReleaseModelSubscriptions();
    }

    /// <summary>
    /// Releases each model subscription to avoid leaking event handlers between sessions.
    /// </summary>
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

    /// <summary>
    /// Gets the map currently being played.
    /// </summary>
    public Map? CurrentMap => _map;

    /// <summary>
    /// Gets the backing model powering the gameplay loop.
    /// </summary>
    public GameModel GameModel => Model;

    /// <summary>
    /// Searches for a registered ghost by name.
    /// </summary>
    public BlPacManGhostBehavior? FindGhost(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return Assets.Ghosts.FirstOrDefault(g => string.Equals(g.GhostName, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Handles Pac-Man's death sequence and schedules the life reset.
    /// </summary>
    public void NotifyPacManEaten()
    {
        if (State.PacManEatenPending || State.GameOver)
        {
            return;
        }

        State.PacManEatenPending = true;
        State.ResetGhostChain();
        Assets.PacMan?.OnEatenByGhost();
        State.PauseFrames = Math.Max(State.PauseFrames, 40);
        State.SoundCooldown = 0;

        Assets.PacMan?.Hide();
        foreach (var ghost in Assets.Ghosts)
        {
            ghost.OnPacManEaten();
        }

        BonusManager.OnPacManEaten();
        State.BonusAppearCountdown = 250;
        State.BonusDestroyCountdown = 0;
        State.BonusLocked = false;

        var lives = Math.Max(0, Model.Lives - 1);
        Model.ResetLives(lives);
        State.ApplyModelSnapshot(Model);
    }

    /// <summary>
    /// Handles Pac-Man collecting the roaming bonus by awarding score and scheduling its removal.
    /// </summary>
    public void NotifyBonusEaten(BlPacManRoamingBonusBehavior bonus)
    {
        if (bonus is null)
        {
            throw new ArgumentNullException(nameof(bonus));
        }

        BonusManager.HandleCollected(Model);
    }

    /// <summary>
    /// Locks the bonus when it leaves the maze without being eaten.
    /// </summary>
    public void NotifyBonusExpired(BlPacManRoamingBonusBehavior bonus)
    {
        if (Assets.Bonus == bonus)
        {
            BonusManager.HandleExpired();
        }
    }

    /// <summary>
    /// Updates HUD bindings when the player's score changes.
    /// </summary>
    private void OnScoreChanged(int score)
    {
        State.Score = score;
    }

    /// <summary>
    /// Updates HUD bindings when the high-score value is bumped.
    /// </summary>
    private void OnHighScoreChanged(int score)
    {
        State.HighScore = score;
    }

    /// <summary>
    /// Reacts to life count updates and triggers the game-over state when necessary.
    /// </summary>
    private void OnLivesChanged(int lives)
    {
        State.Lives = lives;

        if (lives != 0)
        {
            return;
        }

        State.GameOver = true;
        _Player.SoundStopBack();
        Model.ResetScore();
    }

    /// <summary>
    /// Retains a hook for extra-life awards so dedicated HUD behaviours can react if needed.
    /// </summary>
    private void OnExtraLivesChanged(int _)
    {
        State.ApplyModelSnapshot(Model);
    }

    /// <summary>
    /// Applies the current global ghost mode to every active ghost.
    /// </summary>
    private void OnModeChanged(GhostMode? mode)
    {
        if (Assets.Ghosts.Count == 0)
        {
            return;
        }

        foreach (var ghost in Assets.Ghosts)
        {
            ghost.SetMode(mode);
        }
    }

    /// <summary>
    /// Keeps the bonus display model aligned with the active level.
    /// </summary>
    private void OnLevelChanged(int level)
    {
        BonusesModel.Level = level;
        State.Level = level;
    }

    /// <summary>
    /// Chooses which looping audio to play based on frightened and dead ghost states.
    /// </summary>
    private void UpdateGhostAudioState()
    {
        if (State.SoundCooldown > 0)
        {
            State.SoundCooldown--;
            return;
        }

        if (State.Muted || State.PacManEatenPending || State.Paused)
        {
            return;
        }

        if (Assets.Ghosts.Any(static g => g.IsDead))
        {
            _Player.SoundPlayDead();
        }
        else if (!Assets.Ghosts.Any(static g => g.IsFrightened))
        {
            _Player.SoundPlayBack();
        }

        State.SoundCooldown = 5;
    }

    /// <summary>
    /// Restores actors after Pac-Man loses a life and restarts the countdown.
    /// </summary>
    private void ResumeAfterPacManEaten()
    {
        if (!State.PacManEatenPending || State.Lives == 0)
        {
            return;
        }

        var pacMan = Assets.PacMan;
        pacMan?.ResetForLife();
        pacMan?.Show();

        foreach (var ghost in Assets.Ghosts)
        {
            ghost.ResetForLife();
            ghost.Show();
        }

        BonusManager.ResetAfterLifeLost();
        State.PacManEatenPending = false;
        State.StartCountdown = Math.Max(State.StartCountdown, 1);
        State.PauseFrames = Math.Max(State.PauseFrames, 60);
    }
}
