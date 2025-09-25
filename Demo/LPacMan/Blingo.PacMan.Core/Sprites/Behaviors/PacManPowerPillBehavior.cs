using Blingo.PacMan.Core.Game;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;

namespace Blingo.PacMan.Core.Sprites.Behaviors;

internal sealed class PacManPowerPillBehavior : BlingoSpriteBehavior
{
    private readonly PacManConsumableComponent _component;

    public PacManPowerPillBehavior(IBlingoMovieEnvironment env)
        : base(env)
    {
        _component = new PacManConsumableComponent(this, PacManConsumableType.PowerPill, 50);
    }

    public PacManConsumableComponent Component => _component;

    public void Initialize(Tile tile, PacManGameBehavior coordinator)
    {
        _component.Initialize(tile, coordinator);
    }

    public void Consume(PacManActorBehavior pacMan)
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
