
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
        return ConvertArchive(archive, movieName);
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
        return ConvertArchive(archive, movieName);
    }

    public (BlingoStageDTO Stage, BlingoMovieDTO Movie, DirFilesContainerDTO Resources) Import(
        BlLegacyMovieArchive archive,
        string movieName)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(movieName);

        return ConvertArchive(archive, movieName);
    }

    private (BlingoStageDTO Stage, BlingoMovieDTO Movie, DirFilesContainerDTO Resources) ConvertArchive(
        BlLegacyMovieArchive archive,
        string movieName)
    {
        var resources = new DirFilesContainerDTO();
        foreach (var resource in archive.RawResources.Files)
        {
            resources.Files.Add(new DirFileResourceDTO
            {
                CastName = resource.CastName,
                FileName = resource.FileName,
                Bytes = resource.Bytes
            });
        }

        var stage = archive.ToBlingoStage();
        var movie = archive.ToBlingo(movieName, resources,_logger);
        return (stage, movie, resources);
    }
}
