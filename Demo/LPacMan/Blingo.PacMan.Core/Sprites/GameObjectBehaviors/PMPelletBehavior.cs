using Blingo.PacMan.Core.Enums;
using Blingo.PacMan.Core.Game;
using BlingoEngine.Movies;
using BlingoEngine.Sprites;

namespace Blingo.PacMan.Core.Sprites.GameObjectBehaviors;

internal sealed class PMPelletBehavior : BlingoSpriteBehavior
{
    private readonly PMConsumableComponent _component;

    public PMPelletBehavior(IBlingoMovieEnvironment env)
        : base(env)
    {
        _component = new PMConsumableComponent(this, BlPacManConsumableType.Pellet, 10);
    }

    public PMConsumableComponent Component => _component;

    public void Initialize(PMTile tile)
    {
        _component.Initialize(tile);
    }

    public void Consume(PMPacManActorBehavior pacMan)
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
