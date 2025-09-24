using BlingoEngine.Members;
using BlingoEngine.Net.RNetContracts;
using BlingoEngine.Sprites;

namespace BlingoEngine.Net.RNetHost.Common;

/// <summary>Helpers for converting engine objects to RNet DTOs.</summary>
internal static class DtoExtensions
{
    /// <summary>Converts a sprite to its network DTO.</summary>
    public static RNetSpriteDto ToDto(this BlingoSprite2D sprite)
    {
        var (castLib, memberNum) = GetSpriteMemberInfo(sprite);

        return new RNetSpriteDto(
            sprite.SpriteNum,
            sprite.BeginFrame,
            sprite.LocZ,
            castLib,
            memberNum,
            (int)sprite.LocH,
            (int)sprite.LocV,
            (int)sprite.Width,
            (int)sprite.Height,
            (int)sprite.Rotation,
            (int)sprite.Skew,
            (int)sprite.Blend,
            sprite.Ink);
    }

    /// <summary>Converts a sprite to a delta DTO for the specified frame.</summary>
    public static SpriteDeltaDto ToDelta(this BlingoSprite2D sprite, int frame)
    {
        var (castLib, memberNum) = GetSpriteMemberInfo(sprite);

        return new SpriteDeltaDto(
            frame,
            sprite.SpriteNum,
            sprite.BeginFrame,
            sprite.LocZ,
            castLib,
            memberNum,
            (int)sprite.LocH,
            (int)sprite.LocV,
            (int)sprite.Width,
            (int)sprite.Height,
            (int)sprite.Rotation,
            (int)sprite.Skew,
            (int)sprite.Blend,
            sprite.Ink);
    }

    private static (int CastLib, int MemberNum) GetSpriteMemberInfo(BlingoSprite2D sprite)
    {
        int castLib = 0;
        int memberNum = 0;
        if (sprite.Member is IBlingoMember m)
        {
            castLib = m.CastLibNum;
            memberNum = m.NumberInCast;
        }

        return (castLib, memberNum);
    }
}

