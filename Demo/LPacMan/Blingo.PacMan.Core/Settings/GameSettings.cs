namespace Blingo.PacMan.Core.Datas;

/// <summary>
/// Level-specific configuration for overall game behaviour.
/// </summary>
public sealed record GameSettings(
    IReadOnlyList<ModeTiming> ModeSequence,
    int BonusIndex,
    int BonusScore,
    IReadOnlyList<string> MapLayout,
    string MazeMemberName,
    int DefaultLives);
