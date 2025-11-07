using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Data.DTO.Members;
using BlingoEngine.IO.Legacy.Bitmaps;
using BlingoEngine.IO.Legacy.Cast;
using BlingoEngine.IO.Legacy.Cast.Data;
using Microsoft.Extensions.Logging;

namespace BlingoEngine.IO.Legacy.Director;

/// <summary>
/// Extension helpers that translate legacy movie archives into BlingoEngine DTOs.
/// </summary>
public static class BlLegacyMovieBlingoExtensions
{
    public static BlingoStageDTO ToBlingoStage(this BlLegacyMovieArchive archive)
    {
        ArgumentNullException.ThrowIfNull(archive);
        return new BlingoStageDTO();
    }

    public static BlingoMovieDTO ToBlingo(this BlLegacyMovieArchive archive, string movieName, DirFilesContainerDTO resources, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(resources);

        var context = new BlLegacyMovieConvertContext(archive, resources, logger);

        var movie = new BlingoMovieDTO
        {
            Name = movieName,
            Number = 0,
            Tempo = 0,
            FrameCount = 0,
            MaxSpriteChannelCount = 0
        };

        var castNumber = 1;
        foreach (var cast in archive.CastLibraries)
        {
            var castDto = cast.ToBlingo(castNumber, context);
            movie.Casts.Add(castDto);
            castNumber++;
        }

        foreach (var sprite in BlLegacyScoreSpriteBuilder.Build(archive.Score))
            movie.Sprite2Ds.Add(sprite);

        return movie;
    }

    public static BlingoCastDTO ToBlingo(this BlLegacyRawCastLibrary cast, int castNumber, BlLegacyMovieConvertContext context)
    {

        var castName = string.IsNullOrWhiteSpace(cast.Name) ? $"Cast {castNumber}" : cast.Name;
        var castDto = new BlingoCastDTO
        {
            Name = castName,
            FileName = context.GetCastFileName(cast.CastPath),
            Number = castNumber,
            PreLoadMode = cast.Preload.ToDto()
        };
        context.SetCurrentCast(castDto);
        foreach (var slot in cast.MemberSlots)
        {
            var member = slot.ToBlingo(context);
            castDto.Members.Add(member);
        }

        return castDto;
    }

    public static BlingoMemberDTO ToBlingo(this BlLegacyCastMemberSlot slot, BlLegacyMovieConvertContext context)
    {
        var member = slot.Member.MemberType switch
        {
            BlLegacyCastMemberType.Text => slot.Member.ToTextMember(context, slot),
            BlLegacyCastMemberType.Field => slot.Member.ToFieldDto(context, slot),
            BlLegacyCastMemberType.Script => slot.Member.ToScriptDto(context, slot),
            BlLegacyCastMemberType.DigitalVideo => slot.Member.ToVideoDto(context, slot),
            BlLegacyCastMemberType.Sound => slot.Member.ToSoundDto(context,slot),
            BlLegacyCastMemberType.Bitmap or BlLegacyCastMemberType.Picture => slot.Member.ToBitmapDto(context, slot),
            // Not implemented, return default
            _ => new BlingoMemberDTO { Name = slot.Member.Name, CastLibNum = context.CurrentCast.Number,NumberInCast = slot.SlotIndex+1}
        };
        return member;
    }

    public static BlingoMemberDTO ToTextMember(this BlCastRawMemberItem rawMemberG, BlLegacyMovieConvertContext context, BlLegacyCastMemberSlot slot)
    {
        var rawMember = (BlCastRawMemberText)rawMemberG;
        var member = context.CreateMember<BlingoMemberTextDTO>(slot);
        if (context.TryGetText(slot.ResourceId, out var text))
        {
            var content = context.DecodeText(text);
            member.Size = text.Bytes.Length;
            member.MarkDownText = content;
        }

        member.IsEditable = rawMember.IsEditable;
        member.TabsEnabled = rawMember.TabsEnabled;
        member.DtdEnabled = rawMember.DtdEnabled;
        member.IsAntialiasEnabled = rawMember.IsAntialiasEnabled;
        member.AntialiasMode = rawMember.AntialiasMode;
        member.AntialiasLargerThanPointSize = rawMember.AntialiasLargerThanPointSize;
        member.IsKerningEnabled = rawMember.IsKerningEnabled;
        member.KerningMode = rawMember.KerningMode;
        member.KerningLargerThanPointSize = rawMember.KerningLargerThanPointSize;
        return member;
    }

    public static BlingoMemberDTO ToFieldDto(this BlCastRawMemberItem rawMemberG, BlLegacyMovieConvertContext context, BlLegacyCastMemberSlot slot)
    {
        var rawMember = (BlCastRawMemberText)rawMemberG;
        var member = context.CreateMember<BlingoMemberFieldDTO>(slot);
        if (context.TryGetField(slot.ResourceId, out var field))
        {
            var content = context.DecodeField(field);
            member.Size = field.Bytes.Length;
            member.MarkDownText = content;
        }

        member.IsEditable = rawMember.IsEditable;
        member.TabsEnabled = rawMember.TabsEnabled;
        member.DtdEnabled = rawMember.DtdEnabled;
        member.IsAntialiasEnabled = rawMember.IsAntialiasEnabled;
        member.AntialiasMode = rawMember.AntialiasMode;
        member.AntialiasLargerThanPointSize = rawMember.AntialiasLargerThanPointSize;
        member.IsKerningEnabled = rawMember.IsKerningEnabled;
        member.KerningMode = rawMember.KerningMode;
        member.KerningLargerThanPointSize = rawMember.KerningLargerThanPointSize;
        return member;
    }

    internal static BlingoMemberDTO ToBitmapDto(this BlCastRawMemberItem rawMemberG, BlLegacyMovieConvertContext context, BlLegacyCastMemberSlot slot)
    {
        var rawMember = (BlCastRawMemberBitmap)rawMemberG;
        var member = context.CreateMember<BlingoMemberBitmapDTO>(slot);
        if (context.TryGetBitmap(slot.ResourceId, out var bitmap))
        {
            var includeResource = bitmap.Format != BlLegacyBitmapFormatKind.Thumbnail;
            string imageFile = string.Empty;
            int size = includeResource ? bitmap.Bytes.Length : 0;
            if (includeResource)
            {
                var resource = context.CreateBitmapResource(bitmap, member.NumberInCast);
                imageFile = resource.FileName;
            }
            member.Size = size;
            member.ImageFile = imageFile;
        }
         
        member.Width = rawMember.Width;
        member.Height = rawMember.Height;
        
        return member;
    }

    public static BlingoMemberDTO ToSoundDto(this BlCastRawMemberItem rawMemberG, BlLegacyMovieConvertContext context, BlLegacyCastMemberSlot slot)
    {
        var rawMember = (BlCastRawMemberAudio)rawMemberG;
        if (!context.TryGetSound(slot.ResourceId, out var sound))
            return new BlingoMemberSoundDTO();

        var member = context.CreateMember<BlingoMemberSoundDTO>(slot);
        var resource = context.CreateSoundResource(sound, member.NumberInCast);

        member.Size = resource.Bytes.Length;
        member.SoundFile = resource.FileName;
        return member;
    }
    public static BlingoMemberDTO ToScriptDto(this BlCastRawMemberItem rawMemberG, BlLegacyMovieConvertContext context, BlLegacyCastMemberSlot slot)
    {
        var rawMember = (BlCastRawMemberScript)rawMemberG;
        var member = context.CreateMember<BlingoMemberScriptDTO>(slot);
        var resource = context.CreateScriptResource(rawMember, member);

        member.Script = rawMember.Script ?? string.Empty;
        member.IsJavascript = rawMember.IsJavascript;
        member.ScriptType = rawMember.ScriptType;

        return member;
    }
    public static BlingoMemberDTO ToVideoDto(this BlCastRawMemberItem rawMemberG, BlLegacyMovieConvertContext context, BlLegacyCastMemberSlot slot)
    {
        var rawMember = (BlCastRawMemberVideo)rawMemberG;
        var member = context.CreateMember<BlingoMemberVideoDTO>(slot);

        // Video specific
        member.Width = rawMember.Width;
        member.Height = rawMember.Height;
        member.DurationSeconds = rawMember.DurationSeconds;
        member.LinkedFileName = rawMember.LinkedFileName;
        member.LinkedFolder = rawMember.LinkedFolder;
        member.PlayVideo = rawMember.PlayVideo;
        member.PlayAudio = rawMember.PlayAudio;
        member.StartPause = rawMember.StartPause;
        member.EnableLoop = rawMember.EnableLoop;
        member.StartValueMs = rawMember.StartValueMs;
        member.VideoFps = rawMember.VideoFps;
       
        return member;
    }




    internal static BlingoMemberTypeDTO ToDto(this BlLegacyCastMemberType type)
    {
        return type switch
        {
            BlLegacyCastMemberType.Bitmap => BlingoMemberTypeDTO.Bitmap,
            BlLegacyCastMemberType.FilmLoop => BlingoMemberTypeDTO.FilmLoop,
            BlLegacyCastMemberType.Text => BlingoMemberTypeDTO.Text,
            BlLegacyCastMemberType.Palette => BlingoMemberTypeDTO.Palette,
            BlLegacyCastMemberType.Picture => BlingoMemberTypeDTO.Picture,
            BlLegacyCastMemberType.Sound => BlingoMemberTypeDTO.Sound,
            BlLegacyCastMemberType.Button => BlingoMemberTypeDTO.Button,
            BlLegacyCastMemberType.Shape => BlingoMemberTypeDTO.Shape,
            BlLegacyCastMemberType.Movie => BlingoMemberTypeDTO.Movie,
            BlLegacyCastMemberType.DigitalVideo => BlingoMemberTypeDTO.DigitalVideo,
            BlLegacyCastMemberType.Script => BlingoMemberTypeDTO.Script,
            BlLegacyCastMemberType.Rte => BlingoMemberTypeDTO.Script,
            BlLegacyCastMemberType.Font => BlingoMemberTypeDTO.Font,
            BlLegacyCastMemberType.Field => BlingoMemberTypeDTO.Field,
            _ => BlingoMemberTypeDTO.Unknown
        };
    }
    internal static PreLoadModeTypeDTO ToDto(this BlLegacyRawCastLibrary.CastPreload preload)
    {
        return preload switch
        {
            BlLegacyRawCastLibrary.CastPreload.BeforeFrameOne => PreLoadModeTypeDTO.BeforeFrame1,
            BlLegacyRawCastLibrary.CastPreload.AfterFrameOne => PreLoadModeTypeDTO.AfterFrame1,
            _ => PreLoadModeTypeDTO.WhenNeeded
        };
    }
}
