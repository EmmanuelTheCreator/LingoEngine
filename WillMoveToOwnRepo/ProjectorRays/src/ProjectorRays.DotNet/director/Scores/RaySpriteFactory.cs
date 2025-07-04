using ProjectorRays.director.Scores;
using static ProjectorRays.director.Scores.RaysScoreChunk;

namespace ProjectorRays.director.Scores;

internal static class RaySpriteFactory
{
    internal static RaySprite CreateSpriteFromBlock(RayKeyframeBlock block, RaysScoreReader.IntervalDescriptor desc)
    {
        return new RaySprite
        {
            StartFrame = desc.StartFrame,
            EndFrame = desc.EndFrame,
            SpriteNumber = desc.Channel,
            LocH = block.LocH,
            LocV = block.LocV,
            Width = block.Width,
            Height = block.Height,
            Rotation = block.Rotation / 100f,
            Skew = block.Padding2 / 100f,
            Blend = 100 - block.BlendRaw * 100 / 255,
            ForeColor = block.ForeColor,
            BackColor = block.BackColor,
            Ink = block.Padding1
        };
    }

    internal static RayKeyFrame CreateKeyFrame(RaySprite sprite, int frame)
    {
        return new RayKeyFrame
        {
            Frame = frame,
            LocH = sprite.LocH,
            LocV = sprite.LocV,
            Width = sprite.Width,
            Height = sprite.Height,
            Rotation = sprite.Rotation,
            Skew = sprite.Skew,
            Blend = sprite.Blend,
            ForeColor = sprite.ForeColor,
            BackColor = sprite.BackColor,
            Ink = sprite.Ink
        };
    }
}
