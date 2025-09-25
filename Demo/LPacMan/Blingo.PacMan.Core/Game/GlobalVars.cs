using Blingo.PacMan.Core.Datas;
using Blingo.PacMan.Core.Models;
using Blingo.PacMan.Core.Sprites.GameObjectBehaviors;
using Blingo.PacMan.Core.Sprites.GeneralBehaviors;
using BlingoEngine.Core;

namespace Blingo.PacMan.Core.Game;

public sealed class GlobalVars : BlingoGlobalVars
{
    public GlobalVars()
    {
        State = new BlPacManGameState();
        BonusManager = new BlPacManBonusManager(this);
    }

    public GameModel? GameModel { get; set; }

    public BonusesModel? BonusesModel { get; set; }

    public BlPacManMapProvider? MapProvider { get; set; }

    internal BlPacManGameBehavior? GameBehavior { get; set; }

    internal BlPacManAssetContainer Assets { get; } = new();

    internal BlPacManGameState State { get; }

    internal BlPacManBonusManager BonusManager { get; }

    public GameSettings? CurrentGameSettings { get; set; }

    public PacmanSettings? CurrentPacmanSettings { get; set; }

    public GhostSettings? CurrentGhostSettings { get; set; }

    internal BlPacManPauseBehavior? PauseBehavior { get; set; }

    internal BlPacManEventMediator<BlPacManFieldContext>? ConsumableFieldMediator { get; set; }

    internal BlPacManFieldContext? CurrentFieldContext { get; set; }

    protected override void OnClearGlobals()
    {
        base.OnClearGlobals();
        GameModel = null;
        BonusesModel = null;
        MapProvider = null;
        GameBehavior = null;
        Assets.Reset();
        State.Reset();
        BonusManager.Reset();
        CurrentGameSettings = null;
        CurrentPacmanSettings = null;
        CurrentGhostSettings = null;
        ConsumableFieldMediator = null;
        CurrentFieldContext = null;
        PauseBehavior = null;
    }
}
