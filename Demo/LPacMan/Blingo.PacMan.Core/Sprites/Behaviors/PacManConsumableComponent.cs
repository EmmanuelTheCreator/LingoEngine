using System;
using Blingo.PacMan.Core.Game;
using BlingoEngine.Sprites;

namespace Blingo.PacMan.Core.Sprites.Behaviors;

internal sealed class PacManConsumableComponent
{
    private readonly BlingoSpriteBehavior _owner;
    private readonly PacManConsumableType _type;
    private readonly int _scoreValue;
    private Tile? _tile;
    private PacManGameBehavior? _coordinator;

    public PacManConsumableComponent(BlingoSpriteBehavior owner, PacManConsumableType type, int scoreValue)
    {
        _owner = owner ?? throw new ArgumentNullException(nameof(owner));
        _type = type;
        _scoreValue = scoreValue;
    }

    public PacManConsumableType Type => _type;

    public int ScoreValue => _scoreValue;

    public Tile? Tile => _tile;

    public void Initialize(Tile tile, PacManGameBehavior coordinator)
    {
        _tile = tile ?? throw new ArgumentNullException(nameof(tile));
        _coordinator = coordinator ?? throw new ArgumentNullException(nameof(coordinator));
        _tile.Item = this;
        _coordinator.RegisterConsumable(this);
        Show();
    }

    public void Consume(PacManActorBehavior pacMan)
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
