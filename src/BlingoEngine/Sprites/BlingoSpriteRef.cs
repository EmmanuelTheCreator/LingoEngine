using System;
using BlingoEngine.ColorPalettes;
using BlingoEngine.Scripts;
using BlingoEngine.Sounds;
using BlingoEngine.Tempos;
using BlingoEngine.Transitions;

namespace BlingoEngine.Sprites;

public readonly struct BlingoSpriteRef : IEquatable<BlingoSpriteRef>
{
    public BlingoSpriteRef(int spriteNum, int beginFrame, BlingoSpriteType spriteType)
    {
        SpriteNum = spriteNum;
        BeginFrame = beginFrame;
        SpriteType = spriteType;
    }

    public int SpriteNum { get; }
    public int BeginFrame { get; }
    public BlingoSpriteType SpriteType { get; }

    public static BlingoSpriteRef FromSprite(BlingoSprite sprite)
    {
        if (sprite == null) throw new ArgumentNullException(nameof(sprite));
        return new BlingoSpriteRef(sprite.SpriteNumWithChannel, sprite.BeginFrame, GetSpriteType(sprite));
    }

    public static BlingoSpriteType GetSpriteType(BlingoSprite sprite) => sprite switch
    {
        BlingoSprite2D => BlingoSpriteType.Sprite2D,
        BlingoTempoSprite => BlingoSpriteType.Tempo,
        BlingoColorPaletteSprite => BlingoSpriteType.ColorPalette,
        BlingoFrameScriptSprite => BlingoSpriteType.FrameScript,
        BlingoTransitionSprite => BlingoSpriteType.Transition,
        BlingoSpriteSound => BlingoSpriteType.Sound,
        _ => BlingoSpriteType.Unknown,
    };

    public bool Equals(BlingoSpriteRef other) => SpriteNum == other.SpriteNum && BeginFrame == other.BeginFrame && SpriteType == other.SpriteType;

    public override bool Equals(object? obj) => obj is BlingoSpriteRef other && Equals(other);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = SpriteNum;
            hash = (hash * 397) ^ BeginFrame;
            hash = (hash * 397) ^ (int)SpriteType;
            return hash;
        }
    }

    public override string ToString() => $"{SpriteType},{SpriteNum},{BeginFrame}";
}
