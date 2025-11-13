namespace BlingoEngine.IO.Legacy.Cast.Data;

/// <summary>
/// Enumerates the legacy cast-member types encoded at the start of the <c>CASt</c> payload.
/// </summary>
public enum BlCastRawMemberType
{
    /// <summary>
    /// Type code returned when the loader cannot map the stored identifier.
    /// </summary>
    Unknown = -1,

    /// <summary>
    /// Placeholder slot that contains no cast data.
    /// </summary>
    Null = 0,

    /// <summary>
    /// Bitmap member that resolves to <c>BITD</c>, <c>DIB </c>, or authoring metadata. See docs/LegacyBitmapLoading.md.
    /// </summary>
    Bitmap = 1,

    /// <summary>
    /// Film loop that replays a timeline sequence embedded in the cast library.
    /// </summary>
    FilmLoop = 2,

    /// <summary>
    /// Static text member described in docs/LegacyTextFieldMembers.md.
    /// </summary>
    Text = 3,

    /// <summary>
    /// Palette entry that provides colour tables for bitmap members.
    /// </summary>
    Palette = 4,

    /// <summary>
    /// Picture member, typically QuickDraw <c>PICT</c> drawings.
    /// </summary>
    Picture = 5,

    /// <summary>
    /// Sound member stored as <c>ediM</c>, <c>sndS</c>, or classic <c>SND </c> bytes. See docs/LegacySoundLoading.md.
    /// </summary>
    Sound = 6,

    /// <summary>
    /// Button member that combines bitmaps with interaction states.
    /// </summary>
    Button = 7,

    /// <summary>
    /// QuickDraw shape member documented in docs/LegacyShapeRecords.md.
    /// </summary>
    Shape = 8,

    /// <summary>
    /// Linked movie asset (commonly QuickTime clips).
    /// </summary>
    Movie = 9,

    /// <summary>
    /// Digital video member that wraps platform-specific codecs.
    /// </summary>
    DigitalVideo = 10,

    /// <summary>
    /// Lingo script member. See docs/LegacyScriptMembers.md.
    /// </summary>
    Script = 11,

    /// <summary>
    /// Rich-text edit field member introduced in later Director releases.
    /// </summary>
    Rte = 12,

    /// <summary>
    /// Embedded font resource registered inside the cast.
    /// </summary>
    Font = 13,

    /// <summary>
    /// External Xtra component (plug-in) entry.
    /// </summary>
    Xtra = 14,

    /// <summary>
    /// Editable text field documented in docs/LegacyTextFieldMembers.md.
    /// </summary>
    Field = 15
}


public static class BlCastRawMemberTypeHelpers
{
    public static BlCastRawMemberType MapMemberType(uint value)
    {
        return value switch
        {
            0 => BlCastRawMemberType.Null,
            1 => BlCastRawMemberType.Bitmap,
            2 => BlCastRawMemberType.FilmLoop,
            3 => BlCastRawMemberType.Text,
            4 => BlCastRawMemberType.Palette,
            5 => BlCastRawMemberType.Picture,
            6 => BlCastRawMemberType.Sound,
            7 => BlCastRawMemberType.Button,
            8 => BlCastRawMemberType.Shape,
            9 => BlCastRawMemberType.Movie,
            10 => BlCastRawMemberType.DigitalVideo,
            11 => BlCastRawMemberType.Script,
            12 => BlCastRawMemberType.Rte,
            13 => BlCastRawMemberType.Font,
            14 => BlCastRawMemberType.Xtra,
            15 => BlCastRawMemberType.Field,
            _ => BlCastRawMemberType.Unknown
        };
    }
}
