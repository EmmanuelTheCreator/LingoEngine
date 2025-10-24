using Blingo.PacMan.Core.Enums;

namespace Blingo.PacMan.Core.Settings;

/// <summary>
/// Represents the duration of a ghost mode cycle.
/// </summary>
public readonly record struct ModeTiming(GhostMode Mode, TimeSpan Duration);
