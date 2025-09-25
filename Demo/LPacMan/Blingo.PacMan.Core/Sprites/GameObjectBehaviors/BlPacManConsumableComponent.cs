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
    private BlPacManGameBehavior? _coordinator;

    public BlPacManConsumableComponent(BlingoSpriteBehavior owner, BlPacManConsumableType type, int scoreValue)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        _type = type;
        _scoreValue = scoreValue;
    }

    public BlPacManConsumableType Type => _type;

    public int ScoreValue => _scoreValue;

    public Tile? Tile => _tile;

    public void Initialize(Tile tile, BlPacManGameBehavior coordinator)
    {
        _tile = tile ?? throw new ArgumentNullException(nameof(tile));
        _coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
        _tile.Item = this;
        _coordinator.RegisterConsumable(this);
        Show();
    }

    public void Consume(BlPacManActorBehavior pacMan)
    {
        if (_tile is not null)
        {
            _tile.Item = null;
        }

        Hide();
        _coordinator?.UnregisterConsumable(this);
        _coordinator?.NotifyConsumableEaten(this);
    }

    public void Destroy()
    {
        _coordinator?.UnregisterConsumable(this);
        if (_tile is not null)
        {
            _tile.Item = null;
            _tile = null;
        }
        _owner.GetSprite().RemoveMe();
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
