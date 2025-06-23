using System;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using ProjectorRays.Director;
using ProjectorRays.IO;
using ProjectorRays.Common;
using LingoEngine.IO.Data.DTO;

namespace LingoEngine.Director.Core.Import;

/// <summary>
/// Utility to convert Director files using the ProjectorRays library into
/// LingoEngine data transfer objects.
/// The conversion is minimal and only extracts data required to load a movie.
/// </summary>
public static class DirectorRaysImporter
{
    public static (LingoMovieDTO Movie, DirFilesContainerDTO Resources) ImportMovie(string filePath)
    {
        byte[] data = File.ReadAllBytes(filePath);
        var stream = new ReadStream(data, data.Length, Endianness.BigEndian);
        var dir = new DirectorFile(NullLogger.Instance);
        if (!dir.Read(stream))
            throw new Exception($"Failed to read Director file '{filePath}'");

        var resources = new DirFilesContainerDTO();
        var movie = dir.ToDto(Path.GetFileNameWithoutExtension(filePath), resources);
        return (movie, resources);
    }
}
