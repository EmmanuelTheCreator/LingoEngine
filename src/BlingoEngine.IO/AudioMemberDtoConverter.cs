using BlingoEngine.IO.Data.DTO.Members;
using BlingoEngine.Sounds;
using System.IO;

namespace BlingoEngine.IO;

internal static class AudioMemberDtoConverter
{
    public static BlingoMemberSoundDTO ToDto(this BlingoMemberSound sound, BlingoMemberDTO baseDto, JsonStateRepository.MovieStoreOptions options)
    {
        var dto = MemberDtoConverter.PopulateBase(baseDto, new BlingoMemberSoundDTO());
        dto.Stereo = sound.ImportedStereo ?? sound.Stereo;
        dto.Length = sound.OriginalLength > 0 ? sound.OriginalLength : sound.Length;
        dto.Loop = sound.Loop;
        dto.IsLinked = sound.IsLinked;
        dto.LinkedFilePath = sound.LinkedFilePath;
        dto.SoundFile = SaveSound(sound, options);
        return dto;
    }

    private static string SaveSound(BlingoMemberSound sound, JsonStateRepository.MovieStoreOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.TargetDirectory))
        {
            return string.Empty;
        }

        var source = !string.IsNullOrEmpty(sound.FileName) && File.Exists(sound.FileName)
            ? sound.FileName
            : sound.LinkedFilePath;

        if (string.IsNullOrEmpty(source) || !File.Exists(source))
        {
            return string.Empty;
        }

        var ext = GetSoundExtension(source);
        var name = $"{sound.NumberInCast}_{MediaFileNameHelper.SanitizeFileName(sound.Name)}{ext}";
        if (!options.WithMedia)
        {
            return name;
        }

        var dest = Path.Combine(options.TargetDirectory, name);
        File.Copy(source, dest, true);
        return name;
    }

    private static string GetSoundExtension(string path)
    {
        var ext = Path.GetExtension(path);
        if (string.IsNullOrEmpty(ext))
        {
            return ".wav";
        }

        return ext.ToLowerInvariant();
    }
}

