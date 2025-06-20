
using LingoEngine.Movies;

namespace LingoEngine.FrameworkCommunication
{
    /// <summary>
    /// Represents the top-level window or stage. Implementations update the
    /// display when the active <see cref="LingoMovie"/> changes.
    /// </summary>
    public interface ILingoFrameworkStage
    {
        /// <summary>Sets the currently active movie.</summary>
        void SetActiveMovie(LingoMovie? lingoMovie);
        float Scale { get; set; }
    }
}
