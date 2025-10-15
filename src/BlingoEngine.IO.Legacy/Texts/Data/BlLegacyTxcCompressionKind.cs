using System;

namespace BlingoEngine.IO.Legacy.Texts.Data;

/// <summary>
/// Represents the compression scheme applied to the pixel payload inside a legacy
/// <c>TXc</c> resource. Director stores pre-rendered text bitmaps using several simple
/// run-length encodings, so the reader exposes the detected mode to help downstream
/// callers decide how to interpret the <see cref="BlLegacyTxcImage.Pixels"/> buffer.
/// </summary>
public enum BlLegacyTxcCompressionKind
{
    /// <summary>Compression could not be determined.</summary>
    Unknown = 0,

    /// <summary>No compression – pixels are stored as raw indices.</summary>
    None = 1,

    /// <summary>Simple <c>{length, value}</c> run-length encoding.</summary>
    RlePairs = 2,

    /// <summary>Apple PackBits encoding.</summary>
    PackBits = 3
}
