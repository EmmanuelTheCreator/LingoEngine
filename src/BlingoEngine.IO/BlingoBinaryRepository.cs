using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Data.DTO.Members;

namespace BlingoEngine.IO;

public class BlingoBinaryRepository
{
    private readonly Dictionary<(int CastLibNum, int MemberNum), (BlingoCastDTO Cast, BlingoMemberDTO Member)> _membersByKey = new();
    private readonly Dictionary<(int CastLibNum, int MemberNum), string> _memberFilePaths = new();
    private readonly Dictionary<int, string> _castDirectories = new();

    private string _baseDirectory = string.Empty;

    public void Initialize(BlingoMovieDTO movieDto, string baseDirectory)
    {
        ArgumentNullException.ThrowIfNull(movieDto);

        _membersByKey.Clear();
        _memberFilePaths.Clear();
        _castDirectories.Clear();

        _baseDirectory = baseDirectory;
        Directory.CreateDirectory(_baseDirectory);

        foreach (var cast in movieDto.Casts)
        {
            _castDirectories[cast.Number] = ComputeCastDirectory(cast.Name, cast.Number);
            Directory.CreateDirectory(_castDirectories[cast.Number]);

            foreach (var member in cast.Members)
                _membersByKey[(member.CastLibNum, member.NumberInCast)] = (cast, member);
        }
    }

    public void PersistResources(DirFilesContainerDTO resources)
    {
        ArgumentNullException.ThrowIfNull(resources);

        foreach (var resource in resources.Files)
        {
            if (resource.Bytes == null || resource.Bytes.Length == 0)
                continue;

            var key = (resource.CastLibNum, resource.NumberInCast);
            var targetPath = ResolveTargetPath(key, resource.FileName, resource.CastName);
            Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
            File.WriteAllBytes(targetPath, resource.Bytes);
            _memberFilePaths[key] = targetPath;
        }
    }

    public string ResolveBinaryPath(BlingoMemberDTO memberDto, string? referencedFile)
    {
        var key = (memberDto.CastLibNum, memberDto.NumberInCast);
        if (_memberFilePaths.TryGetValue(key, out var stored) && File.Exists(stored))
            return stored;

        var targetPath = ResolveTargetPath(key, referencedFile, null);

        if (!File.Exists(targetPath))
        {
            var sourceCandidates = GetCandidatePaths(referencedFile)
                .Concat(GetCandidatePaths(memberDto.FileName));

            foreach (var candidate in sourceCandidates)
            {
                if (File.Exists(candidate))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                    File.Copy(candidate, targetPath, true);
                    break;
                }
            }
        }

        _memberFilePaths[key] = targetPath;
        return targetPath;
    }

    public string WriteScriptFile(BlingoMemberScriptDTO scriptDto)
    {
        var key = (scriptDto.CastLibNum, scriptDto.NumberInCast);
        var targetPath = ResolveTargetPath(key, scriptDto.LinkedFilePath, null, scriptDto.IsJavascript ? ".js" : ".lingo");
        Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
        File.WriteAllText(targetPath, scriptDto.Script ?? string.Empty);
        _memberFilePaths[key] = targetPath;
        return targetPath;
    }

    public string? TryGetStoredPath(BlingoMemberDTO memberDto)
    {
        var key = (memberDto.CastLibNum, memberDto.NumberInCast);
        return _memberFilePaths.TryGetValue(key, out var stored) ? stored : null;
    }

    private string ResolveTargetPath((int CastLibNum, int MemberNum) key, string? originalFileName, string? castName, string? forcedExtension = null)
    {
        if (_membersByKey.TryGetValue(key, out var entry))
            return BuildMemberPath(entry.Cast, entry.Member, originalFileName, forcedExtension);

        return BuildFallbackPath(key.CastLibNum, key.MemberNum, castName, originalFileName, forcedExtension);
    }

    private string BuildMemberPath(BlingoCastDTO cast, BlingoMemberDTO member, string? originalFileName, string? forcedExtension)
    {
        var castDir = GetCastDirectory(cast.Name, cast.Number);
        var extension = !string.IsNullOrWhiteSpace(forcedExtension)
            ? NormalizeExtension(forcedExtension)
            : DetermineExtension(member, originalFileName);
        var safeMemberName = Sanitize(member.Name, $"Member_{member.NumberInCast.ToString(CultureInfo.InvariantCulture)}");
        var fileName = $"{member.NumberInCast.ToString(CultureInfo.InvariantCulture)}_{safeMemberName}{extension}";
        return Path.Combine(castDir, fileName);
    }

    private string BuildFallbackPath(int castLibNum, int memberNumber, string? castName, string? originalFileName, string? forcedExtension)
    {
        var castDir = GetCastDirectory(castName, castLibNum);
        var extension = !string.IsNullOrWhiteSpace(forcedExtension)
            ? NormalizeExtension(forcedExtension)
            : NormalizeExtension(Path.GetExtension(originalFileName));
        if (string.IsNullOrEmpty(extension))
            extension = ".bin";

        var baseName = Path.GetFileNameWithoutExtension(originalFileName);
        if (string.IsNullOrWhiteSpace(baseName))
            baseName = $"resource_{memberNumber.ToString(CultureInfo.InvariantCulture)}";

        var safeMemberName = Sanitize(baseName, $"resource_{memberNumber.ToString(CultureInfo.InvariantCulture)}");
        var fileName = $"{memberNumber.ToString(CultureInfo.InvariantCulture)}_{safeMemberName}{extension}";
        return Path.Combine(castDir, fileName);
    }

    private string GetCastDirectory(string? castName, int castNumber)
    {
        if (_castDirectories.TryGetValue(castNumber, out var existing))
            return existing;

        var directory = ComputeCastDirectory(castName, castNumber);
        Directory.CreateDirectory(directory);
        _castDirectories[castNumber] = directory;
        return directory;
    }

    private string ComputeCastDirectory(string? castName, int castNumber)
    {
        if (!string.IsNullOrWhiteSpace(castName))
        {
            var trimmed = castName.Trim();
            var sanitized = MediaFileNameHelper.SanitizeFileName(trimmed);
            if (!string.IsNullOrEmpty(sanitized))
                return Path.Combine(_baseDirectory, sanitized);
        }

        var fallback = castNumber > 0
            ? castNumber.ToString(CultureInfo.InvariantCulture)
            : "Cast";
        var safeFallback = MediaFileNameHelper.SanitizeFileName(fallback);
        return Path.Combine(_baseDirectory, string.IsNullOrEmpty(safeFallback) ? "Cast" : safeFallback);
    }

    private IEnumerable<string> GetCandidatePaths(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            yield break;

        var trimmed = path.Trim();
        if (Path.IsPathRooted(trimmed))
        {
            yield return trimmed;
        }
        else
        {
            yield return Path.Combine(_baseDirectory, Path.GetFileName(trimmed));
            yield return Path.Combine(_baseDirectory, trimmed);
        }
    }

    private string DetermineExtension(BlingoMemberDTO member, string? originalFileName)
    {
        var ext = NormalizeExtension(Path.GetExtension(originalFileName));
        if (!string.IsNullOrEmpty(ext))
            return ext;

        ext = NormalizeExtension(Path.GetExtension(member.FileName));
        if (!string.IsNullOrEmpty(ext))
            return ext;

        ext = GetExtensionFromContentType(member.MediaContentType);
        if (!string.IsNullOrEmpty(ext))
            return ext;

        return GetDefaultExtension(member.Type);
    }

    private static string NormalizeExtension(string? extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
            return string.Empty;

        extension = extension.Trim();
        if (!extension.StartsWith('.'))
            extension = "." + extension;

        return extension.ToLowerInvariant();
    }

    private static string GetExtensionFromContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return string.Empty;

        var trimmed = contentType.Trim();
        var normalized = trimmed.ToLowerInvariant();

        var extension = normalized switch
        {
            "image/jpeg" or "image/jpg" or "kmoacfformat_jpeg" => ".jpg",
            "image/png" or "kmoacfformat_png" => ".png",
            "image/gif" or "animated gif..." => ".gif",
            "image/bmp" => ".bmp",
            "image/tiff" or "kmoacfformat_tiff" => ".tiff",
            "audio/mpeg" or "audio/mp3" or "kmoacfformat_mpeg3" => ".mp3",
            "audio/wav" or "audio/x-wav" or "kmoacfformat_wav" => ".wav",
            "audio/aiff" or "audio/x-aiff" or "kmoacfformat_aiff" => ".aiff",
            "video/quicktime" or "quicktimemedia" => ".mov",
            "video/mp4" => ".mp4",
            "video/x-msvideo" or "avi" => ".avi",
            "text/plain" => ".txt",
            "application/javascript" or "text/javascript" => ".js",
            "bitmap" or "bitmappainted" => ".png",
            "anim.gif" or "animgif" => ".gif",
            "mp3" => ".mp3",
            "wav" => ".wav",
            "aiff" => ".aiff",
            "script" => ".lingo",
            _ => string.Empty
        };

        if (!string.IsNullOrEmpty(extension))
            return extension;

        return trimmed switch
        {
            "kMoaCfFormat_JPEG" => ".jpg",
            "kMoaCfFormat_PNG" => ".png",
            "Animated GIF..." => ".gif",
            "kMoaCfFormat_MPEG3" => ".mp3",
            "kMoaCfFormat_WAV" => ".wav",
            "kMoaCfFormat_AIFF" => ".aiff",
            "kMoaCfFormat_TIFF" => ".tiff",
            _ => string.Empty
        };
    }

    private static string GetDefaultExtension(BlingoMemberTypeDTO type)
    {
        return type switch
        {
            BlingoMemberTypeDTO.Bitmap or BlingoMemberTypeDTO.Picture => ".png",
            BlingoMemberTypeDTO.Animgif => ".gif",
            BlingoMemberTypeDTO.Sound => ".wav",
            BlingoMemberTypeDTO.Text or BlingoMemberTypeDTO.Field => ".txt",
            BlingoMemberTypeDTO.Script => ".lingo",
            BlingoMemberTypeDTO.QuickTimeMedia or BlingoMemberTypeDTO.DigitalVideo => ".mov",
            BlingoMemberTypeDTO.RealMedia => ".rm",
            BlingoMemberTypeDTO.FilmLoop => ".flp",
            _ => ".bin"
        };
    }

    private static string Sanitize(string? value, string fallback)
    {
        var working = string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : MediaFileNameHelper.SanitizeFileName(value.Trim());

        if (string.IsNullOrEmpty(working))
        {
            var fallbackValue = fallback?.Trim() ?? string.Empty;
            working = MediaFileNameHelper.SanitizeFileName(fallbackValue);
        }

        return string.IsNullOrEmpty(working)
            ? "resource"
            : working;
    }
}
