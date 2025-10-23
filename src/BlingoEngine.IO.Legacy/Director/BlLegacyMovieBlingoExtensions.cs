using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Data.DTO.Members;
using BlingoEngine.IO.Legacy.Bitmaps;
using BlingoEngine.IO.Legacy.Cast;
using BlingoEngine.IO.Legacy.Cast.Data;
using BlingoEngine.IO.Legacy.Fields;
using BlingoEngine.IO.Legacy.Sounds;
using BlingoEngine.IO.Legacy.Texts;
using BlingoEngine.IO.Legacy.Texts.Data;
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

        var movie = new BlingoMovieDTO
        {
            Name = movieName,
            Number = 0,
            Tempo = 0,
            FrameCount = 0,
            MaxSpriteChannelCount = 0
        };

        var bitmapExporter = new BlLegacyBitmapExporter();
        var soundExporter = new BlLegacySoundExporter();
        var usedNames = new HashSet<string>(resources.Files.Select(f => f.FileName), StringComparer.OrdinalIgnoreCase);

        var castNumber = 1;
        foreach (var cast in archive.CastLibraries)
        {
            var castDto = cast.ToBlingo(castNumber, archive, resources, usedNames, bitmapExporter, soundExporter, logger);
            movie.Casts.Add(castDto);
            castNumber++;
        }

        return movie;
    }

    public static BlingoCastDTO ToBlingo(
        this BlLegacyCastLibrary cast,
        int castNumber,
        BlLegacyMovieArchive archive,
        DirFilesContainerDTO resources,
        HashSet<string> usedNames,
        BlLegacyBitmapExporter bitmapExporter,
        BlLegacySoundExporter soundExporter, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(cast);
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(usedNames);
        ArgumentNullException.ThrowIfNull(bitmapExporter);
        ArgumentNullException.ThrowIfNull(soundExporter);

        var castDto = new BlingoCastDTO
        {
            Name = $"Cast {castNumber}",
            Number = castNumber,
            PreLoadMode = PreLoadModeTypeDTO.WhenNeeded
        };

        foreach (var slot in cast.MemberSlots)
        {
            var member = slot.ToBlingo(archive, castDto, resources, usedNames, bitmapExporter, soundExporter, logger);
            castDto.Members.Add(member);
        }

        return castDto;
    }

    public static BlingoMemberDTO ToBlingo(
        this BlLegacyCastMemberSlot slot,
        BlLegacyMovieArchive archive,
        BlingoCastDTO castDto,
        DirFilesContainerDTO resources,
        HashSet<string> usedNames,
        BlLegacyBitmapExporter bitmapExporter,
        BlLegacySoundExporter soundExporter, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(castDto);
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(usedNames);
        ArgumentNullException.ThrowIfNull(bitmapExporter);
        ArgumentNullException.ThrowIfNull(soundExporter);

        var memberIndex = slot.SlotIndex + 1;
        var memberName = string.IsNullOrWhiteSpace(slot.Member.Name) ? $"Member {memberIndex}" : slot.Member.Name;

        var baseDto = new BlingoMemberDTO
        {
            Name = memberName,
            CastLibNum = castDto.Number,
            NumberInCast = memberIndex,
            Type = MapMemberType(slot.Member.MemberType),
            RegPoint =  new BlingoPointDTO(),
            Width = 0,
            Height = 0,
            Size = 0,
            Comments = string.Empty,
            FileName = string.Empty,
            PurgePriority = 0
        };

        var member = slot.Member.MemberType switch
        {
            BlLegacyCastMemberType.Text => baseDto.ToTextMember(archive, slot.ResourceId, logger, (BlCastMemberText)slot.Member),
            BlLegacyCastMemberType.Field => baseDto.ToFieldMember(archive, slot.ResourceId, logger, (BlCastMemberText)slot.Member),
            BlLegacyCastMemberType.Script => baseDto.ToScriptMember(archive, slot.ResourceId, logger, (BlCastMemberScript)slot.Member),
            BlLegacyCastMemberType.DigitalVideo => baseDto.ToVideoMember(archive, slot.ResourceId, logger, (BlCastMemberVideo)slot.Member),
            BlLegacyCastMemberType.Bitmap or BlLegacyCastMemberType.Picture => baseDto.ToBitmapMember(
                archive,
                slot.ResourceId,
                castDto,
                resources,
                usedNames,
                bitmapExporter, (BlCastMemberBitmap)slot.Member),
            BlLegacyCastMemberType.Sound => baseDto.ToSoundMember(
                archive,
                slot.ResourceId,
                castDto,
                resources,
                usedNames,
                soundExporter, (BlCastMemberAudio)slot.Member),
            _ => baseDto
        };
        return member;
    }

    public static BlingoMemberDTO ToTextMember(this BlingoMemberDTO baseDto, BlLegacyMovieArchive archive, int castResourceId, ILogger logger, BlCastMemberText memberTxt)
    {
        if (!archive.TryGetText(castResourceId, out var text))
            return baseDto;

        var content = DecodeText(text, archive.DirectorVersion, logger);
        return new BlingoMemberTextDTO
        {
            Name = baseDto.Name,
            CastLibNum = baseDto.CastLibNum,
            NumberInCast = baseDto.NumberInCast,
            Type = baseDto.Type,
            RegPoint = baseDto.RegPoint,
            Width = baseDto.Width,
            Height = baseDto.Height,
            Size = text.Bytes.Length,
            Comments = baseDto.Comments,
            FileName = baseDto.FileName,
            PurgePriority = baseDto.PurgePriority,
            MarkDownText = content,

            IsEditable = memberTxt.IsEditable,
            TabsEnabled = memberTxt.TabsEnabled,
            DtdEnabled = memberTxt.DtdEnabled,
            IsAntialiasEnabled = memberTxt.IsAntialiasEnabled,
            AntialiasMode = memberTxt.AntialiasMode,
            AntialiasLargerThanPointSize = memberTxt.AntialiasLargerThanPointSize,
            IsKerningEnabled = memberTxt.IsKerningEnabled,
            KerningMode = memberTxt.KerningMode,
            KerningLargerThanPointSize = memberTxt.KerningLargerThanPointSize,
            // Common
            DateCreated = memberTxt.Created.GetValueOrDefault(),
            DateModified = memberTxt.Modified.GetValueOrDefault(),
            MediaContentType = memberTxt.MediaContentType ?? "",
        };
    }

    public static BlingoMemberDTO ToFieldMember(this BlingoMemberDTO baseDto, BlLegacyMovieArchive archive, int castResourceId, ILogger logger, BlCastMemberText memberTxt)
    {
        if (!archive.TryGetField(castResourceId, out var field))
            return baseDto;

        var content = DecodeField(field, archive.DirectorVersion, logger);
        return new BlingoMemberFieldDTO
        {
            Name = baseDto.Name,
            CastLibNum = baseDto.CastLibNum,
            NumberInCast = baseDto.NumberInCast,
            Type = baseDto.Type,
            RegPoint = baseDto.RegPoint,
            Width = baseDto.Width,
            Height = baseDto.Height,
            Size = field.Bytes.Length,
            Comments = baseDto.Comments,
            FileName = baseDto.FileName,
            PurgePriority = baseDto.PurgePriority,
            MarkDownText = content,

            IsEditable = memberTxt.IsEditable,
            TabsEnabled = memberTxt.TabsEnabled,
            DtdEnabled = memberTxt.DtdEnabled,
            IsAntialiasEnabled = memberTxt.IsAntialiasEnabled,
            AntialiasMode = memberTxt.AntialiasMode,
            AntialiasLargerThanPointSize = memberTxt.AntialiasLargerThanPointSize,
            IsKerningEnabled = memberTxt.IsKerningEnabled,
            KerningMode = memberTxt.KerningMode,
            KerningLargerThanPointSize = memberTxt.KerningLargerThanPointSize,
            // Common
            DateCreated = memberTxt.Created.GetValueOrDefault(),
            DateModified = memberTxt.Modified.GetValueOrDefault(),
            MediaContentType = memberTxt.MediaContentType ?? "",
        };
    }

    internal static BlingoMemberDTO ToBitmapMember(
        this BlingoMemberDTO baseDto,
        BlLegacyMovieArchive archive,
        int castResourceId,
        BlingoCastDTO castDto,
        DirFilesContainerDTO resources,
        HashSet<string> usedNames,
        BlLegacyBitmapExporter exporter, BlCastMemberBitmap memberBm)
    {
        if (!archive.TryGetBitmap(castResourceId, out var bitmap))
            return baseDto;

        var resource = exporter.CreateResource(bitmap, castDto.Name, $"{castDto.Number}_{baseDto.NumberInCast}");
        var fileName = EnsureUniqueFileName(resource.FileName, usedNames);
        resource.FileName = fileName;
        resources.Files.Add(resource);

        return new BlingoMemberBitmapDTO
        {
            Name = baseDto.Name,
            CastLibNum = baseDto.CastLibNum,
            NumberInCast = baseDto.NumberInCast,
            Type = baseDto.Type,
            RegPoint = new BlingoPointDTO { X= memberBm.LocH, Y= memberBm.LocV},
            Width = memberBm.Width,
            Height = memberBm.Height,
            Size = bitmap.Bytes.Length,
            Comments = baseDto.Comments,
            FileName = baseDto.FileName,
            PurgePriority = baseDto.PurgePriority,
            ImageFile = fileName,
            // Common
            DateCreated = memberBm.Created.GetValueOrDefault(),
            DateModified = memberBm.Modified.GetValueOrDefault(),
            MediaContentType = memberBm.MediaContentType ?? "",
        };
    }

    public static BlingoMemberDTO ToSoundMember(
        this BlingoMemberDTO baseDto,
        BlLegacyMovieArchive archive,
        int castResourceId,
        BlingoCastDTO castDto,
        DirFilesContainerDTO resources,
        HashSet<string> usedNames,
        BlLegacySoundExporter exporter, BlCastMemberAudio memberSnd)
    {
        if (!archive.TryGetSound(castResourceId, out var sound))
            return baseDto;

        var resource = exporter.CreateResource(sound, castDto.Name, $"{castDto.Number}_{baseDto.NumberInCast}", castDto.Number, baseDto.NumberInCast);
        var fileName = EnsureUniqueFileName(resource.FileName, usedNames);
        resource.FileName = fileName;
        resources.Files.Add(resource);

        return new BlingoMemberSoundDTO
        {
            Name = baseDto.Name,
            CastLibNum = baseDto.CastLibNum,
            NumberInCast = baseDto.NumberInCast,
            Type = baseDto.Type,
            RegPoint = baseDto.RegPoint,
            Width = baseDto.Width,
            Height = baseDto.Height,
            Size = sound.Bytes.Length,
            Comments = baseDto.Comments,
            FileName = baseDto.FileName,
            PurgePriority = baseDto.PurgePriority,
            SoundFile = fileName,
            // Common
            DateCreated = memberSnd.Created.GetValueOrDefault(),
            DateModified = memberSnd.Modified.GetValueOrDefault(),
            MediaContentType = memberSnd.MediaContentType ?? "",
        };
    }
    public static BlingoMemberDTO ToScriptMember(
        this BlingoMemberDTO baseDto,
        BlLegacyMovieArchive archive,
        int castResourceId,
        ILogger logger,
        BlCastMemberScript memberScript)
    {
        return new BlingoMemberScriptDTO
        {
            Name = baseDto.Name,
            CastLibNum = baseDto.CastLibNum,
            NumberInCast = baseDto.NumberInCast,
            Type = baseDto.Type,
            RegPoint = baseDto.RegPoint,
            Width = baseDto.Width,
            Height = baseDto.Height,
            Size = memberScript.Script.Length,
            Comments = baseDto.Comments,
            FileName = baseDto.FileName,
            PurgePriority = baseDto.PurgePriority,

            // Script specific
            Script = memberScript.Script,
            IsJavascript = memberScript.IsJavascript,
            LinkedFilePath = memberScript.LinkedFileName,
            ScriptType = memberScript.ScriptType,
            
            // Common
            DateCreated = memberScript.Created.GetValueOrDefault(),
            DateModified = memberScript.Modified.GetValueOrDefault(),
            MediaContentType = memberScript.MediaContentType ?? "",
        };
    }
    public static BlingoMemberDTO ToVideoMember(
        this BlingoMemberDTO baseDto,
        BlLegacyMovieArchive archive,
        int castResourceId,
        ILogger logger,
        BlCastMemberVideo memberScript)
    {
        return new BlingoMemberVideoDTO
        {
            Name = baseDto.Name,
            CastLibNum = baseDto.CastLibNum,
            NumberInCast = baseDto.NumberInCast,
            Type = baseDto.Type,
            RegPoint = baseDto.RegPoint,
            Size = 0,
            Comments = baseDto.Comments,
            FileName = baseDto.FileName,
            PurgePriority = baseDto.PurgePriority,

            // Video specific
            Width = memberScript.Width,
            Height = memberScript.Height,
            DurationSeconds = memberScript.DurationSeconds,
            LinkedFileName = memberScript.LinkedFileName,
            LinkedFolder = memberScript.LinkedFolder,
            PlayVideo = memberScript.PlayVideo,
            PlayAudio = memberScript.PlayAudio,
            StartPause = memberScript.StartPause,
            EnableLoop = memberScript.EnableLoop,
            StartValueMs = memberScript.StartValueMs,
            VideoFps = memberScript.VideoFps,


            // Common
            DateCreated = memberScript.Created.GetValueOrDefault(),
            DateModified = memberScript.Modified.GetValueOrDefault(),
            MediaContentType = memberScript.MediaContentType ?? "",
        };
    }

    private static string DecodeText(BlLegacyText text, int directorVersion, ILogger logger)
    {
        return text.Format switch
        {
            BlLegacyTextFormatKind.Stxt => XmedExtensions.DecodeSTXT(text.Bytes),
            BlLegacyTextFormatKind.Xmed => DecodeStyledText(text.Bytes, directorVersion, logger),
            _ => string.Empty
        };
    }

    private static string DecodeField(BlLegacyField field, int directorVersion, ILogger logger)
    {
        return field.Format switch
        {
            BlLegacyFieldFormatKind.Stxt => XmedExtensions.DecodeSTXT(field.Bytes),
            BlLegacyFieldFormatKind.Xmed => DecodeStyledText(field.Bytes, directorVersion, logger),
            _ => string.Empty
        };
    }

    private static string DecodeStyledText(byte[] data, int directorVersion, ILogger logger)
    {
        var reader = new BlXmedTextReader(logger);
        var document = directorVersion > 0 ? reader.Read(data, directorVersion) : reader.Read(data);
        return BlXmedMarkdownConverter.ToCustomMarkdown(document);
    }

    private static string EnsureUniqueFileName(string fileName, HashSet<string> usedNames)
    {
        if (usedNames.Add(fileName))
            return fileName;

        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);
        var index = 1;
        string candidate;
        do
        {
            candidate = $"{nameWithoutExtension}_{index}{extension}";
            index++;
        }
        while (!usedNames.Add(candidate));

        return candidate;
    }

    private static BlingoMemberTypeDTO MapMemberType(BlLegacyCastMemberType type)
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
}
