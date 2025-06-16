using System.Text.Json;
using System.Reflection;
using LingoEngine.Movies;
using LingoEngine.Core;
using LingoEngine.Texts;
using LingoEngine.Sounds;
using LingoEngine.IO.Data.DTO;
using LingoEngine.Pictures;

namespace LingoEngine.IO;

public class JsonStateRepository
{
    public void Save(string filePath, LingoMovie movie)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (string.IsNullOrEmpty(dir))
            dir = Directory.GetCurrentDirectory();
        var dto = ToDto(movie, dir);
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(dto, options);
        File.WriteAllText(filePath, json);
    }

    private static LingoMovieDTO ToDto(LingoMovie movie, string dir)
    {
        return new LingoMovieDTO
        {
            Name = movie.Name,
            Number = movie.Number,
            Tempo = movie.Tempo,
            FrameCount = movie.FrameCount,
            Casts = movie.CastLib.GetAll().Select(c => ToDto((LingoCast)c, dir)).ToList(),
            Sprites = GetAllSprites(movie).Select(ToDto).ToList()
        };
    }

    private static LingoCastDTO ToDto(LingoCast cast, string dir)
    {
        return new LingoCastDTO
        {
            Name = cast.Name,
            FileName = cast.FileName,
            Number = cast.Number,
            PreLoadMode = (PreLoadModeTypeDTO)cast.PreLoadMode,
            Members = cast.GetAll().Select(m => ToDto(m, dir)).ToList()
        };
    }

    private static LingoMemberDTO ToDto(ILingoMember member, string dir)
    {
        var baseDto = new LingoMemberDTO
        {
            Name = member.Name,
            Number = member.Number,
            CastLibNum = member.CastLibNum,
            NumberInCast = member.NumberInCast,
            Type = (LingoMemberTypeDTO)member.Type,
            RegPoint = new LingoPointDTO { X = member.RegPoint.X, Y = member.RegPoint.Y },
            Width = member.Width,
            Height = member.Height,
            Size = member.Size,
            Comments = member.Comments,
            FileName = member.FileName,
            PurgePriority = member.PurgePriority
        };

        return member switch
        {
            LingoMemberField field => new LingoMemberFieldDTO
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
                IsFocused = field.IsFocused,
                Text = field.Text
            },
            LingoMemberSound sound => new LingoMemberSoundDTO
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
                Stereo = sound.Stereo,
                Length = sound.Length,
                Loop = sound.Loop,
                IsLinked = sound.IsLinked,
                LinkedFilePath = sound.LinkedFilePath,
                SoundFile = SaveSound(sound, dir)
            },
            LingoMemberText text => new LingoMemberTextDTO
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
                Text = text.Text
            },
            LingoMemberPicture picture => new LingoMemberPictureDTO
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
                ImageFile = SavePicture(picture, dir)
            },
            _ => baseDto
        };
    }

    private static LingoSpriteDTO ToDto(LingoSprite sprite)
    {
        return new LingoSpriteDTO
        {
            Name = sprite.Name,
            SpriteNum = sprite.SpriteNum,
            MemberNum = sprite.MemberNum,
            Puppet = sprite.Puppet,
            Visibility = sprite.Visibility,
            LocH = sprite.LocH,
            LocV = sprite.LocV,
            LocZ = sprite.LocZ,
            Rotation = sprite.Rotation,
            Skew = sprite.Skew,
            RegPoint = new LingoPointDTO { X = sprite.RegPoint.X, Y = sprite.RegPoint.Y },
            Width = sprite.Width,
            Height = sprite.Height,
            BeginFrame = sprite.BeginFrame,
            EndFrame = sprite.EndFrame
        };
    }

    private static IEnumerable<LingoSprite> GetAllSprites(LingoMovie movie)
    {
        var field = typeof(LingoMovie).GetField("_allTimeSprites", BindingFlags.NonPublic | BindingFlags.Instance);
        if (field?.GetValue(movie) is IEnumerable<LingoSprite> sprites)
            return sprites;
        return Enumerable.Empty<LingoSprite>();
    }

    private static string SavePicture(LingoMemberPicture picture, string dir)
    {
        if (picture.ImageData == null)
            return string.Empty;

        var ext = GetPictureExtension(picture);
        var name = $"{picture.NumberInCast}_{SanitizeFileName(picture.Name)}.{ext}";
        var path = Path.Combine(dir, name);
        File.WriteAllBytes(path, picture.ImageData);
        return name;
    }

    private static string SaveSound(LingoMemberSound sound, string dir)
    {
        var source = !string.IsNullOrEmpty(sound.FileName) && File.Exists(sound.FileName)
            ? sound.FileName
            : sound.LinkedFilePath;
        if (string.IsNullOrEmpty(source) || !File.Exists(source))
            return string.Empty;

        var ext = GetSoundExtension(source);
        var name = $"{sound.NumberInCast}_{SanitizeFileName(sound.Name)}{ext}";
        var dest = Path.Combine(dir, name);
        File.Copy(source, dest, true);
        return name;
    }

    private static string GetSoundExtension(string path)
    {
        var ext = Path.GetExtension(path);
        if (string.IsNullOrEmpty(ext))
            return ".wav";
        return ext.ToLowerInvariant();
    }

    private static string GetPictureExtension(LingoMemberPicture picture)
    {
        var format = picture.Format.ToLowerInvariant();
        if (format.Contains("png") || format.Contains("gif") || format.Contains("tiff"))
            return "png";
        return "bmp";
    }

    private static string SanitizeFileName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }
}
