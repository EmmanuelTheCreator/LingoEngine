namespace BlingoEngine.Legacy.Lingo.Analysis;

/// <summary>
/// Describes the high-level category of Lingo script represented by a class scope.
/// </summary>
public enum BlLingoScriptKind
{
    /// <summary>
    /// The script kind could not be determined from the analyzed source.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The script behaves like a sprite or frame behavior.
    /// </summary>
    Behavior = 1,

    /// <summary>
    /// The script handles movie-level lifecycle messages.
    /// </summary>
    Movie = 2,

    /// <summary>
    /// The script represents a parent script that creates child objects.
    /// </summary>
    Parent = 3,
}
