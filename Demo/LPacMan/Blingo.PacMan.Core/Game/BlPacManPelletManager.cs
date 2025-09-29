using AbstUI.Primitives;
using Blingo.PacMan.Core.Sprites.GameObjectBehaviors;
using BlingoEngine.Bitmaps;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;

namespace Blingo.PacMan.Core.Game;

internal sealed class BlPacManPelletManager
{
    // image is 24 x 6
    private static readonly ARect _pelletRect = ARect.New(6, 0, 2, 2);

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
            var behavior = CreateConsumableSprite(lingoMovie, tile , i);
            _spawnedConsumables.Add(behavior.Component);
        }
    }

    private BlPacManPelletBehavior CreateConsumableSprite(IBlingoMovie lingoMovie, Tile tile,  int index)
    {
        var name = $"Pellet_{tile.Column}_{tile.Row}";
        var spriteNum = index + PCSpriteNums.PelletsStart;
        lingoMovie.Channel(spriteNum).Puppet = true;
        BlingoSprite2D sprite2D = (BlingoSprite2D)lingoMovie.GetSprite(spriteNum)!;
        sprite2D.LocH = tile.CenterX;
        sprite2D.LocV = tile.CenterY;
        sprite2D.SetMember("pills");
        sprite2D.SetMemberRect(_pelletRect);

        var behavior = sprite2D.SetBehavior<BlPacManPelletBehavior>();
        behavior.Component.SetGlobals(_globals);
        behavior.Initialize(tile);
        return behavior;
    }

   
}
