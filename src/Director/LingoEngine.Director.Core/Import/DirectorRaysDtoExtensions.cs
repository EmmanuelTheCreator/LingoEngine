using System;
using ProjectorRays.Director;
using ProjectorRays.director.Chunks;
using LingoEngine.IO.Data.DTO;

namespace LingoEngine.Director.Core.Import;

internal static class DirectorRaysDtoExtensions
{
    public static LingoMovieDTO ToDto(this DirectorFile dir, string movieName, DirFilesContainerDTO resources)
    {
        var movie = new LingoMovieDTO
        {
            Name = movieName,
            Number = 0,
            Tempo = 0,
            FrameCount = dir.Score?.Frames.Count ?? 0
        };

        int castNum = 1;
        foreach (var cast in dir.Casts)
            movie.Casts.Add(cast.ToDto(castNum++, resources));

        if (dir.Score != null)
        {
            foreach (var f in dir.Score.Frames)
                movie.Sprites.Add(f.ToDto());
        }

        return movie;
    }

    public static LingoCastDTO ToDto(this CastChunk cast, int number, DirFilesContainerDTO resources)
    {
        var castDto = new LingoCastDTO
        {
            Name = cast.Name,
            Number = number,
            FileName = string.Empty,
            PreLoadMode = PreLoadModeTypeDTO.WhenNeeded
        };

        foreach (var mem in cast.Members.Values)
            castDto.Members.Add(mem.ToDto(castDto, resources));

        return castDto;
    }

    public static LingoMemberDTO ToDto(this CastMemberChunk mem, LingoCastDTO cast, DirFilesContainerDTO resources)
    {
        var baseDto = CreateBaseDto(mem, cast);

        return mem.Type switch
        {
            MemberType.FieldMember or MemberType.TextMember => mem.ToTextDto(baseDto),
            MemberType.BitmapMember or MemberType.PictureMember => mem.ToPictureDto(baseDto, cast, resources),
            MemberType.SoundMember => mem.ToSoundDto(baseDto, cast, resources),
            _ => baseDto
        };
    }

    private static LingoMemberDTO CreateBaseDto(CastMemberChunk mem, LingoCastDTO cast)
        => new LingoMemberDTO
        {
            Name = mem.GetName(),
            Number = mem.Id,
            CastLibNum = cast.Number,
            NumberInCast = mem.Id,
            Type = MapMemberType(mem.Type),
            RegPoint = new LingoPointDTO(),
            Width = 0,
            Height = 0,
            Size = mem.SpecificData.Size,
            Comments = string.Empty,
            FileName = string.Empty,
            PurgePriority = 0
        };

    private static LingoMemberTextDTO ToTextDto(this CastMemberChunk mem, LingoMemberDTO baseDto)
        => new LingoMemberTextDTO
        {
            Name = baseDto.Name,
            Number = baseDto.Number,
            CastLibNum = baseDto.CastLibNum,
            NumberInCast = baseDto.NumberInCast,
            Type = baseDto.Type,
            RegPoint = baseDto.RegPoint,
            Width = baseDto.Width,
            Height = baseDto.Height,
            Size = baseDto.Size,
            Comments = baseDto.Comments,
            FileName = baseDto.FileName,
            PurgePriority = baseDto.PurgePriority,
            Text = mem.GetText()
        };

    private static LingoMemberPictureDTO ToPictureDto(this CastMemberChunk mem, LingoMemberDTO baseDto, LingoCastDTO cast, DirFilesContainerDTO resources)
    {
        var file = $"{cast.Number}_{mem.Id}.img";
        var dto = new LingoMemberPictureDTO
        {
            Name = baseDto.Name,
            Number = baseDto.Number,
            CastLibNum = baseDto.CastLibNum,
            NumberInCast = baseDto.NumberInCast,
            Type = baseDto.Type,
            RegPoint = baseDto.RegPoint,
            Width = baseDto.Width,
            Height = baseDto.Height,
            Size = baseDto.Size,
            Comments = baseDto.Comments,
            FileName = baseDto.FileName,
            PurgePriority = baseDto.PurgePriority,
            ImageFile = file
        };
        var bytes = mem.SpecificData.Data.AsSpan(mem.SpecificData.Offset, mem.SpecificData.Size).ToArray();
        resources.Files.Add(new DirFileResourceDTO
        {
            CastName = cast.Name,
            FileName = file,
            Bytes = bytes
        });
        return dto;
    }

    private static LingoMemberSoundDTO ToSoundDto(this CastMemberChunk mem, LingoMemberDTO baseDto, LingoCastDTO cast, DirFilesContainerDTO resources)
    {
        var file = $"{cast.Number}_{mem.Id}.snd";
        var dto = new LingoMemberSoundDTO
        {
            Name = baseDto.Name,
            Number = baseDto.Number,
            CastLibNum = baseDto.CastLibNum,
            NumberInCast = baseDto.NumberInCast,
            Type = baseDto.Type,
            RegPoint = baseDto.RegPoint,
            Width = baseDto.Width,
            Height = baseDto.Height,
            Size = baseDto.Size,
            Comments = baseDto.Comments,
            FileName = baseDto.FileName,
            PurgePriority = baseDto.PurgePriority,
            SoundFile = file
        };
        var bytes = mem.SpecificData.Data.AsSpan(mem.SpecificData.Offset, mem.SpecificData.Size).ToArray();
        resources.Files.Add(new DirFileResourceDTO
        {
            CastName = cast.Name,
            FileName = file,
            Bytes = bytes
        });
        return dto;
    }

    public static LingoSpriteDTO ToDto(this ScoreChunk.FrameDescriptor f)
        => new LingoSpriteDTO
        {
            Name = $"Sprite{f.SpriteNumber}",
            SpriteNum = f.SpriteNumber,
            MemberNum = f.SpriteNumber,
            Puppet = false,
            Lock = false,
            Visibility = true,
            LocH = 0,
            LocV = 0,
            LocZ = f.Channel,
            Rotation = 0,
            Skew = 0,
            RegPoint = new LingoPointDTO(),
            Width = 0,
            Height = 0,
            BeginFrame = f.StartFrame,
            EndFrame = f.EndFrame
        };

    private static LingoMemberTypeDTO MapMemberType(MemberType t)
        => t switch
        {
            MemberType.BitmapMember => LingoMemberTypeDTO.Bitmap,
            MemberType.FilmLoopMember => LingoMemberTypeDTO.FilmLoop,
            MemberType.TextMember => LingoMemberTypeDTO.Text,
            MemberType.PaletteMember => LingoMemberTypeDTO.Palette,
            MemberType.PictureMember => LingoMemberTypeDTO.Picture,
            MemberType.SoundMember => LingoMemberTypeDTO.Sound,
            MemberType.ButtonMember => LingoMemberTypeDTO.Button,
            MemberType.ShapeMember => LingoMemberTypeDTO.Shape,
            MemberType.MovieMember => LingoMemberTypeDTO.Movie,
            MemberType.DigitalVideoMember => LingoMemberTypeDTO.DigitalVideo,
            MemberType.ScriptMember => LingoMemberTypeDTO.Script,
            MemberType.FieldMember => LingoMemberTypeDTO.Field,
            MemberType.FontMember => LingoMemberTypeDTO.Font,
            _ => LingoMemberTypeDTO.Unknown
        };
}
