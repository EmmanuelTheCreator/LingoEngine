using LingoEngine.FilmLoops;
using LingoEngine.Members;

namespace LingoEngine.Pictures
{
    /// <summary>
    /// Framework specific implementation details for a film loop member.
    /// </summary>
    public interface ILingoFrameworkMemberFilmLoop : ILingoFrameworkMember
    {
        /// <summary>
        /// Raw data representing the film loop media. The exact format is framework dependent.
        /// </summary>
        byte[]? Media { get; set; }

        /// <summary>
        /// Determines how the film loop should be framed within its sprite rectangle.
        /// Mirrors the Lingo <c>framing</c> property with values <c>#crop</c>, <c>#scale</c>, or <c>#auto</c>.
        /// </summary>
        LingoFilmLoopFraming Framing { get; set; }

        /// <summary>
        /// Controls whether playback should loop when the last frame is reached.
        /// Corresponds to the Lingo <c>loop</c> property used with film loop sprites.
        /// </summary>
        bool Loop { get; set; }
    }
}
