namespace BlingoEngine.Legacy.Lingo.Analysis;

/// <summary>
/// Categorizes a handler based on the type of message it responds to.
/// </summary>
public enum BlLingoHandlerKind
{
    /// <summary>
    /// The handler does not match any known system message.
    /// </summary>
    Custom = 0,

    /// <summary>
    /// The handler responds to movie-level lifecycle messages.
    /// </summary>
    Movie = 1,

    /// <summary>
    /// The handler responds to sprite or frame events.
    /// </summary>
    Behavior = 2,
}
