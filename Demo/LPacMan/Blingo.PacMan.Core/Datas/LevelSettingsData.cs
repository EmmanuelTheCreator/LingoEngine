using System;
using System.Collections.Generic;

namespace Blingo.PacMan.Core.Datas;

/// <summary>
/// Represents the duration of a ghost mode cycle.
/// </summary>
public readonly record struct ModeTiming(GhostMode Mode, TimeSpan Duration);

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

/// <summary>
/// Level-specific configuration for Pac-Man's speed profile.
/// </summary>
public sealed record PacmanSettings(
    float Speed,
    float DotSpeed,
    float FrightenedSpeed,
    float FrightenedDotSpeed);

/// <summary>
/// Cruise Elroy thresholds when ghosts speed up as pellets disappear.
/// </summary>
public sealed record CruiseElroySettings(
    int DotsThreshold1,
    float Speed1,
    int DotsThreshold2,
    float Speed2);

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

/// <summary>
/// Bundles the settings for a particular level.
/// </summary>
public sealed record LevelSettings(
    GameSettings Game,
    PacmanSettings Pacman,
    GhostSettings Ghost);
