using Blingo.PacMan.Core;
using Blingo.PacMan.Core.Datas;
using Blingo.PacMan.Core.Engine;
using Blingo.PacMan.Core.Game;
using Blingo.PacMan.Core.Models;
using Blingo.PacMan.Core.Settings;
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
    private readonly BlPacManRepository _gameRepository;
    private GameModel? _model;

    
    private GameSettings? _currentGameSettings;
    private PacmanSettings? _currentPacmanSettings;
    private GhostSettings? _currentGhostSettings;
    private bool _initialized;
    private IBlingoSpriteChannel _spriteTextPlayer1 = null!;
    private IBlingoSpriteChannel _spriteTextPlayer2 = null!;
    private IBlingoSpriteChannel _spriteTextReady = null!;
    private IBlingoSpriteChannel _gameBG = null!;

    private GameModel Model => _model ??= _globals.GameModel ?? throw new InvalidOperationException("GameModel was not initialised.");

    private BlPacManGameState State => _globals.State;

    private BlPacManBonusManager BonusManager => _globals.BonusManager;



    public BlPacManGameBehavior(IBlingoMovieEnvironment env, GlobalVars globals, BlPacManRepository gameRepository)
        : base(env)
    {
        _globals = globals;
        _gameRepository = gameRepository;
    }

    /// <inheritdoc />
    public void BeginSprite()
    {
        _globals.GameBehavior = this;
        if (_initialized)
            return;
        _gameBG = Sprite(PCSpriteNums.GameBG);
        _globals.ScoreManager.Init(_Movie);
        InitTargetSprites();
        Model.Reset();
        StartCleanNewGame();
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
        Model.Reset();
        _initialized = false;
    }

   

   
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
        _globals.LevelManager.ResetForNewLevel();
        _globals.GameModel.PacMan?.Configure(this, _currentPacmanSettings);

        _globals.GhostManager.MakeLevel(_globals.LevelManager.GetGhostSettings());
        _globals.BonusManager.MakeLevel(settings);
        _globals.PelletManager.MakeLevel(_Movie);
        _globals.PowerPillManager.MakeLevel(_Movie);
        _globals.BonusAvailableManager.Init(_Movie);
        _globals.LivesManager.Init(_Movie,_Player);
        _gameBG.SetMember(_currentGameSettings.MazeMemberName);
        if (_gameBG.Sprite is BlingoSprite2D background)
        {
            background.Width = BlPacManProjectFactory.GameWidth;
            background.Height = BlPacManProjectFactory.GameHeight;
        }

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

        if (State.WaitForPauseTick())
            return;

        if (State.DecrementStartCountdown(out var countDownChanged))
        {
            ShowReady();
            ShowTextPlayer1();
            return;
        }
        if (!State.IsActivePlaying)
        {
            State.IsActivePlaying = true;
            HideReady();
            HideTextPlayer1();
            HideTextPlayer2();
        }

        if (State.Win)
        {
            StartLevel();
            return;
        }

        if (State.IsGameOver)
            return;

        if (State.PacManEatenPending)
        {
            if (!_globals.LivesManager.HasLives())
                return;

            ResumeAfterPacManEaten();
            return;
        }

        BonusManager.Update(Model);
        UpdateGhostAudioState();
    }

    public void Load()
    {
        var data = _gameRepository.Load();
        if (data is null)
            return;

        _globals.ScoreManager.SetHighScore(data.HighScore);
    }

    public void Save()
    {
        _gameRepository.Save(new BlPacManSaveData
        {
            HighScore = _globals.ScoreManager.HighScore,
        });
    }

    /// <summary>
    /// Called by the start button sprite to kick off the current level.
    /// </summary>
    public void StartCleanNewGame()
    {
        _globals.LevelManager.Reset();
        ResetInternalState(true);
    }


    /// <summary>
    /// Clears transient counters and optionally resets the score when restarting gameplay.
    /// </summary>
    /// <param name="resetScore">Whether to wipe the player's score.</param>
    private void ResetInternalState(bool resetScore)
    {
        State.Reset();
        _globals.PauseBehavior?.Reset();
        _globals.BonusManager?.Reset();
        _globals.GhostManager.Reset();

        if (resetScore)
            _globals.ScoreManager.ResetScore();

        _globals.LivesManager.ResetLives(_globals.LevelManager.GetGameSettings().DefaultLives + 1);
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
        _globals.LivesManager.OnPacManEaten();
    }
    /// <summary>
    /// Restores actors after Pac-Man loses a life and restarts the countdown.
    /// </summary>
    private void ResumeAfterPacManEaten()
    {
        if (!State.PacManEatenPending || !_globals.LivesManager.HasLives())
            return;

        Model.ResumeAfterPacManEaten();
        State.ResumeAfterPacManEaten();
        _globals.GhostManager.ResumeAfterPacManEaten();
        _globals.BonusManager.ResetAfterLifeLost();

    }

    public void DoGameOver()
    {
        Model.GameIsOver();
        _Player.SoundStopBack();
        _globals.LevelManager.GameIsOver();
        ResetInternalState(resetScore: true);
        MakeLevel();
        _Player.SoundPlayIntro();
        State.IsActivePlaying = false;
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

    private void InitTargetSprites()
    {
        _spriteTextPlayer1 = Sprite(9);
        _spriteTextPlayer2 = Sprite(10);
        _spriteTextReady = Sprite(11);
        HideTextPlayer2();
    }

    public void ShowTextPlayer1() => _spriteTextPlayer1.Blend = 100;
    public void ShowTextPlayer2() => _spriteTextPlayer2.Blend = 100;
    public void HideTextPlayer1() => _spriteTextPlayer1.Blend = 0;
    public void HideTextPlayer2() => _spriteTextPlayer2.Blend = 0;
    public void ShowReady() => _spriteTextReady.Blend = 100;
    public void HideReady() => _spriteTextReady.Blend = 0;


}
