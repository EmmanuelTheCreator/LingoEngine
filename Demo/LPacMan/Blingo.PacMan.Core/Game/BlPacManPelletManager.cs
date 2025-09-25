using AbstUI.Primitives;
using Blingo.PacMan.Core.Game;
using BlingoEngine.Bitmaps;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;

namespace Blingo.PacMan.Core.Sprites.GameObjectBehaviors;

internal sealed class BlPacManPelletManager
{
    private static readonly ARect _pelletRect = new(0, 0, 16, 24);

    private readonly List<BlPacManConsumableComponent> _spawnedConsumables = new();
    private readonly GlobalVars _globals;

    public BlPacManPelletManager(GlobalVars globals)
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
        var pellets = _globals.Map.Tiles.Where(x => x.Code == '.').ToList();
        for (int i = 0; i < pellets.Count; i++)
        {
            Tile? tile = pellets[i];
            CreateConsumableSprite(lingoMovie, tile , i);
        }
    }

    private void CreateConsumableSprite(IBlingoMovie lingoMovie, Tile tile,  int index)
    {
        var name = $"Pellet_{tile.Column}_{tile.Row}";
        var spriteNum = index + 121;
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

        sprite2D.MemberSourceRect = _pelletRect;

        var behavior = sprite2D.SetBehavior<BlPacManPelletBehavior>();
        behavior.Component.SetGlobals(_globals);
        behavior.Initialize(tile);
        _spawnedConsumables.Add(behavior.Component);
    }

   
}
