using BlingoEngine.Movies;
using BlingoEngine.Movies.Events;
using BlingoEngine.Sprites;

namespace Blingo.PacMan.Core.Sprites.GameObjectBehaviors;

/// <summary>
/// Simple frame behaviour that keeps the playhead anchored on the current frame so Pac-Man can manage playback manually.
/// </summary>
internal sealed class PMStayOnFrameBehavior : BlingoSpriteBehavior, IHasEnterFrameEvent
{
    public PMStayOnFrameBehavior(IBlingoMovieEnvironment env)
        : base(env)
    {
    }

    public void EnterFrame()
    {
        _Movie.GoTo(_Movie.Frame);
    }
}
