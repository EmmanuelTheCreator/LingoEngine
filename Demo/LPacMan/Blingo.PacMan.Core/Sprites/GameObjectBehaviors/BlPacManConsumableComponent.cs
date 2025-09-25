using System;
using Blingo.PacMan.Core.Datas;
using Blingo.PacMan.Core.Game;
using BlingoEngine.Sprites;

namespace Blingo.PacMan.Core.Sprites.GameObjectBehaviors;

internal sealed class BlPacManConsumableComponent
{
    private readonly BlingoSpriteBehavior _owner;
    private readonly BlPacManConsumableType _type;
    private readonly int _scoreValue;
    private Tile? _tile;
    private BlPacManAssetContainer? _assets;
    private GlobalVars? _globals;

    public BlPacManConsumableComponent(BlingoSpriteBehavior owner, BlPacManConsumableType type, int scoreValue)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        _type = type;
        _scoreValue = scoreValue;
    }

    public BlPacManConsumableType Type => _type;

    public int ScoreValue => _scoreValue;

    public Tile? Tile => _tile;

    public void SetGlobals(GlobalVars globals)
    {
        _globals = globals ?? throw new ArgumentNullException(nameof(globals));
    }

    public void Initialize(Tile tile, BlPacManAssetContainer assets)
    {
        _tile = tile ?? throw new ArgumentNullException(nameof(tile));
        _assets = assets ?? throw new ArgumentNullException(nameof(assets));
        _tile.Item = this;
        _globals?.State.RegisterConsumableSpawn();
        Show();
    }

    public void Consume(BlPacManActorBehavior pacMan)
    {
        if (_tile is not null)
        {
            _tile.Item = null;
        }

        Hide();
        pacMan.HandleConsumableEaten(this);
    }

    public void Destroy()
    {
        if (_tile is not null)
        {
            _tile.Item = null;
            _tile = null;
        }
        _owner.GetSprite().RemoveMe();
        _assets = null;
    }

    public void Hide()
    {
        _owner.GetSprite().Visibility = false;
    }

    public void Show()
    {
        _owner.GetSprite().Visibility = true;
    }
}
