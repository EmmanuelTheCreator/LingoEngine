
namespace ProjectorRays.director.Scores.Data;


/// <summary>
/// Defines flags for keyframe properties in Director's sprite and animation system.
/// These flags correspond to properties like position, size, rotation, color, tweening, and more.
/// </summary>
[Flags]
public enum RayKeyframeEnabled : int
{
    /// <summary>Indicates no properties are enabled.</summary>
    None = 0,

    /// <summary>Indicates tweening is enabled.</summary>
    TweeningEnabled = 1 << 0,

    /// <summary>Indicates position (LocH + LocV) is enabled.</summary>
    Path = 1 << 1,

    /// <summary>Indicates size (Width + Height) is enabled.</summary>
    Size = 1 << 2,

    /// <summary>Indicates rotation is enabled.</summary>
    Rotation = 1 << 3,

    /// <summary>Indicates skew is enabled.</summary>
    Skew = 1 << 4,

    /// <summary>Indicates blend is enabled.</summary>
    Blend = 1 << 5,

    /// <summary>Indicates fore color is enabled.</summary>
    ForeColor = 1 << 6,        // ForeColor (2 bytes)

    /// <summary>Indicates back color is enabled.</summary>
    BackColor = 1 << 7,        // BackColor (2 bytes)

    /// <summary>Indicates continuous tweening at the endpoint.</summary>
    ContinuousAtEndpoint = 1 << 8, // For Continuous checkbox

    /// <summary>Indicates speed (used in tweening).</summary>
    Speed = 1 << 9,              // For Speed setting (1 bit)

    /// <summary>Indicates ease-in for the tweening.</summary>
    EaseIn = 1 << 10,            // Ease-in (1 byte)

    /// <summary>Indicates ease-out for the tweening.</summary>
    EaseOut = 1 << 11,           // Ease-out (1 byte)

    /// <summary>Indicates curvature for the tweening.</summary>
    Curvature = 1 << 12          // Curvature (1 byte)
}

public class RayScoreKeyFrame
{
    public RayKeyframeEnabled EnabledProperties { get; set; } = RayKeyframeEnabled.None;
    public int LocH { get; set; }
    public int LocV { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public float Rotation { get; set; }
    public float Skew { get; set; }
    public int Blend { get; set; }
    public int Ink { get; set; }
    public int ForeColor { get; set; }
    public int BackColor { get; set; }
    public int MemberCastLib { get; set; }
    public int MemberNum { get; set; }
    public int Duration { get; set; }
    /// <summary>Absolute frame this keyframe applies to.</summary>
    public int FrameNum { get; set; }
    public int SpriteNum { get; set; }
    public float ScaleX { get; internal set; }
    public float ScaleY { get; internal set; }
    public List<UnknownTag> UnknownTags { get; } = new();
}
