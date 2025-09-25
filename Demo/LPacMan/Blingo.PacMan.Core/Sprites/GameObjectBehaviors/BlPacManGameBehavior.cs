using Blingo.PacMan.Core.Datas;
using Blingo.PacMan.Core.Game;
using Blingo.PacMan.Core.Models;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;
using BlingoEngine.Sprites;
using BlingoEngine.Sprites.Events;
using System.Reflection;

namespace Blingo.PacMan.Core.Sprites.GameObjectBehaviors;

/// <summary>
/// Coordinates the overall Pac-Man gameplay state, wiring the models and sprite behaviours into the runtime.
/// It keeps track of the game's meta state (start screen, pause, win, game over) and forwards mode updates to the model.
/// </summary>
internal sealed class BlPacManGameBehavior : BlingoSpriteBehavior,
    IHasBeginSpriteEvent,
    IHasEndSpriteEvent,
    IHasEnterFrameEvent
{
    private readonly GlobalVars _globals;
    private readonly GameModelRepository _gameModelRepository;
    private GameModel? _model;
    private readonly List<BlPacManEventSubscription> _modelSubscriptions = new();

    
    private GameSettings? _currentGameSettings;
    private PacmanSettings? _currentPacmanSettings;
    private GhostSettings? _currentGhostSettings;
    private bool _initialized;
    
    private GameModel Model => _model ??= _globals.GameModel ?? throw new InvalidOperationException("GameModel was not initialised.");

    private BlPacManGameState State => _globals.State;

    private BlPacManBonusManager BonusManager => _globals.BonusManager;



    public BlPacManGameBehavior(IBlingoMovieEnvironment env, GlobalVars globals, GameModelRepository gameModelRepository)
        : base(env)
    {
        _globals = globals;
        _gameModelRepository = gameModelRepository;
    }

    /// <inheritdoc />
    public void BeginSprite()
    {
        _globals.GameBehavior = this;
        if (_initialized)
            return;
        Model.Reset();
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
        _globals.Reset();
        _currentGameSettings = null;
        _currentPacmanSettings = null;
        _currentGhostSettings = null;
        UnsubscribeModelEvents();
        Model.Reset();
        _initialized = false;
    }

   

    /// <summary>
    /// Called by the start button sprite to kick off the current level.
    /// </summary>
    public void StartLevel()
    {
        if (!_initialized)
            return;

        //_Movie.GoTo(BlPacManProjectFactory.GameRunningLabel);

        if (State.Win)
        {
            _globals.LevelManager.GameWon();
            ResetInternalState(resetScore: false);
            MakeLevel();
            State.Win = false;
            return;
        }

        if (State.IsGameOver)
        {
            DoGameOver();
            return;
        }

        _Player.SoundPlayIntro();
        State.StartTheCountDown();
        
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
        _globals.LevelManager.Reset();
        
        ResetInternalState(resetScore: true);
        MakeLevel();
        _Movie.GoTo(BlPacManProjectFactory.MenuLabel);
    }

    /// <summary>
    /// Reads the level settings from the model and re-initialises counters used by the gameplay loop.
    /// </summary>
    private void MakeLevel()
    {
        GameSettings settings = _globals.LevelManager.MakeLevel();

        _currentGameSettings = settings;
        _currentPacmanSettings = _globals.LevelManager.GetPacmanSettings();
        _currentGhostSettings = _globals.LevelManager.GetGhostSettings();

        _globals.CurrentGameSettings = _currentGameSettings;
        _globals.CurrentPacmanSettings = _currentPacmanSettings;
        _globals.CurrentGhostSettings = _currentGhostSettings;

        State.ResetForNewLevel(settings, Model);

        if (Model.PacMan is { } pacMan && _currentPacmanSettings is not null)
            pacMan.Configure(this, _currentPacmanSettings);

        _globals.GhostManager.MakeLevel(_globals.LevelManager.GetGhostSettings());
        _globals.BonusManager.MakeLevel(settings);
        _globals.PelletManager.MakeLevel(_Movie);
        _globals.PowerPillManager.MakeLevel(_Movie);

    }

   

    /// <summary>
    /// Runs the central gameplay loop for each frame, coordinating timers, audio, and state checks.
    /// </summary>
    public void EnterFrame()
    {
        GameLoopTick();
        _Movie.GoTo(_Movie.CurrentFrame);
    }
    public void GameLoopTick()
    {
        if (_globals.Map is null)
            return;

        if (State.IsPaused)
            return;

        if (State.StartCountdown <= 0)
            Model.UpdateMode();

        if (State.PauseFrames > 0)
        {
            State.PauseFrames--;
            return;
        }

        if (State.DecrementStartCountdown())
            return;

        if (State.Win)
        {
            StartLevel();
            return;
        }

        if (State.IsGameOver)
            return;

        if (State.PacManEatenPending)
        {
            if (State.Lives == 0)
                return;

            ResumeAfterPacManEaten();
            return;
        }

        BonusManager.Update(Model);
        UpdateGhostAudioState();
    }

    public void Load()
    {
        var data = _gameModelRepository.Load();
        if (data is null)
            return;

        Model.SetHighScore(data.HighScore);
    }

    public void Save()
    {
        _gameModelRepository.Save(new BlPacManSaveData
        {
            HighScore = Model.HighScore,
        });
    }



    /// <summary>
    /// Clears transient counters and optionally resets the score when restarting gameplay.
    /// </summary>
    /// <param name="resetScore">Whether to wipe the player's score.</param>
    private void ResetInternalState(bool resetScore)
    {
        State.Reset();
        _globals.PauseBehavior?.Reset();

        if (resetScore)
            Model.ResetScore();

        Model.ResetLives(_globals.LevelManager.GetGameSettings().DefaultLives + 1);
    }

    /// <summary>
    /// Subscribes to model change notifications so the behaviour can react to score, lives, and mode updates.
    /// </summary>
    private void SubscribeModelEvents()
    {
        ReleaseModelSubscriptions();
        _modelSubscriptions.Add(Model.SubscribeLivesChanged(OnLivesChanged));
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
            return;

        foreach (var subscription in _modelSubscriptions)
            subscription.Release();

        _modelSubscriptions.Clear();
    }

  
    /// <summary>
    /// Handles Pac-Man's death sequence and schedules the life reset.
    /// </summary>
    public void NotifyPacManEaten()
    {
        if (State.PacManEatenPending || State.IsGameOver)
            return;

        _globals.GhostManager.OnEatenByGhost();
        BonusManager.OnPacManEaten();
        State.OnPacManEaten();
        Model.OnPacManEaten();
    }
    /// <summary>
    /// Restores actors after Pac-Man loses a life and restarts the countdown.
    /// </summary>
    private void ResumeAfterPacManEaten()
    {
        if (!State.PacManEatenPending || State.Lives == 0)
            return;

        Model.ResumeAfterPacManEaten();
        State.ResumeAfterPacManEaten();
        _globals.GhostManager.ResumeAfterPacManEaten();
        _globals.BonusManager.ResetAfterLifeLost();

    }


    /// <summary>
    /// Reacts to life count updates and triggers the game-over state when necessary.
    /// </summary>
    private void OnLivesChanged(int lives)
    {
        if (lives != 0)
            return;
        DoGameOver();
    }

    public void DoGameOver()
    {
        Model.GameIsOver();
        _Player.SoundStopBack();
        _globals.LevelManager.GameIsOver();
        ResetInternalState(resetScore: true);
        MakeLevel();
        _Player.SoundPlayIntro();
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

        if (State.IsMuted || State.PacManEatenPending || State.IsPaused)
            return;

        if (_globals.GhostManager.HasDeathGhosts())
            _Player.SoundPlayDead();
        else if (!_globals.GhostManager.HasFrightenedGhosts())
            _Player.SoundPlayBack();

        State.SoundCooldown = 5;
    }

    
}
