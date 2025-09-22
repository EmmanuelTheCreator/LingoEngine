using System;

namespace BlingoEngine.Net.RNetHost.Common;

/// <summary>
/// Helpers for converting between related enum types without duplicating parsing logic.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Attempts to convert an enum value to another enum type, returning <paramref name="fallback"/> when conversion fails.
    /// </summary>
    public static TTarget ConvertTo<TTarget>(this Enum value, TTarget fallback)
        where TTarget : struct, Enum
        => Enum.TryParse(value.ToString(), out TTarget parsed)
            ? parsed
            : fallback;

    /// <summary>
    /// Attempts to convert an enum value to another enum type, returning that type's default value on failure.
    /// </summary>
    public static TTarget ConvertTo<TTarget>(this Enum value)
        where TTarget : struct, Enum
        => value.ConvertTo(default(TTarget));
}
