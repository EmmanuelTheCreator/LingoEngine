using Blingo.PacMan.Core.Models;
using Blingo.PacMan.Core.Settings;
using Blingo.PacMan.Core.Sprites.GameObjectBehaviors;
using Blingo.PacMan.Core.Sprites.GeneralBehaviors;
using BlingoEngine.Core;

namespace Blingo.PacMan.Core.Game;

public sealed class GlobalVars : BlingoGlobalVars
{
   

    public GameModel GameModel { get; set; }

    internal PMGameBehavior? GameBehavior { get; set; }

    internal PMGameState State { get; } 

    internal PMBonusManager BonusManager { get; }
    internal PMPelletManager PelletManager { get; }
    internal PMPillManager PowerPillManager { get; }
    internal PMBonusAvailableManager BonusAvailableManager { get; }
    internal PMLivesManager LivesManager { get; }
    internal PMScoreManager ScoreManager { get; }

    public GameSettings? CurrentGameSettings { get; set; }

    public PacmanSettings? CurrentPacmanSettings { get; set; }

    public GhostSettings? CurrentGhostSettings { get; set; }
    
    internal BlPacManPauseBehavior? PauseBehavior { get; set; }

    /// <summary>
    /// Gets the map currently being played.
    /// </summary>
    public PMMap Map => LevelManager.Map;
    internal PMGameBehavior? Coordinator { get; set; }
    internal PMGhostManager GhostManager { get; }
    internal PMLevelManager LevelManager { get; }

    public GlobalVars()
    {
        State = new PMGameState(this);
        GameModel = new GameModel(this);
        BonusManager = new PMBonusManager(this);
        GhostManager = new PMGhostManager();
        LevelManager = new PMLevelManager();
        PelletManager = new PMPelletManager(this);
        PowerPillManager = new PMPillManager(this);
        BonusAvailableManager = new PMBonusAvailableManager(this);
        LivesManager = new PMLivesManager(this);
        ScoreManager = new PMScoreManager(this);
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
