using System;

namespace BlingoEngine.IO.Legacy.Scripts;

/// <summary>
/// Enumerates the legacy Lingo script categories encoded inside the <c>CASt</c>
/// specific data block. Director stores a 16-bit selector whose low byte
/// distinguishes behaviour, movie, and parent scripts.
/// </summary>
internal enum BlLegacyScriptFormatKind
{
    /// <summary>
    /// The selector could not be resolved to a known script category.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Behaviour (score) script identified by selector code <c>0x01</c>.
    /// </summary>
    Behavior = 1,

    /// <summary>
    /// Movie script that listens for global events (<c>0x03</c> selector).
    /// </summary>
    Movie = 3,

    /// <summary>
    /// Parent script that defines factory prototypes (<c>0x07</c> selector).
    /// </summary>
    Parent = 7
}

/// <summary>
/// Provides helpers for interpreting script selectors stored alongside
/// compiled <c>Lscr</c> resources.
/// </summary>
internal static class BlLegacyScriptFormat
{
    /// <summary>
    /// Attempts to resolve the script category from the raw selector bytes.
    /// Director writes a 16-bit value where the active code normally lives in the
    /// low byte while the high byte stays at zero. Both big- and little-endian
    /// encodings are tolerated.
    /// </summary>
    public static BlLegacyScriptFormatKind Detect(ReadOnlySpan<byte> selectorData)
    {
        if (selectorData.IsEmpty)
        {
            return BlLegacyScriptFormatKind.Unknown;
        }

        var code = ExtractSelector(selectorData);
        return MapSelector(code);
    }

    /// <summary>
    /// Maps the selector byte to a script category.
    /// </summary>
    public static BlLegacyScriptFormatKind MapSelector(byte selector)
    {
        return selector switch
        {
            0x01 => BlLegacyScriptFormatKind.Behavior,
            0x03 => BlLegacyScriptFormatKind.Movie,
            0x07 => BlLegacyScriptFormatKind.Parent,
            _ => BlLegacyScriptFormatKind.Unknown
        };
    }

    private static byte ExtractSelector(ReadOnlySpan<byte> data)
    {
        if (data.Length >= 2)
        {
            var first = data[0];
            var second = data[1];

            if (first == 0 && second != 0)
                return second;

            if (second == 0 && first != 0)
                return first;

            if (first == second && first != 0)
                return first;

            if (first != 0)
                return first;

            if (second != 0)
                return second;
        }

        foreach (var candidate in data)
        {
            if (candidate != 0)
                return candidate;
        }

        return data.Length > 0 ? data[0] : (byte)0;
    }
}
