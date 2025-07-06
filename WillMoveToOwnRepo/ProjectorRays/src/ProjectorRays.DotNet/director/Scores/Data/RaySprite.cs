using static ProjectorRays.director.Scores.RaysScoreChunk;

namespace ProjectorRays.director.Scores.Data;



/// <summary>
/// Flags used in Director sprite control byte (Flip/Edit/Trail/Lock/etc).
/// </summary>
[Flags]
internal enum RaySpriteFlags : byte
{
    None = 0,

    /// <summary>Sprite is flipped horizontally.</summary>
    FlipH = 1 << 0, // 0x01

    /// <summary>Sprite is flipped vertically.</summary>
    FlipV = 1 << 6, // 0x40

    /// <summary>Sprite is editable at runtime (typically for text sprites).</summary>
    Editable = 1 << 5, // 0x20

    /// <summary>Sprite is moveable during playback.</summary>
    Moveable = 1 << 4, // 0x10

    /// <summary>Sprite leaves a trail behind while moving.</summary>
    Trails = 1 << 3, // 0x08

    /// <summary>Sprite is locked and cannot be edited.</summary>
    Locked = 1 << 2  // 0x04
}

/// <summary>Unknown tag entry encountered during keyframe parsing.</summary>
public record UnknownTag(ushort Tag, byte[] Data);
/// <summary>Descriptor of a sprite on the score timeline.</summary>
public class RaySprite
{
    /// <summary>Cast member displayed by this sprite.</summary>
    /// <summary>Offset to sprite property table for behaviors.</summary>
    public int StartFrame { get; internal set; }
    public int EndFrame { get; internal set; }
    public int SpriteNumber { get; internal set; }
    public int MemberCastLib { get; set; }
    public int MemberNum { get; set; }
    public int SpritePropertiesOffset { get; internal set; }
    public int LocH { get; internal set; }
    public int LocV { get; internal set; }
    public int Width { get; internal set; }
    public int Height { get; internal set; }
    public float Rotation { get; internal set; }
    public float Skew { get; internal set; }
    public int Ink { get; internal set; }
    public int ForeColor { get; internal set; }
    public int BackColor { get; internal set; }
    public int ScoreColor { get; internal set; }
    public int Blend { get; internal set; }
    public bool FlipH { get; internal set; }
    public bool FlipV { get; internal set; }
    public bool Editable { get; internal set; }
    public bool Moveable { get; internal set; }
    public bool Trails { get; internal set; }
    public bool IsLocked { get; internal set; }

    public byte EaseIn { get; set; }
    public byte EaseOut { get; set; }
    public ushort Curvature { get; set; }
    public RayTweenFlags TweenFlags { get; set; }

    public List<RaysBehaviourRef> Behaviors { get; internal set; } = new();
    public List<RayScoreKeyFrame> Keyframes { get; internal set; } = new();
    public int LocZ { get; set; }
    public List<int> ExtraValues { get; internal set; }
}