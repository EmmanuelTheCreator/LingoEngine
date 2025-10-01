using AbstUI.Primitives;
using Blingo.PacMan.Core.Sprites.GameObjectBehaviors;
using BlingoEngine.Bitmaps;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;

namespace Blingo.PacMan.Core.Game;

internal sealed class PMPelletManager
{
    // image is 24 x 6
    private static readonly ARect _pelletRect = ARect.New(6, 0, 2, 2);

    private readonly List<PMConsumableComponent> _spawnedConsumables = new();
    private readonly GlobalVars _globals;

    public PMPelletManager(GlobalVars globals)
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
        var pellets = _globals.Map.Pellets.ToList();
        for (int i = 0; i < pellets.Count; i++)
        {
            PMTile? tile = pellets[i];
            var behavior = CreateConsumableSprite(lingoMovie, tile , i);
            _spawnedConsumables.Add(behavior.Component);
        }
    }

    private PMPelletBehavior CreateConsumableSprite(IBlingoMovie lingoMovie, PMTile tile,  int index)
    {
        var name = $"Pellet_{tile.Column}_{tile.Row}";
        var spriteNum = index + PCSpriteNums.PelletsStart;
        lingoMovie.Channel(spriteNum).Puppet = true;
        BlingoSprite2D sprite2D = (BlingoSprite2D)lingoMovie.GetSprite(spriteNum)!;
        sprite2D.LocH = tile.X;
        sprite2D.LocV = tile.Y;
        sprite2D.SetMember("pills");
        sprite2D.SetMemberRect(_pelletRect);

        var behavior = sprite2D.SetBehavior<PMPelletBehavior>();
        behavior.Component.SetGlobals(_globals);
        behavior.Initialize(tile);
        return behavior;
    }

   
}
