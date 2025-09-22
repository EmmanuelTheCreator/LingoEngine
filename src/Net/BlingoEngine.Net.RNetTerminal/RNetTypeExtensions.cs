using BlingoEngine.IO.Data.DTO.Sprites;
using BlingoEngine.Net.RNetContracts;

namespace BlingoEngine.Net.RNetTerminal;

internal static class RNetTypeExtensions
{
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
