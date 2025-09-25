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

    private int _bonusAppearCountdown;
    private int _bonusDestroyCountdown;
    private bool _bonusLocked;
    private bool _pacManEatenPending;
    private int _ghostChainIndex;

    private readonly BlPacManEventMediator<BlPacManPositionContext> _pacManPositionChanged = new();
    private readonly List<BlPacManEventSubscription> _pacManSubscriptions = new();
    private BlPacManPositionContext? _lastPacManPosition;

    private bool _initialized;
    private bool _muted;
    private bool _paused;
    private bool _win;
    private bool _gameOver;

    private static readonly int[] GhostScoreChain = { 200, 400, 800, 1_600 };

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
        ReleasePacManSubscriptions();
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
        _bonus?.ResetForLife();
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

    /// <summary>
    /// Switches between paused and resumed gameplay states, invoking the appropriate handlers.
    /// </summary>
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

    /// <summary>
    /// Notifies the model of a pause and halts looping sound effects.
    /// </summary>
    private void Pause()
    {
        Model.Pause();
        _Player.SoundStopBack();
    }

    /// <summary>
    /// Resumes the model and background sounds if audio is not muted.
    /// </summary>
    private void Resume()
    {
        Model.Resume();
        if (!_muted)
        {
            _Player.SoundPlayBack();
        }
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
        _paused = false;
        _remainingConsumables = 0;
        _consumables.Clear();
        _bonusAppearCountdown = 500;
        _bonusDestroyCountdown = 0;
        _bonusLocked = false;
        _pacManEatenPending = false;
        _ghostChainIndex = 0;
        _lastPacManPosition = null;

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
    /// Gets a value indicating whether gameplay updates should be skipped for this frame.
    /// </summary>
    public bool IsGameplayFrozen => _paused || _pauseFrames > 0 || _startCountdown > 0 || _win || _gameOver || _pacManEatenPending;

    /// <summary>
    /// Provides a subscription to Pac-Man's live position feed.
    /// </summary>
    public BlPacManEventSubscription SubscribePacManPosition(Action<BlPacManPositionContext> handler)
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        var subscription = _pacManPositionChanged.Subscribe(handler);

        if (_lastPacManPosition is { } context)
        {
            handler(context);
        }

        return subscription;
    }

    /// <summary>
    /// Searches for a registered ghost by name.
    /// </summary>
    public BlPacManGhostBehavior? FindGhost(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return _ghosts.FirstOrDefault(g => string.Equals(g.GhostName, name, StringComparison.OrdinalIgnoreCase));
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
        _Player.SoundPlayEat();
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
        _Player.SoundPlayEaten();
        _Player.SoundStopBack();
        _pauseFrames = Math.Max(_pauseFrames, 40);
        _soundBackCooldown = 0;

        _pacMan?.Hide();
        foreach (var ghost in _ghosts)
        {
            ghost.OnPacManEaten();
        }

        _bonus?.OnPacManEaten();
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

        _Player.SoundPlayBonus();
        _bonusLocked = true;
        _bonusDestroyCountdown = 45;
        bonus.ShowScore();
    }

    /// <summary>
    /// Locks the bonus when it leaves the maze without being eaten.
    /// </summary>
    public void NotifyBonusExpired(BlPacManRoamingBonusBehavior bonus)
    {
        if (_bonus == bonus)
        {
            _bonusLocked = true;
        }
    }

    /// <summary>
    /// Registers a dot, pill, or bonus so the coordinator can track remaining items.
    /// </summary>
    public void RegisterConsumable(BlPacManConsumableComponent consumable)
    {
        if (consumable is null)
        {
            throw new ArgumentNullException(nameof(consumable));
        }

        _consumables.Add(consumable);
        _remainingConsumables++;
    }

    /// <summary>
    /// Processes score, audio, and frightened state when Pac-Man eats a consumable.
    /// </summary>
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
                _Player.SoundPlayFrightened();
                _pauseFrames = Math.Max(_pauseFrames, 2);
                _ghostChainIndex = 0;
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

    /// <summary>
    /// Hooks Pac-Man into the coordinator and reapplies the latest settings.
    /// </summary>
    public void RegisterPacMan(BlPacManActorBehavior pacMan)
    {
        _pacMan = pacMan ?? throw new ArgumentNullException(nameof(pacMan));
        if (_currentPacmanSettings is not null)
        {
            _pacMan.Configure(this, _currentPacmanSettings);
        }

        HookPacManEvents(_pacMan);
    }

    /// <summary>
    /// Adds a ghost to the internal registry and configures it with the current level data.
    /// </summary>
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

    /// <summary>
    /// Stores the roaming bonus instance so it can be triggered when the timer elapses.
    /// </summary>
    public void RegisterBonus(BlPacManRoamingBonusBehavior bonus)
    {
        _bonus = bonus ?? throw new ArgumentNullException(nameof(bonus));
        if (_currentGameSettings is not null)
        {
            _bonus.Configure(this, _currentGameSettings);
        }
    }

    /// <summary>
    /// Removes a consumable from the remaining counter when it leaves the stage.
    /// </summary>
    public void UnregisterConsumable(BlPacManConsumableComponent consumable)
    {
        if (consumable is null)
        {
            return;
        }

        _consumables.Remove(consumable);
    }

    /// <summary>
    /// Clears references when Pac-Man's sprite is removed from the stage.
    /// </summary>
    public void UnregisterPacMan(BlPacManActorBehavior pacMan)
    {
        if (_pacMan == pacMan)
        {
            ReleasePacManSubscriptions();
            _pacMan = null;
        }
    }

    /// <summary>
    /// Removes a ghost from the internal list.
    /// </summary>
    public void UnregisterGhost(BlPacManGhostBehavior ghost)
    {
        _ghosts.Remove(ghost);
    }

    /// <summary>
    /// Clears the active bonus reference once it despawns.
    /// </summary>
    public void UnregisterBonus(BlPacManRoamingBonusBehavior bonus)
    {
        if (_bonus == bonus)
        {
            _bonus = null;
        }
    }

    /// <summary>
    /// Subscribes to Pac-Man callbacks and immediately publishes his current position.
    /// </summary>
    private void HookPacManEvents(BlPacManActorBehavior pacMan)
    {
        if (pacMan is null)
        {
            return;
        }

        ReleasePacManSubscriptions();

        var character = pacMan.Character;
        _pacManSubscriptions.Add(character.SubscribePositionChanged(OnPacManPositionChanged));

        var sprite = pacMan.GetSprite();
        var tile = character.GetTile();
        var snapshot = new BlPacManPositionContext(sprite.LocH, sprite.LocV, tile, character.Direction);
        OnPacManPositionChanged(snapshot);
    }

    private void OnPacManPositionChanged(BlPacManPositionContext context)
    {
        _lastPacManPosition = context;
        _pacManPositionChanged.Publish(context);
    }

    /// <summary>
    /// Emits a snapshot of Pac-Man's position to all subscribers.
    /// </summary>
    private void BroadcastPacManPosition()
    {
        if (_pacMan is null)
        {
            return;
        }

        var character = _pacMan.Character;
        var sprite = _pacMan.GetSprite();
        OnPacManPositionChanged(new BlPacManPositionContext(sprite.LocH, sprite.LocV, character.GetTile(), character.Direction));
    }

    /// <summary>
    /// Releases any temporary subscriptions when Pac-Man despawns.
    /// </summary>
    private void ReleasePacManSubscriptions()
    {
        if (_pacManSubscriptions.Count == 0)
        {
            return;
        }

        foreach (var subscription in _pacManSubscriptions)
        {
            subscription.Release();
        }

        _pacManSubscriptions.Clear();
    }

    /// <summary>
    /// Placeholder hook for HUD updates once the score display behaviours are connected.
    /// </summary>
    private void OnScoreChanged(int score)
    {
        // Score display is handled by dedicated HUD behaviours. Keep the hook for future updates.
    }

    /// <summary>
    /// Placeholder hook that will bridge to the high-score HUD once implemented.
    /// </summary>
    private void OnHighScoreChanged(int score)
    {
        // Update hooks for score and HUD elements will be wired up once the UI sprites are in place.
    }

    /// <summary>
    /// Handles the game-over transition when the player's lives reach zero.
    /// </summary>
    private void OnLivesChanged(int lives)
    {
        if (lives == 0)
        {
            _gameOver = true;
            _Player.SoundStopBack();
            Model.ResetScore();
        }
    }

    /// <summary>
    /// Plays the extra-life sound whenever the model awards one.
    /// </summary>
    private void OnExtraLivesChanged(int _)
    {
        _Player.SoundPlayLife();
    }

    /// <summary>
    /// Propagates global mode changes to each registered ghost.
    /// </summary>
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

    /// <summary>
    /// Keeps the bonus display model in sync with the active level.
    /// </summary>
    private void OnLevelChanged(int level)
    {
        BonusesModel.Level = level;
    }

    /// <summary>
    /// Advances timers that control when the roaming bonus appears, despawns, or moves.
    /// </summary>
    private void UpdateBonusLifecycle()
    {
        if (_bonus is null)
        {
            return;
        }

        if (_bonusDestroyCountdown > 0)
        {
            _bonusDestroyCountdown--;
            if (_bonusDestroyCountdown == 0)
            {
                _bonus.Deactivate();
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
                _bonus.Activate();
            }

            return;
        }

        _bonus.Tick();
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

        if (_muted || _pacManEatenPending || _paused)
        {
            return;
        }

        if (_ghosts.Any(static g => g.IsDead))
        {
            _Player.SoundPlayDead();
        }
        else if (!_ghosts.Any(static g => g.IsFrightened))
        {
            _Player.SoundPlayBack();
        }

        _soundBackCooldown = 5;
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

        _pacMan?.ResetForLife();
        _pacMan?.Show();

        foreach (var ghost in _ghosts)
        {
            ghost.ResetForLife();
            ghost.Show();
        }

        _bonus?.ResetForLife();
        BroadcastPacManPosition();

        _pacManEatenPending = false;
        _startCountdown = Math.Max(_startCountdown, 1);
        _pauseFrames = 60;
    }
}
