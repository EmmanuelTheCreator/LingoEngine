using System;
using BlingoEngine.IO.Data.DTO.Members;
using BlingoEngine.IO.Data.DTO.Sprites;
using BlingoEngine.Net.RNetContracts;

namespace BlingoEngine.Net.RNetTerminal;

internal static class RNetTypeExtensions
{
    public static RNetMemberTypeDto ToRNet(this BlingoMemberTypeDTO type)
        => Enum.TryParse<RNetMemberTypeDto>(type.ToString(), out var result)
            ? result
            : RNetMemberTypeDto.Unknown;

    public static RNetSpriteTypeDto ToRNet(this Blingo2DSpriteDTO sprite)
        => RNetSpriteTypeDto.Sprite2D;

    public static RNetSpriteTypeDto ToRNet(this BlingoTempoSpriteDTO sprite)
        => RNetSpriteTypeDto.Tempo;

    public static RNetSpriteTypeDto ToRNet(this BlingoColorPaletteSpriteDTO sprite)
        => RNetSpriteTypeDto.ColorPalette;

    public static RNetSpriteTypeDto ToRNet(this BlingoTransitionSpriteDTO sprite)
        => RNetSpriteTypeDto.Transition;

    public static RNetSpriteTypeDto ToRNet(this BlingoSpriteSoundDTO sprite)
        => RNetSpriteTypeDto.Sound;
}
