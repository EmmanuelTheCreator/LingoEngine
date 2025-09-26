using Blingo.PacMan.Core.Enums;
using Blingo.PacMan.Core.Game;
using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;
using BlingoEngine.Sprites;

namespace Blingo.PacMan.Core.Sprites.GameObjectBehaviors;

internal sealed class BlPacManPowerPillBehavior : BlingoSpriteBehavior, IHasExitFrameEvent
{
    private readonly BlPacManConsumableComponent _component;
    private readonly GlobalVars _globalVars;
    private int _blinkMax = 20;
    private int _blinkCurrent = 0;
    private bool _lastVisibleState;

    public BlPacManPowerPillBehavior(IBlingoMovieEnvironment env, GlobalVars globalVars)
        : base(env)
    {
        _component = new BlPacManConsumableComponent(this, BlPacManConsumableType.PowerPill, 50);
        _globalVars = globalVars;
    }

    public BlPacManConsumableComponent Component => _component;

    public void Initialize(Tile tile)
    {
        _component.Initialize(tile);
        _blinkCurrent = Random(10);
    }

    public void Consume(BlPacManActorBehavior pacMan)
    {
        _component.Consume(pacMan);
    }

    public void ExitFrame()
    {
        if (!_globalVars.State.IsActivePlaying) return;
        _blinkCurrent++;
        if (_blinkCurrent >= _blinkMax)
        {
            _blinkCurrent = 0;
            _lastVisibleState = !_lastVisibleState;
            Me.Blend = _lastVisibleState ? 100 : 0;
            _blinkMax = _lastVisibleState ? 15 : 10;
        }
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
