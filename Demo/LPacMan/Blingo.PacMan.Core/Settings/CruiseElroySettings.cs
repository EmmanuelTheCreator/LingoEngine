namespace Blingo.PacMan.Core.Settings;

/// <summary>
/// Cruise Elroy thresholds when ghosts speed up as pellets disappear.
/// </summary>
public sealed record CruiseElroySettings(
    int DotsThreshold1,
    float Speed1,
    int DotsThreshold2,
    float Speed2);
