namespace Blingo.PacMan.Core.Settings;

/// <summary>
/// Level-specific configuration for Pac-Man's speed profile.
/// </summary>
public sealed record PacmanSettings(
    float Speed,
    float DotSpeed,
    float FrightenedSpeed,
    float FrightenedDotSpeed);
