namespace Blingo.PacMan.Core.Datas;

/// <summary>
/// Represents the duration of a ghost mode cycle.
/// </summary>
public readonly record struct ModeTiming(GhostMode Mode, TimeSpan Duration);
