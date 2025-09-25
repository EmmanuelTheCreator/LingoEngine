using Blingo.PacMan.Core.Datas;
using Blingo.PacMan.Core.Models;
using Blingo.PacMan.Core.Sprites.GameObjectBehaviors;

namespace Blingo.PacMan.Core.Game;

/// <summary>
/// Holds runtime flags and counters that multiple behaviours share while the Pac-Man game runs.
/// </summary>
internal sealed class BlPacManGameState
{
  
    private GlobalVars _globalVars;

   

    public bool IsMuted { get; set; }

    public bool IsPaused => _globalVars.PauseBehavior?.IsPaused ?? false;

    public bool Win { get; set; }

    public bool IsGameOver => Game.IsGameOver;

    public bool PacManEatenPending { get; set; }

    public int PauseFrames { get; set; }

    public int StartCountdown { get; private set; }

    public int SoundCooldown { get; set; }

    public int RemainingConsumables { get; set; }

   

   

    public int Score => Game.Score;

    public int HighScore => Game.HighScore;

    public int Lives => Game.Lives;
    public int ExtraLives => Game.ExtraLives;


    public Map? CurrentMap { get; set; }


    public bool IsGameplayFrozen => IsPaused || PauseFrames > 0 || StartCountdown > 0 || Win || IsGameOver || PacManEatenPending;
   
    public BlPacManPositionEventData? PacManPosition { get; private set; }



    public GameModel Game => _globalVars.GameModel;


    public BlPacManGameState(GlobalVars globalVars)
    {
        _globalVars = globalVars;
        
    }

  

    public void UpdatePacManPosition(BlPacManPositionEventData context)
    {
        PacManPosition = context;
    }

   
    public void Reset()
    {
        _globalVars.BonusManager?.Reset();
        Win = false;
        IsMuted = false;
        PacManEatenPending = false;
        PauseFrames = 0;
        StartCountdown = 2;
        SoundCooldown = 0;
        RemainingConsumables = 0;
        _globalVars.GhostManager.Reset();
        _globalVars.BonusManager?.Reset();
       
        Game.Reset();
        PacManPosition = null;
    }

    public void ResetForNewLevel(GameSettings settings, GameModel model)
    {
        _globalVars.BonusManager?.ResetForNewLevel();
        _globalVars.GhostManager.Reset();
        
        PauseFrames = 80;
        StartCountdown = 2;
        SoundCooldown = 0;
        PacManEatenPending = false;
        Win = false;
        model.Reset();
        RemainingConsumables = 0;
        Game.Reset();
    }
    internal void ResetPacManPosition()
    {
        PacManPosition = null;
    }

    public void RegisterConsumableSpawn()
    {
        RemainingConsumables++;
    }

    public void ConsumableEaten()
    {
        if (RemainingConsumables > 0)
            RemainingConsumables--;
        if (RemainingConsumables == 0)
            Win = true;
    }

    internal void ResumeAfterPacManEaten()
    {
        PacManEatenPending = false;
        StartTheCountDown();
        PauseFrames = Math.Max(PauseFrames, 60);
    }

    internal void StartTheCountDown()
    {
        StartCountdown = Math.Max(StartCountdown, 1);
    }

    internal void OnPacManEaten()
    {
        PacManEatenPending = true;
        PauseFrames = Math.Max(PauseFrames, 40);
        SoundCooldown = 0;
    }

    public bool DecrementStartCountdown()
    {
        if (StartCountdown > 0)
        {
            StartCountdown--;
            if (StartCountdown == 0)
                PauseFrames = Math.Max(PauseFrames, 60);

            return true;
        }
        return false;
    }

    
}
