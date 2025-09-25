using Blingo.PacMan.Core.Datas;
using Blingo.PacMan.Core.Game;
using Blingo.PacMan.Core.Sprites.GameObjectBehaviors;
using System.Collections.ObjectModel;
using System.Reflection;


namespace Blingo.PacMan.Core.Models;

/// <summary>
/// Tracks the level state, score and life counters while exposing game settings.
/// </summary>
public sealed class GameModel
{
    private readonly GlobalVars _globalVars;
    private const int _modeFrameRate = 30;
    private int _modeElapsedFrames;
    
    private int _score;
    private int _highScore;
    private int _lives = 3;
    private int _extraLives = 1;
    private GhostMode? _mode;

    

   
    private BlPacManActorBehavior? _pacMan;

    
    private readonly BlPacManEventMediator<int> _scoreChanged = new();
    private readonly BlPacManEventMediator<int> _highScoreChanged = new();
    private readonly BlPacManEventMediator<int> _livesChanged = new();
    private readonly BlPacManEventMediator<int> _extraLivesChanged = new();
    private readonly BlPacManEventMediator<GhostMode?> _modeChanged = new();

    public GameModel(GlobalVars globalVars)
    {
        _globalVars = globalVars;
    }

    
    public bool IsGameOver { get; private set; }
    public int Score => _score;

    public int HighScore => _highScore;

    public int Lives => _lives;

    public int ExtraLives => _extraLives;

    public int ExtraLifeScore { get; set; } = 10_000;

    public GhostMode? Mode => _mode;

    

    public BlPacManEventSubscription SubscribeScoreChanged(Action<int> handler) => _scoreChanged.Subscribe(handler);

    public BlPacManEventSubscription SubscribeHighScoreChanged(Action<int> handler) => _highScoreChanged.Subscribe(handler);

    public BlPacManEventSubscription SubscribeLivesChanged(Action<int> handler) => _livesChanged.Subscribe(handler);

    public BlPacManEventSubscription SubscribeExtraLivesChanged(Action<int> handler) => _extraLivesChanged.Subscribe(handler);

    public BlPacManEventSubscription SubscribeModeChanged(Action<GhostMode?> handler) => _modeChanged.Subscribe(handler);


    internal BlPacManActorBehavior? PacMan => _pacMan;


    internal void AttachPacMan(BlPacManActorBehavior pacMan)
    {
        _pacMan = pacMan ?? throw new ArgumentNullException(nameof(pacMan));
    }

    internal void DetachPacMan(BlPacManActorBehavior pacMan)
    {
        if (!ReferenceEquals(_pacMan, pacMan))
            return;

        _pacMan = null;
        _globalVars.State.ResetPacManPosition();
        
    }


   


    #region Scores
    public void AddScore(int score)
    {
        if (score == 0)
            return;

        _score = Math.Max(0, _score + score);
        OnScoreChanged();
    }

    public void SetScore(int score)
    {
        _score = Math.Max(0, score);
    }
    public void SetHighScore(int score)
    {
        _highScore = Math.Max(0, score);
    }
    public void ResetScore()
    {
        if (_score == 0)
        {
            return;
        }

        _score = 0;
        OnScoreChanged();
    }
    private void OnScoreChanged()
    {
        _scoreChanged.Publish(_score);

        if (_extraLives > 0 && _score >= ExtraLifeScore)
        {
            _extraLives--;
            _extraLivesChanged.Publish(_extraLives);
            SetLives(_lives + 1);
        }

        if (_score > _highScore)
        {
            _highScore = _score;
            _highScoreChanged.Publish(_highScore);
        }
    }
    #endregion


    #region Lives
    public void ResetLives(int lives)
    {
        SetLives(Math.Max(0, lives));
    }
    private void SetLives(int lives)
    {
        if (_lives == lives)
            return;

        _lives = lives;
        _livesChanged.Publish(_lives);
    } 
    #endregion


    #region Mode
    public void UpdateMode()
    {
        var sequence = _globalVars.LevelManager.GetGameSettings().ModeSequence;
        if (sequence.Count == 0)
        {
            SetMode(null);
            return;
        }

        if (!_globalVars.State.IsPaused)
            _modeElapsedFrames++;

        var elapsedFrames = _modeElapsedFrames;
        var cumulativeFrames = 0;

        GhostMode? selected = null;
        for (var i = 0; i < sequence.Count; i++)
        {
            var timing = sequence[i];
            cumulativeFrames += ConvertToFrames(timing.Duration);
            if (elapsedFrames < cumulativeFrames || i == sequence.Count - 1)
            {
                selected = timing.Mode;
                break;
            }
        }

        SetMode(selected);
    }
    private void SetMode(GhostMode? mode)
    {
        if (_mode == mode)
            return;

        _mode = mode;
        _modeChanged.Publish(_mode);
    }

    private void ResetModeTimer()
    {
        _modeElapsedFrames = 0;
        _globalVars.PauseBehavior?.Reset();
        SetMode(null);
    }

    #endregion


   



    public void Reset()
    {
        _score = 0;
        _highScore = 0;
        _lives = 0;
        _globalVars.GhostManager.Reset();
        _globalVars.LevelManager.Reset();
        _pacMan = null;
        IsGameOver = false;
        
    }
    public void ResetForNewLevel()
    {
        IsGameOver = false;
    }
    private static int ConvertToFrames(TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
            return 1;

        var frames = (int)Math.Ceiling(duration.TotalSeconds * _modeFrameRate);
        return Math.Max(1, frames);
    }


    internal void GameIsOver()
    {
        IsGameOver = false;
    }

    internal void OnPacManEaten()
    {
        var lives = Math.Max(0, Lives - 1);
        ResetLives(lives);
    }

    internal void ResumeAfterPacManEaten()
    {
        PacMan?.ResetForLife();
        PacMan?.Show();
    }
}
