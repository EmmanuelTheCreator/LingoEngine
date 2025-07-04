using System;

namespace ProjectorRays.director.Scores;

/// <summary>
/// Tag definitions used in Director score keyframes.
/// Sizes are based on observations in docs/director_keyframe_tags.md
/// </summary>
internal static class RayScoreTagsV2
{
    // Prefix constants used by the score parser
    public const ushort BlockEnd = 0x0008;
    public const ushort SpriteBlockPrefix = 0x0030;

    // Flags observed in advance tags (0x0136)
    public const ushort KeyframeCreateFlag = 0x8100;
    public const ushort KeyframeNoCreateFlag = 0x0100;
    public enum ScoreTagV2 : ushort
    {
        Size = 0x0130,
        Position = 0x015C,
        Ease = 0x0120,
        AdvanceFrame = 0x0136,
        PathPart = 0x0166,
        Ink = 0x0196,
        Rotation = 0x019E,
        Skew = 0x01A2,
        Colors = 0x0212,
        Composite = 0x0190,
        BlockControl = 0x0180,
        FrameRect = 0x01EC,
        Curvature = 0x01F4,
        Flags = 0x01FE,
        FlagsControl = 0x01FC,
        TweenFlags = 0x01F6,

       
    }
    public enum ScoreTagMain : ushort
    {
        NextByteIsSpriteIf10Hex = 0x0120, // followed by   0x0120 , SpriteLock.dir
        ThisIsASpriteBlock = 0x1000, // followed by 0x0030, SpriteBlock.dir
    }

    public static ScoreTagMain? TryParseTagMain(ushort raw)
    {
        object boxed = raw;
        return Enum.IsDefined(typeof(ScoreTagMain), boxed) ? (ScoreTagMain?)raw : null;
    }

    public static ScoreTagV2? TryParseTag(ushort raw)
    {
        object boxed = raw;
        return Enum.IsDefined(typeof(ScoreTagV2), boxed) ? (ScoreTagV2?)raw : null;
    }

    public static int GetDataLength(ScoreTagV2 tag)
        => tag switch
        {
            ScoreTagV2.Size => 4,
            ScoreTagV2.Position => 4,
            ScoreTagV2.Ease => 2,
            ScoreTagV2.AdvanceFrame => 2,
            ScoreTagV2.PathPart => 2,
            ScoreTagV2.Ink => 2,
            ScoreTagV2.Rotation => 2,
            ScoreTagV2.Skew => 2,
            ScoreTagV2.Colors => 2,
            ScoreTagV2.Composite => 6,
            ScoreTagV2.FrameRect => 8,
            ScoreTagV2.Curvature => 2,
            ScoreTagV2.Flags => 2,
            ScoreTagV2.FlagsControl => 2,
            ScoreTagV2.TweenFlags => 1,
            ScoreTagV2.BlockControl => 2,
            _ => 0
        };
}
