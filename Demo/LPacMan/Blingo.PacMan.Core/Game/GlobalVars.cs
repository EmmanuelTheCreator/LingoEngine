using Blingo.PacMan.Core.Datas;
using Blingo.PacMan.Core.Models;
using Blingo.PacMan.Core.Sprites.GameObjectBehaviors;
using BlingoEngine.Core;

namespace Blingo.PacMan.Core.Game;

public sealed class GlobalVars : BlingoGlobalVars
{
    public GameModel? GameModel { get; set; }

    public BonusesModel? BonusesModel { get; set; }

    public BlPacManMapProvider? MapProvider { get; set; }

    internal BlPacManGameBehavior? GameBehavior { get; set; }

    public GameSettings? CurrentGameSettings { get; set; }

    public PacmanSettings? CurrentPacmanSettings { get; set; }

    public GhostSettings? CurrentGhostSettings { get; set; }

    internal BlPacManEventMediator<BlPacManFieldContext>? ConsumableFieldMediator { get; set; }

    internal BlPacManFieldContext? CurrentFieldContext { get; set; }

    protected override void OnClearGlobals()
    {
        base.OnClearGlobals();
        GameModel = null;
        BonusesModel = null;
        MapProvider = null;
        GameBehavior = null;
        CurrentGameSettings = null;
        CurrentPacmanSettings = null;
        CurrentGhostSettings = null;
        ConsumableFieldMediator = null;
        CurrentFieldContext = null;
    }
}
