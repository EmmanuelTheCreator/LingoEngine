using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Legacy.Director;
using Microsoft.Extensions.Logging;

namespace BlingoEngine.Director.Core.Importer;

/// <summary>
/// Utility to convert Director files through the BlingoEngine legacy importer into data transfer
/// objects.
/// </summary>
public static class LegacyImporter
{
    public static (BlingoStageDTO Stage, BlingoMovieDTO Movie, DirFilesContainerDTO Resources) ImportMovie(string filePath, ILogger logger)
    {
        var importer = new BlLegacyMovieImporter(logger);
        return importer.Import(filePath);
    }
}
