namespace BlingoEngine.IO.Legacy.Cast.Data;

/// <summary>
/// Enumerates the legacy cast-member types encoded at the start of the <c>CASt</c> payload.
/// </summary>
public enum BlLegacyCastMemberType
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


public static class BlLegacyCastMemberTypeHelpers
{
    public static BlLegacyCastMemberType MapMemberType(uint value)
    {
        return value switch
        {
            0 => BlLegacyCastMemberType.Null,
            1 => BlLegacyCastMemberType.Bitmap,
            2 => BlLegacyCastMemberType.FilmLoop,
            3 => BlLegacyCastMemberType.Text,
            4 => BlLegacyCastMemberType.Palette,
            5 => BlLegacyCastMemberType.Picture,
            6 => BlLegacyCastMemberType.Sound,
            7 => BlLegacyCastMemberType.Button,
            8 => BlLegacyCastMemberType.Shape,
            9 => BlLegacyCastMemberType.Movie,
            10 => BlLegacyCastMemberType.DigitalVideo,
            11 => BlLegacyCastMemberType.Script,
            12 => BlLegacyCastMemberType.Rte,
            13 => BlLegacyCastMemberType.Font,
            14 => BlLegacyCastMemberType.Xtra,
            15 => BlLegacyCastMemberType.Field,
            _ => BlLegacyCastMemberType.Unknown
        };
    }
}
