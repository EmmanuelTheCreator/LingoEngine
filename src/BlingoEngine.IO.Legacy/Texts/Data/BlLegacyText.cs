using System;

namespace BlingoEngine.IO.Legacy.Texts.Data;

/// <summary>
/// Represents the decoded payload of a legacy Director text resource. The reader exposes both the
/// detected container format and the raw payload bytes so higher layers can experiment with the
/// embedded styling or plain text streams without re-reading the archive.
/// </summary>
public sealed class BlLegacyText
{
    /// <summary>
    /// Gets the resource identifier assigned to the text entry inside the resource table.
    /// </summary>
    public int ResourceId { get; }

    /// <summary>
    /// Gets the detected text container format such as <c>XMED</c> or <c>STXT</c>.
    /// </summary>
    public BlLegacyTextFormatKind Format { get; }

    /// <summary>
    /// Gets the raw bytes copied from the text resource. Layout tables for historical Director
    /// versions live in <c>BlingoEngine.IO.Legacy/docs/LegacyTextFieldMembers.md</c> to help decode this payload.
    /// </summary>
    public byte[] Bytes { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BlLegacyText"/> class.
    /// </summary>
    public BlLegacyText(int resourceId, BlLegacyTextFormatKind format, byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);

        ResourceId = resourceId;
        Format = format;
        Bytes = bytes;
    }
}
