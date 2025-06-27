using LingoEngine.Movies.Events;
using LingoEngine.Sprites.Events;

namespace LingoEngine.Sprites
{
    /// <summary>
    /// Represents an internal sprite actor that participates in the begin/step/end sprite lifecycle.
    /// </summary>
    internal interface IPlayableActor : IHasBeginSpriteEvent, IHasStepFrameEvent, IHasEndSpriteEvent
    {
    }
}
