namespace Blingo.PacMan.Core.Datas;

/// <summary>
/// Ghost-related level settings.
/// </summary>
public sealed record GhostSettings(
    float Speed,
    float TunnelSpeed,
    CruiseElroySettings CruiseElroy,
    float FrightenedSpeed,
    TimeSpan FrightenedDuration,
    int FrightenedFlashes);
