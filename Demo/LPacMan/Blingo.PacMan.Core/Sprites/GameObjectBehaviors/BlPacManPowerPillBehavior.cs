using Blingo.PacMan.Core.Datas;
using Blingo.PacMan.Core.Game;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;

namespace Blingo.PacMan.Core.Sprites.GameObjectBehaviors;

internal sealed class BlPacManPowerPillBehavior : BlingoSpriteBehavior
{
    private readonly BlPacManConsumableComponent _component;

    public BlPacManPowerPillBehavior(IBlingoMovieEnvironment env)
        : base(env)
    {
        _component = new BlPacManConsumableComponent(this, BlPacManConsumableType.PowerPill, 50);
    }

    public BlPacManConsumableComponent Component => _component;

    public void Initialize(Tile tile)
    {
        _component.Initialize(tile);
    }

    public void Consume(BlPacManActorBehavior pacMan)
    {
        _component.Consume(pacMan);
    }

    public void Destroy()
    {
        _component.Destroy();
    }

    public void Hide()
    {
        _component.Hide();
    }

    public void Show()
    {
        _component.Show();
    }
}
