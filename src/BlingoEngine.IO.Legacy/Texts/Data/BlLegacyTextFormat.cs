using System;

namespace BlingoEngine.IO.Legacy.Texts.Data;

/// <summary>
/// Describes the byte layout stored inside a text cast payload. Director encodes styled text
/// either as <c>XMED</c> resources or plain <c>STXT</c> streams depending on the movie version.
/// Consumers can inspect this value to decide how the <see cref="BlLegacyText.Bytes"/> buffer
/// should be interpreted.
/// </summary>
public enum BlLegacyTextFormatKind
{
    /// <summary>
    /// Format could not be determined from the resource tag.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Styled text stored inside an <c>XMED</c> chunk.
    /// </summary>
    Xmed,

    /// <summary>
    /// Plain text stored in an <c>STXT</c> chunk used by older Director releases.
    /// </summary>
    Stxt
}
