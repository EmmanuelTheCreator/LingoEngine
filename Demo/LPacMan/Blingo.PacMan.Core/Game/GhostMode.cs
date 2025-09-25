namespace Blingo.PacMan.Core.Game;

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
