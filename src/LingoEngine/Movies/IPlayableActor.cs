namespace LingoEngine.Movies
{
    /// <summary>
    /// Represents an internal sprite actor that participates in the begin/step/end sprite lifecycle.
    /// </summary>
    internal interface IPlayableActor : Events.IHasBeginSpriteEvent, Events.IHasStepFrameEvent, Events.IHasEndSpriteEvent
    {
    }
}
