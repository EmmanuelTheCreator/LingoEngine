using ProjectorRays.director.Scores.Data;

namespace ProjectorRays.director.Scores;

internal static class RaySpriteFactory
{


    internal static RayScoreKeyFrame CreateKeyFrame(RaySprite sprite, int spriteNum, int frame)
    {
        return new RayScoreKeyFrame
        {
            FrameNum = frame,
            SpriteNum = spriteNum,
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
