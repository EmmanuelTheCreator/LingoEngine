namespace BlingoEngine.Movies.Events
{
    /// <summary>
    /// Receives idle ticks between <c>enterFrame</c> and <c>exitFrame</c>.
    /// </summary>
    public interface IHasIdleFrameEvent
    {
        /// <summary>Invoked while the playhead is waiting inside a frame.</summary>
        void IdleFrame();
    }
}
