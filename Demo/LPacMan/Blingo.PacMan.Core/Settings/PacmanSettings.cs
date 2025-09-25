namespace Blingo.PacMan.Core.Datas;

/// <summary>
/// Level-specific configuration for Pac-Man's speed profile.
/// </summary>
public sealed record PacmanSettings(
    float Speed,
    float DotSpeed,
    float FrightenedSpeed,
    float FrightenedDotSpeed);
