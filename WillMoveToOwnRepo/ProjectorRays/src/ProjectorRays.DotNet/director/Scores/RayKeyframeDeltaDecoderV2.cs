using ProjectorRays.Common;
using static ProjectorRays.director.Scores.RayScoreTagsV2;

namespace ProjectorRays.director.Scores;

internal static class RayKeyframeDeltaDecoderV2
{
    internal static void ApplyKnownTag(RaySprite sprite, RaysScoreChunk.RayKeyFrame kf, ScoreTagV2 tag, byte[] data, RayScoreParseContext ctx)
    {
        var rs = new ReadStream(data, data.Length, Endianness.BigEndian, annotator: ctx.Annotator);
        int expectedSize = RayScoreTagsV2.GetDataLength(tag);
        if (expectedSize > 0 && data.Length != expectedSize)
            ctx.Logger.LogWarning($"Tag {tag:X4} expected {expectedSize} bytes, got {data.Length}");
        switch (tag)
        {
            case ScoreTagV2.Size:
                kf.Width = rs.ReadInt16("width", ctx.ToDict());
                kf.Height = rs.ReadInt16("height", ctx.ToDict());
                break;
            case ScoreTagV2.Position:
                kf.LocH = rs.ReadInt16("locH", ctx.ToDict());
                kf.LocV = rs.ReadInt16("locV", ctx.ToDict());
                break;
            case ScoreTagV2.Colors:
                kf.ForeColor = rs.ReadUint8("foreColor", ctx.ToDict());
                kf.BackColor = rs.ReadUint8("backColor", ctx.ToDict());
                break;
            case ScoreTagV2.Ink:
                kf.Ink = rs.ReadInt16("ink", ctx.ToDict());
                break;
            case ScoreTagV2.Rotation:
                kf.Rotation = rs.ReadInt16("rot", ctx.ToDict()) / 100f;
                break;
            case ScoreTagV2.Skew:
                kf.Skew = rs.ReadInt16("skew", ctx.ToDict()) / 100f;
                break;
            case ScoreTagV2.Ease:
                sprite.EaseIn = rs.ReadUint8("easeIn", ctx.ToDict());
                sprite.EaseOut = rs.ReadUint8("easeOut", ctx.ToDict());
                break;
            case ScoreTagV2.Curvature:
                sprite.Curvature = rs.ReadUint16("curv", ctx.ToDict());
                break;
            case ScoreTagV2.AdvanceFrame:
                rs.ReadUint16("adv", ctx.ToDict());
                ctx.CurrentFrame++;
                break;
            case ScoreTagV2.Composite:
                kf.Width = rs.ReadInt16("width", ctx.ToDict());
                kf.Height = rs.ReadInt16("height", ctx.ToDict());
                byte blendRaw = rs.ReadUint8("blendRaw", ctx.ToDict());
                kf.Blend = 100 - blendRaw * 100 / 255;
                kf.Ink = rs.ReadUint8("ink", ctx.ToDict());
                break;
            case ScoreTagV2.FrameRect:
                kf.LocH = rs.ReadInt16("locH", ctx.ToDict());
                kf.LocV = rs.ReadInt16("locV", ctx.ToDict());
                kf.Width = rs.ReadInt16("width", ctx.ToDict());
                kf.Height = rs.ReadInt16("height", ctx.ToDict());
                break;
            case ScoreTagV2.Flags:
                rs.ReadUint16("flags", ctx.ToDict());
                break;
            case ScoreTagV2.FlagsControl:
                rs.ReadUint16("flagsCtrl", ctx.ToDict());
                break;
            case ScoreTagV2.TweenFlags:
                sprite.TweenFlags = rs.ReadUint8("tweenFlags", ctx.ToDict());
                break;
            default:
                break;
        }
    }
}
