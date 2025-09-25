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
    private readonly List<BlPacManEventSubscription> _assetSubscriptions = new();

    private Map? _map;
    private GameSettings? _currentGameSettings;
    private PacmanSettings? _currentPacmanSettings;
    private GhostSettings? _currentGhostSettings;
    private bool _isActorRegistered;

    private int _pauseFrames;
    private int _startCountdown;
    private int _soundBackCooldown;
    private int _remainingConsumables;

    private int _bonusAppearCountdown;
    private int _bonusDestroyCountdown;
    private bool _bonusLocked;
    private bool _pacManEatenPending;
    private int _ghostChainIndex;

    private bool _initialized;
    private bool _win;
    private bool _gameOver;

    private static readonly int[] GhostScoreChain = { 200, 400, 800, 1_600 };

    private GameModel Model => _model ??= _globals.GameModel ?? throw new InvalidOperationException("GameModel was not initialised.");

    private BonusesModel BonusesModel => _bonusesModel ??= _globals.BonusesModel ?? throw new InvalidOperationException("BonusesModel was not initialised.");

    private BlPacManMapProvider MapProvider => _mapProvider ??= _globals.MapProvider ?? throw new InvalidOperationException("Pac-Man map provider was not initialised.");

    private BlPacManAssetContainer Assets => _globals.Assets;

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
        SubscribeAssetEvents();
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
        ReleaseAssetSubscriptions();
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
            bonus.Configure(this, settings);
            bonus.ResetForLife();
        }

        Assets.BroadcastPacManPosition();
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

        if (IsPaused)
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

        UpdateBonusLifecycle();

        if (_pacManEatenPending)
        {
            if (Model.Lives == 0)
            {
                return;
            }

            ResumeAfterPacManEaten();
            return;
        }

        UpdateGhostAudioState();
    }

    /// <summary>
    /// Toggles the attract music playback while updating the muted flag.
    /// </summary>
    private void ToggleSound()
    {
        var muted = !IsMuted;
        _globals.IsMuted = muted;

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
        _pauseFrames = 0;
        _soundBackCooldown = 0;
        _startCountdown = 2;
        _win = false;
        _gameOver = false;
        _globals.IsPaused = false;
        _remainingConsumables = 0;
        _bonusAppearCountdown = 500;
        _bonusDestroyCountdown = 0;
        _bonusLocked = false;
        _pacManEatenPending = false;
        _ghostChainIndex = 0;
        _globals.PauseBehavior?.Reset();

        if (resetScore)
        {
            Model.ResetScore();
        }

        Model.ResetLives(Model.GetGameSettings().DefaultLives + 1);
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
    /// Subscribes to asset container events so the coordinator can react to consumable updates.
    /// </summary>
    private void SubscribeAssetEvents()
    {
        ReleaseAssetSubscriptions();
        _assetSubscriptions.Add(Assets.SubscribeConsumableRegistered(OnConsumableRegistered));
        _assetSubscriptions.Add(Assets.SubscribeConsumableEaten(OnConsumableEaten));
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
    /// Releases the event subscriptions registered against the asset container.
    /// </summary>
    private void ReleaseAssetSubscriptions()
    {
        if (_assetSubscriptions.Count == 0)
        {
            return;
        }

        foreach (var subscription in _assetSubscriptions)
        {
            subscription.Release();
        }

        _assetSubscriptions.Clear();
    }

    /// <summary>
    /// Gets the map currently being played.
    /// </summary>
    public Map? CurrentMap => _map;

    /// <summary>
    /// Gets the backing model powering the gameplay loop.
    /// </summary>
    public GameModel GameModel => Model;

    private bool IsMuted => _globals.IsMuted;

    private bool IsPaused => _globals.IsPaused;

    /// <summary>
    /// Gets a value indicating whether gameplay updates should be skipped for this frame.
    /// </summary>
    public bool IsGameplayFrozen => IsPaused || _pauseFrames > 0 || _startCountdown > 0 || _win || _gameOver || _pacManEatenPending;

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
    /// Awards score and handles audio when Pac-Man eats a ghost.
    /// </summary>
    public void NotifyGhostEaten(BlPacManGhostBehavior ghost)
    {
        if (ghost is null)
        {
            throw new ArgumentNullException(nameof(ghost));
        }

        var chainIndex = Math.Clamp(_ghostChainIndex, 0, GhostScoreChain.Length - 1);
        var score = GhostScoreChain[chainIndex];
        if (_ghostChainIndex < GhostScoreChain.Length - 1)
        {
            _ghostChainIndex++;
        }

        Model.AddScore(score);
        _soundBackCooldown = 5;
        _pauseFrames = Math.Max(_pauseFrames, 15);
        ghost.OnEaten(score);
    }

    /// <summary>
    /// Handles Pac-Man's death sequence and schedules the life reset.
    /// </summary>
    public void NotifyPacManEaten()
    {
        if (_pacManEatenPending || _gameOver)
        {
            return;
        }

        _pacManEatenPending = true;
        _ghostChainIndex = 0;
        Assets.PacMan?.OnEatenByGhost();
        _pauseFrames = Math.Max(_pauseFrames, 40);
        _soundBackCooldown = 0;

        Assets.PacMan?.Hide();
        foreach (var ghost in Assets.Ghosts)
        {
            ghost.OnPacManEaten();
        }

        Assets.Bonus?.OnPacManEaten();
        _bonusAppearCountdown = 250;
        _bonusDestroyCountdown = 0;
        _bonusLocked = false;

        var lives = Math.Max(0, Model.Lives - 1);
        Model.ResetLives(lives);
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

        if (_bonusLocked)
        {
            return;
        }

        var score = _currentGameSettings?.BonusScore ?? 0;
        if (score > 0)
        {
            Model.AddScore(score);
        }

        _bonusLocked = true;
        _bonusDestroyCountdown = 45;
        bonus.ShowScore();
    }

    /// <summary>
    /// Locks the bonus when it leaves the maze without being eaten.
    /// </summary>
    public void NotifyBonusExpired(BlPacManRoamingBonusBehavior bonus)
    {
        if (Assets.Bonus == bonus)
        {
            _bonusLocked = true;
        }
    }

    private void OnConsumableRegistered(BlPacManConsumableComponent consumable)
    {
        if (consumable is null)
        {
            return;
        }

        _remainingConsumables++;
    }

    private void OnConsumableEaten(BlPacManConsumableComponent consumable)
    {
        NotifyConsumableEaten(consumable);
    }

    /// <summary>
    /// Updates HUD bindings when the player's score changes.
    /// </summary>
    private void OnScoreChanged(int score)
    {
        // Score display is handled by dedicated HUD behaviours. Keep the hook for future updates.
    }

    /// <summary>
    /// Updates HUD bindings when the high-score value is bumped.
    /// </summary>
    private void OnHighScoreChanged(int score)
    {
        // Update hooks for score and HUD elements will be wired up once the UI sprites are in place.
    }

    /// <summary>
    /// Reacts to life count updates and triggers the game-over state when necessary.
    /// </summary>
    private void OnLivesChanged(int lives)
    {
        if (lives != 0)
        {
            return;
        }

        _gameOver = true;
        _Player.SoundStopBack();
        Model.ResetScore();
    }

    /// <summary>
    /// Retains a hook for extra-life awards so dedicated HUD behaviours can react if needed.
    /// </summary>
    private void OnExtraLivesChanged(int _)
    {
        // Lives HUD handles the bonus-life chime to keep audio logic close to the display.
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
    }

    /// <summary>
    /// Advances timers that control the roaming bonus' lifecycle.
    /// </summary>
    private void UpdateBonusLifecycle()
    {
        var bonus = Assets.Bonus;
        if (bonus is null)
        {
            return;
        }

        if (_bonusDestroyCountdown > 0)
        {
            _bonusDestroyCountdown--;
            if (_bonusDestroyCountdown == 0)
            {
                bonus.Deactivate();
            }

            return;
        }

        if (_bonusLocked)
        {
            return;
        }

        if (_bonusAppearCountdown > 0)
        {
            _bonusAppearCountdown--;
            if (_bonusAppearCountdown == 0)
            {
                bonus.Activate();
            }

            return;
        }

        bonus.Tick();
    }

    /// <summary>
    /// Chooses which looping audio to play based on frightened and dead ghost states.
    /// </summary>
    private void UpdateGhostAudioState()
    {
        if (_soundBackCooldown > 0)
        {
            _soundBackCooldown--;
            return;
        }

        if (IsMuted || _pacManEatenPending || IsPaused)
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

        _soundBackCooldown = 5;
    }

    /// <summary>
    /// Applies scoring, frightened mode transitions, and win detection when Pac-Man consumes an item.
    /// </summary>
    private void NotifyConsumableEaten(BlPacManConsumableComponent consumable)
    {
        if (consumable is null)
        {
            return;
        }

        if (_remainingConsumables > 0)
        {
            _remainingConsumables--;
        }

        if (consumable.ScoreValue > 0)
        {
            Model.AddScore(consumable.ScoreValue);
        }

        if (consumable.Type == BlPacManConsumableType.PowerPill)
        {
            _ghostChainIndex = 0;
            foreach (var ghost in Assets.Ghosts)
            {
                ghost.SetMode(GhostMode.Frightened);
            }
        }

        if (_remainingConsumables == 0)
        {
            _win = true;
        }
    }

    /// <summary>
    /// Restores actors after Pac-Man loses a life and restarts the countdown.
    /// </summary>
    private void ResumeAfterPacManEaten()
    {
        if (!_pacManEatenPending || Model.Lives == 0)
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

        Assets.Bonus?.ResetForLife();
        Assets.BroadcastPacManPosition();

        _pacManEatenPending = false;
        _startCountdown = Math.Max(_startCountdown, 1);
        _pauseFrames = 60;
    }
}
