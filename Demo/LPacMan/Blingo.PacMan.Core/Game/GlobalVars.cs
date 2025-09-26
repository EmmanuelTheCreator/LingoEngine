using Blingo.PacMan.Core.Models;
using Blingo.PacMan.Core.Settings;
using Blingo.PacMan.Core.Sprites.GameObjectBehaviors;
using Blingo.PacMan.Core.Sprites.GeneralBehaviors;
using BlingoEngine.Core;

namespace Blingo.PacMan.Core.Game;

public sealed class GlobalVars : BlingoGlobalVars
{
   

    public GameModel GameModel { get; set; }

    internal BlPacManGameBehavior? GameBehavior { get; set; }

    internal BlPacManGameState State { get; } 

    internal BlPacManBonusManager BonusManager { get; }
    internal BlPacManPelletManager PelletManager { get; }
    internal BlPacManPowerPillManager PowerPillManager { get; }
    internal BlPacManBonusAvailableManager BonusAvailableManager { get; }
    internal BlPacManLivesManager LivesManager { get; }
    internal BlPacManScoreManager ScoreManager { get; }

    public GameSettings? CurrentGameSettings { get; set; }

    public PacmanSettings? CurrentPacmanSettings { get; set; }

    public GhostSettings? CurrentGhostSettings { get; set; }
    
    internal BlPacManPauseBehavior? PauseBehavior { get; set; }

    /// <summary>
    /// Gets the map currently being played.
    /// </summary>
    public Map Map => LevelManager.Map;
    internal BlPacManGameBehavior? Coordinator { get; set; }
    internal BlGhostManager GhostManager { get; }
    internal BlLevelManager LevelManager { get; }

    public GlobalVars()
    {
        State = new BlPacManGameState(this);
        GameModel = new GameModel(this);
        BonusManager = new BlPacManBonusManager(this);
        GhostManager = new BlGhostManager();
        LevelManager = new BlLevelManager();
        PelletManager = new BlPacManPelletManager(this);
        PowerPillManager = new BlPacManPowerPillManager(this);
        BonusAvailableManager = new BlPacManBonusAvailableManager(this);
        LivesManager = new BlPacManLivesManager(this);
        ScoreManager = new BlPacManScoreManager(this);
    }

    protected override void OnClearGlobals()
    {
        base.OnClearGlobals();
        GameModel = null;
        GameBehavior = null;
        Reset();
        State.Reset();
        BonusManager?.Reset();
        CurrentGameSettings = null;
        CurrentPacmanSettings = null;
        CurrentGhostSettings = null;
        PauseBehavior = null;
    }
    public void Reset()
    {
        GameBehavior = null;
        CurrentGameSettings = null;
        CurrentPacmanSettings = null;
        CurrentGhostSettings = null;
    }

}
