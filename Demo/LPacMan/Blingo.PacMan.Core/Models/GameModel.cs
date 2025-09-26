using Blingo.PacMan.Core.Engine;
using Blingo.PacMan.Core.Enums;
using Blingo.PacMan.Core.Game;
using Blingo.PacMan.Core.Sprites.GameObjectBehaviors;


namespace Blingo.PacMan.Core.Models;

/// <summary>
/// Tracks the level state
/// </summary>
public sealed class GameModel
{
    private readonly GlobalVars _globalVars;
    private const int _modeFrameRate = 30;
    private int _modeElapsedFrames;
    private GhostMode? _mode;
    private BlPacManActorBehavior? _pacMan;
    private readonly BlPacManEventMediator<GhostMode?> _modeChanged = new();

    public GameModel(GlobalVars globalVars)
    {
        _globalVars = globalVars;
    }

    
    public bool IsGameOver { get; private set; }
    public GhostMode? Mode => _mode;


    public BlPacManEventSubscription SubscribeModeChanged(Action<GhostMode?> handler) => _modeChanged.Subscribe(handler);
    internal BlPacManActorBehavior? PacMan => _pacMan;


    internal void AttachPacMan(BlPacManActorBehavior pacMan)
    {
        _pacMan = pacMan;
    }

    internal void DetachPacMan(BlPacManActorBehavior pacMan)
    {
        if (!ReferenceEquals(_pacMan, pacMan))
            return;

        _pacMan = null;
        _globalVars.State.ResetPacManPosition();
        
    }


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

    internal void ResumeAfterPacManEaten()
    {
        PacMan?.ResetForLife();
        PacMan?.Show();
    }
}
