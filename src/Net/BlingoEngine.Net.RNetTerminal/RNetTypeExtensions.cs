using BlingoEngine.IO.Data.DTO.Members;
using BlingoEngine.IO.Data.DTO.Sprites;
using BlingoEngine.Net.RNetContracts;

namespace BlingoEngine.Net.RNetTerminal;

internal static class RNetTypeExtensions
{
    public static RNetMemberTypeDto ToRNet(this BlingoMemberTypeDTO memberType)
        => memberType switch
        {
            BlingoMemberTypeDTO.Animgif => RNetMemberTypeDto.Animgif,
            BlingoMemberTypeDTO.Ole => RNetMemberTypeDto.Ole,
            BlingoMemberTypeDTO.Bitmap => RNetMemberTypeDto.Bitmap,
            BlingoMemberTypeDTO.Palette => RNetMemberTypeDto.Palette,
            BlingoMemberTypeDTO.Button => RNetMemberTypeDto.Button,
            BlingoMemberTypeDTO.Picture => RNetMemberTypeDto.Picture,
            BlingoMemberTypeDTO.Cursor => RNetMemberTypeDto.Cursor,
            BlingoMemberTypeDTO.QuickTimeMedia => RNetMemberTypeDto.QuickTimeMedia,
            BlingoMemberTypeDTO.DigitalVideo => RNetMemberTypeDto.DigitalVideo,
            BlingoMemberTypeDTO.RealMedia => RNetMemberTypeDto.RealMedia,
            BlingoMemberTypeDTO.DVD => RNetMemberTypeDto.DVD,
            BlingoMemberTypeDTO.Script => RNetMemberTypeDto.Script,
            BlingoMemberTypeDTO.Empty => RNetMemberTypeDto.Empty,
            BlingoMemberTypeDTO.Shape => RNetMemberTypeDto.Shape,
            BlingoMemberTypeDTO.Field => RNetMemberTypeDto.Field,
            BlingoMemberTypeDTO.Shockwave3D => RNetMemberTypeDto.Shockwave3D,
            BlingoMemberTypeDTO.FilmLoop => RNetMemberTypeDto.FilmLoop,
            BlingoMemberTypeDTO.Sound => RNetMemberTypeDto.Sound,
            BlingoMemberTypeDTO.Flash => RNetMemberTypeDto.Flash,
            BlingoMemberTypeDTO.Swa => RNetMemberTypeDto.Swa,
            BlingoMemberTypeDTO.Flashcomponent => RNetMemberTypeDto.Flashcomponent,
            BlingoMemberTypeDTO.Text => RNetMemberTypeDto.Text,
            BlingoMemberTypeDTO.Font => RNetMemberTypeDto.Font,
            BlingoMemberTypeDTO.Transition => RNetMemberTypeDto.Transition,
            BlingoMemberTypeDTO.Havok => RNetMemberTypeDto.Havok,
            BlingoMemberTypeDTO.VectorShape => RNetMemberTypeDto.VectorShape,
            BlingoMemberTypeDTO.Movie => RNetMemberTypeDto.Movie,
            BlingoMemberTypeDTO.WindowsMedia => RNetMemberTypeDto.WindowsMedia,
            _ => RNetMemberTypeDto.Unknown,
        };

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
