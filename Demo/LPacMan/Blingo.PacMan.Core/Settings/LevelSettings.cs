namespace Blingo.PacMan.Core.Datas;

/// <summary>
/// Bundles the settings for a particular level.
/// </summary>
public sealed record LevelSettings(
    GameSettings Game,
    PacmanSettings Pacman,
    GhostSettings Ghost);
