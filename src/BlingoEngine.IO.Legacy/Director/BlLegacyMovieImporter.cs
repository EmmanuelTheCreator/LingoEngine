using System;
using System.IO;

using BlingoEngine.IO.Data.DTO;
using Microsoft.Extensions.Logging;

namespace BlingoEngine.IO.Legacy.Director;

/// <summary>
/// Provides a high level façade that reads legacy Director movies and converts them into
/// <see cref="BlingoEngine.IO.Data.DTO"/> transfer objects fully within the legacy layer.
/// </summary>
public sealed class BlLegacyMovieImporter
{
    private readonly BlLegacyMovieReader _reader;
    private readonly ILogger _logger;

    public BlLegacyMovieImporter(ILogger logger)
        : this(logger, new BlLegacyMovieReader())
    {
        
    }

    public BlLegacyMovieImporter(ILogger logger, BlLegacyMovieReader reader)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        _logger = logger;
    }

    public (BlingoStageDTO Stage, BlingoMovieDTO Movie, DirFilesContainerDTO Resources) Import(string filePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        var archive = _reader.Read(filePath);
        var movieName = Path.GetFileNameWithoutExtension(filePath) ?? string.Empty;
        var sourceDirectory = ResolveSourceDirectory(filePath);
        return ConvertArchive(archive, movieName, sourceDirectory);
    }

    public (BlingoStageDTO Stage, BlingoMovieDTO Movie, DirFilesContainerDTO Resources) Import(
        Stream stream,
        string fileName,
        bool leaveOpen = false)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentException.ThrowIfNullOrEmpty(fileName);

        var archive = _reader.Read(stream, fileName, leaveOpen);
        var movieName = Path.GetFileNameWithoutExtension(fileName) ?? string.Empty;
        var sourceDirectory = ResolveSourceDirectory(fileName);
        return ConvertArchive(archive, movieName, sourceDirectory);
    }

    public (BlingoStageDTO Stage, BlingoMovieDTO Movie, DirFilesContainerDTO Resources) Import(
        BlLegacyMovieArchive archive,
        string movieName)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(movieName);

        return ConvertArchive(archive, movieName, null);
    }

    private (BlingoStageDTO Stage, BlingoMovieDTO Movie, DirFilesContainerDTO Resources) ConvertArchive(
        BlLegacyMovieArchive archive,
        string movieName,
        string? sourceDirectory)
    {
        var resources = new DirFilesContainerDTO();
        var stage = archive.ToBlingoStage();
        var movie = archive.ToBlingo(movieName, resources, _logger);
        ImportExternalCasts(archive, sourceDirectory, movie, resources);
        return (stage, movie, resources);
    }

    private void ImportExternalCasts(
        BlLegacyMovieArchive archive,
        string? sourceDirectory,
        BlingoMovieDTO movie,
        DirFilesContainerDTO resources)
    {
        if (movie.Casts.Count == 0)
            return;

        for (var index = 0; index < archive.CastLibraries.Count && index < movie.Casts.Count; index++)
        {
            var cast = archive.CastLibraries[index];
            if (cast.IsInternal)
                continue;

            var resolvedPath = ResolveExternalCastPath(cast.CastPath, sourceDirectory);
            if (resolvedPath is null)
            {
                if (!string.IsNullOrWhiteSpace(cast.CastPath))
                    _logger?.LogWarning("External cast '{CastPath}' could not be resolved.", cast.CastPath);
                continue;
            }

            try
            {
                var resourceStartIndex = resources.Files.Count;
                var externalArchive = _reader.Read(resolvedPath);
                var externalMovie = externalArchive.ToBlingo(
                    Path.GetFileNameWithoutExtension(resolvedPath) ?? string.Empty,
                    resources,
                    _logger);

                if (externalMovie.Casts.Count == 0)
                    continue;

                var replacement = externalMovie.Casts[0];
                replacement.Name = string.IsNullOrWhiteSpace(cast.Name) ? replacement.Name : cast.Name;
                replacement.Number = movie.Casts[index].Number;
                replacement.PreLoadMode = BlLegacyMovieBlingoExtensions.MapPreloadMode(cast.Preload);
                replacement.FileName = Path.GetFileName(resolvedPath) ?? string.Empty;

                foreach (var member in replacement.Members)
                    member.CastLibNum = replacement.Number;

                for (var resourceIndex = resourceStartIndex; resourceIndex < resources.Files.Count; resourceIndex++)
                {
                    var resource = resources.Files[resourceIndex];
                    resource.CastName = replacement.Name;
                    resource.CastLibNum = replacement.Number;
                }

                movie.Casts[index] = replacement;
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to import external cast '{CastPath}'.", cast.CastPath);
            }
        }
    }

    private static string? ResolveSourceDirectory(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        try
        {
            var hasSeparator = path.Contains(Path.DirectorySeparatorChar) || path.Contains(Path.AltDirectorySeparatorChar);
            if (!hasSeparator && !Path.IsPathRooted(path))
                return null;

            var fullPath = Path.GetFullPath(path);
            return Path.GetDirectoryName(fullPath);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static string? ResolveExternalCastPath(string? castPath, string? sourceDirectory)
    {
        if (string.IsNullOrWhiteSpace(castPath))
            return null;

        var trimmed = castPath.Trim();
        if (Path.IsPathRooted(trimmed) && File.Exists(trimmed))
            return trimmed;

        if (string.IsNullOrEmpty(sourceDirectory))
            return null;

        var normalized = trimmed.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);

        var candidate = Path.Combine(sourceDirectory, normalized);
        if (File.Exists(candidate))
            return candidate;

        var root = Path.GetPathRoot(normalized);
        if (!string.IsNullOrEmpty(root) && normalized.Length > root.Length)
        {
            var withoutRoot = normalized[root.Length..].TrimStart(Path.DirectorySeparatorChar);
            if (!string.IsNullOrEmpty(withoutRoot))
            {
                candidate = Path.Combine(sourceDirectory, withoutRoot);
                if (File.Exists(candidate))
                    return candidate;
            }
        }

        var fileName = Path.GetFileName(normalized);
        if (string.IsNullOrEmpty(fileName))
            return null;

        candidate = Path.Combine(sourceDirectory, fileName);
        if (File.Exists(candidate))
            return candidate;

        try
        {
            var matches = Directory.GetFiles(sourceDirectory, fileName, SearchOption.AllDirectories);
            if (matches.Length > 0)
                return matches[0];
        }
        catch (Exception)
        {
        }

        return null;
    }
}
