using AbstUI.Primitives;
using Blingo.PacMan.Core.Sprites.GameObjectBehaviors;
using BlingoEngine.Bitmaps;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;

namespace Blingo.PacMan.Core.Game;

internal sealed class BlPacManPowerPillManager 
{
    private static readonly ARect _pillRect = new(32, 0, 48, 24);

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
        var spriteNum = index + 71;
        lingoMovie.Channel(spriteNum).Puppet = true;
        BlingoSprite2D sprite2D = (BlingoSprite2D)lingoMovie.GetSprite(spriteNum)!;
        sprite2D.LocH = tile.CenterX;
        sprite2D.LocV = tile.CenterY;
        sprite2D.Puppet = true;
        sprite2D.Visibility = true;

        var cast = lingoMovie.CastLib["Data"];
        var member = cast?.GetMember<BlingoMemberBitmap>("pills");
        if (member != null)
            sprite2D.Member = member;

        sprite2D.MemberSourceRect = _pillRect;

        var behavior = sprite2D.SetBehavior<BlPacManPowerPillBehavior>();
        behavior.Component.SetGlobals(_globals);
        behavior.Initialize(tile);
        _spawnedConsumables.Add(behavior.Component);
    }
}
