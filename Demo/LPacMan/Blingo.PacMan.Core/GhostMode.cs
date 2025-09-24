namespace Blingo.PacMan.Core;

/// <summary>
/// Represents the high-level behavioural state a ghost can assume.
/// </summary>
public enum GhostMode
{
    Scatter,
    Chase,
    Frightened,
    House,
    Dead,
}
