using Blingo.PacMan.Core.Enums;
using Blingo.PacMan.Core.Game;
using BlingoEngine.Sprites;

namespace Blingo.PacMan.Core.Sprites.GameObjectBehaviors;

internal sealed class BlPacManConsumableComponent
{
    private readonly BlingoSpriteBehavior _owner;
    private readonly BlPacManConsumableType _type;
    private readonly int _scoreValue;
    private Tile? _tile;
    private GlobalVars? _globals;

    public BlPacManConsumableComponent(BlingoSpriteBehavior owner, BlPacManConsumableType type, int scoreValue)
    {
        _owner = owner;
        _type = type;
        _scoreValue = scoreValue;
    }

    public BlPacManConsumableType Type => _type;

    public int ScoreValue => _scoreValue;

    public Tile? Tile => _tile;

    public void SetGlobals(GlobalVars globals)
    {
        _globals = globals;
    }

    public void Initialize(Tile tile)
    {
        _tile = tile;
        _tile.Item = this;
        _globals?.State.RegisterConsumableSpawn();
        Show();
    }

    public void Consume(BlPacManActorBehavior pacMan)
    {
        if (_tile is not null)
            _tile.Item = null;

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
        _owner.GetSprite().Puppet = false;
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
