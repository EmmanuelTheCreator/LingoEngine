using BlingoEngine.IO.Legacy.Bitmaps;
using BlingoEngine.IO.Legacy.Cast;
using BlingoEngine.IO.Legacy.Core;
using BlingoEngine.IO.Legacy.Data;
using BlingoEngine.IO.Legacy.Fields;
using BlingoEngine.IO.Legacy.Files;
using BlingoEngine.IO.Legacy.Scores;
using BlingoEngine.IO.Legacy.Sounds;
using BlingoEngine.IO.Legacy.Texts;

namespace BlingoEngine.IO.Legacy.Director;

/// <summary>
/// Reads legacy Director movie archives and produces <see cref="BlLegacyMovieArchive"/> instances
/// that expose the decoded cast members and media payloads.
/// </summary>
public sealed class BlLegacyMovieReader
{
    public BlLegacyMovieArchive Read(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        using var stream = File.OpenRead(path);
        var fileName = Path.GetFileName(path);
        return Read(stream, fileName, leaveOpen: false);
    }

    public BlLegacyMovieArchive Read(Stream stream, string fileName, bool leaveOpen = false)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(fileName);

        using var context = new ReaderContext(stream, fileName, leaveOpen);
        return ReadFromContext(context);
    }

    private static BlLegacyMovieArchive ReadFromContext(ReaderContext context)
    {
        var dirFile = new BlDirFile(context);
        var rawResources = dirFile.Read();

        var casts = context.ReadCastLibraries();
        var texts = context.ReadTexts();
        var fields = context.ReadFields();
        var bitmaps = context.ReadBitmaps();
        var sounds = context.ReadSounds();
        var scoreReader = new BlLegacyScoreReader(context);
        var score = scoreReader.Read();

        var children = new Dictionary<int, IReadOnlyList<BlResourceKeyLink>>();
        foreach (var pair in context.Resources.ChildrenByParent)
            children[pair.Key] = pair.Value.ToArray();

        var parent = new Dictionary<int, BlResourceKeyLink>();
        foreach (var pair in context.Resources.ParentByChild)
            parent[pair.Key] = pair.Value;

        var directorVersion = context.DataBlock?.Format.DirectorVersion ?? 0;

        return new BlLegacyMovieArchive(
            context.FileName,
            directorVersion,
            rawResources,
            casts.ToList(),
            texts.ToList(),
            fields.ToList(),
            bitmaps.ToList(),
            sounds.ToList(),
            children,
            parent,
            score);
    }
}
