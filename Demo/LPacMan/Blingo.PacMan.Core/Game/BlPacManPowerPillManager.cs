using AbstUI.Primitives;
using Blingo.PacMan.Core.Sprites.GameObjectBehaviors;
using BlingoEngine.Bitmaps;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;

namespace Blingo.PacMan.Core.Game;

internal sealed class BlPacManPowerPillManager 
{
    private readonly List<BlPacManConsumableComponent> _spawnedConsumables = new();
    private readonly GlobalVars _globals;


    public BlPacManPowerPillManager(GlobalVars globals)
    {
        _globals = globals;
    }

    public void MakeLevel(IBlingoMovie lingoMovie)
    {
        ClearField();
        CreateMap(lingoMovie);
    }
    public void ClearField()
    {
        foreach (var consumable in _spawnedConsumables)
            consumable.Destroy();

        _spawnedConsumables.Clear();
    }
    private void CreateMap(IBlingoMovie lingoMovie)
    {
        if (_globals.Map == null)
            return;
        var pills = _globals.Map.Tiles.Where(x => x.Code == '*').ToList();
        for (int i = 0; i < pills.Count; i++)
        {
            Tile? tile = pills[i];
            CreateConsumableSprite(lingoMovie, tile,i);
        }
    }

    private void CreateConsumableSprite(IBlingoMovie lingoMovie, Tile tile, int index)
    {
        var name = $"PowerPill_{tile.Column}_{tile.Row}";
        var spriteNum = index + PCSpriteNums.PowerPillStart;
        lingoMovie.Channel(spriteNum).Puppet = true;
        BlingoSprite2D sprite2D = (BlingoSprite2D)lingoMovie.GetSprite(spriteNum)!;
        sprite2D.LocH = tile.CenterX-2;
        sprite2D.LocV = tile.CenterY-2;
        sprite2D.SetMember("pills");
        sprite2D.MemberSourceRect = new(0, 0, 6, 6);

        var behavior = sprite2D.SetBehavior<BlPacManPowerPillBehavior>();
        behavior.Component.SetGlobals(_globals);
        behavior.Initialize(tile);
        _spawnedConsumables.Add(behavior.Component);
    }
}
