using System.Diagnostics.CodeAnalysis;

using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Legacy.Bitmaps;
using BlingoEngine.IO.Legacy.Cast;
using BlingoEngine.IO.Legacy.Data;
using BlingoEngine.IO.Legacy.Fields;
using BlingoEngine.IO.Legacy.Scores;
using BlingoEngine.IO.Legacy.Sounds;
using BlingoEngine.IO.Legacy.Texts.Data;

namespace BlingoEngine.IO.Legacy.Director;

/// <summary>
/// Represents the high-level data extracted from a legacy Director movie. The archive groups cast
/// libraries, media payloads, and resource relationships so editor tooling can build DTOs without
/// repeating the low-level parsing steps handled by the legacy reader.
/// </summary>
public sealed class BlLegacyMovieArchive
{
    private readonly Dictionary<int, BlLegacyText> _textsByCastId;
    private readonly Dictionary<int, BlLegacyField> _fieldsByCastId;
    private readonly Dictionary<int, BlLegacyBitmap> _bitmapsByCastId;
    private readonly Dictionary<int, BlLegacySound> _soundsByCastId;

    /// <summary>
    /// Gets the logical file name associated with the archive.
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// Gets the Director version detected in the movie header.
    /// </summary>
    public int DirectorVersion { get; }

    /// <summary>
    /// Gets the raw resource container built while parsing the archive.
    /// </summary>
    public DirFilesContainerDTO RawResources { get; }

    /// <summary>
    /// Gets the list of cast libraries discovered in the <c>CAS*</c> tables.
    /// </summary>
    public IReadOnlyList<BlLegacyRawCastLibrary> CastLibraries { get; }

    /// <summary>
    /// Gets the resource relationships registered in the <c>KEY*</c> table, grouped by parent.
    /// </summary>
    public IReadOnlyDictionary<int, IReadOnlyList<BlResourceKeyLink>> ChildrenByParent { get; }

    /// <summary>
    /// Gets a lookup that maps child resources back to their parent entries.
    /// </summary>
    public IReadOnlyDictionary<int, BlResourceKeyLink> ParentByChild { get; }

    internal BlLegacyScore? Score { get; }


    internal BlLegacyMovieArchive(
        string fileName,
        int directorVersion,
        DirFilesContainerDTO rawResources,
        IReadOnlyList<BlLegacyRawCastLibrary> casts,
        IReadOnlyList<BlLegacyText> texts,
        IReadOnlyList<BlLegacyField> fields,
        IReadOnlyList<BlLegacyBitmap> bitmaps,
        IReadOnlyList<BlLegacySound> sounds,
        IReadOnlyDictionary<int, IReadOnlyList<BlResourceKeyLink>> childrenByParent,
        IReadOnlyDictionary<int, BlResourceKeyLink> parentByChild,
        BlLegacyScore? score)
    {
        ArgumentNullException.ThrowIfNull(rawResources);
        ArgumentNullException.ThrowIfNull(casts);
        ArgumentNullException.ThrowIfNull(texts);
        ArgumentNullException.ThrowIfNull(fields);
        ArgumentNullException.ThrowIfNull(bitmaps);
        ArgumentNullException.ThrowIfNull(sounds);
        ArgumentNullException.ThrowIfNull(childrenByParent);
        ArgumentNullException.ThrowIfNull(parentByChild);

        FileName = fileName;
        DirectorVersion = directorVersion;
        RawResources = rawResources;
        CastLibraries = casts;
        ChildrenByParent = childrenByParent;
        ParentByChild = parentByChild;
        Score = score;

        _textsByCastId = BuildTextLookup(texts, ParentByChild);
        _fieldsByCastId = BuildFieldLookup(fields, ParentByChild);
        _bitmapsByCastId = BuildBitmapLookup(bitmaps, ParentByChild);
        _soundsByCastId = BuildSoundLookup(sounds, ParentByChild);
    }



    public bool TryGetText(int castResourceId, [NotNullWhen(true)] out BlLegacyText? text)
        => _textsByCastId.TryGetValue(castResourceId, out text);

    public bool TryGetField(int castResourceId, [NotNullWhen(true)] out BlLegacyField? field)
        => _fieldsByCastId.TryGetValue(castResourceId, out field);

    public bool TryGetBitmap(int castResourceId, [NotNullWhen(true)] out BlLegacyBitmap? bitmap)
        => _bitmapsByCastId.TryGetValue(castResourceId, out bitmap);

    public bool TryGetSound(int castResourceId, [NotNullWhen(true)] out BlLegacySound? sound)
        => _soundsByCastId.TryGetValue(castResourceId, out sound);

    private static Dictionary<int, BlLegacyText> BuildTextLookup(
        IEnumerable<BlLegacyText> texts,
        IReadOnlyDictionary<int, BlResourceKeyLink> parentByChild)
    {
        var map = new Dictionary<int, BlLegacyText>();
        foreach (var text in texts)
        {
            var registered = false;
            if (parentByChild.TryGetValue(text.ResourceId, out var link))
            {
                RegisterText(map, link.ParentId, text);
                registered = true;
            }

            if (!registered)
                RegisterText(map, text.ResourceId, text);
        }

        return map;
    }

    private static void RegisterText(Dictionary<int, BlLegacyText> map, int castId, BlLegacyText candidate)
    {
        if (castId <= 0)
            return;

        if (map.TryGetValue(castId, out var existing))
        {
            if (existing.Format == BlLegacyTextFormatKind.Xmed && candidate.Format != BlLegacyTextFormatKind.Xmed)
                return;

            if (candidate.Format == BlLegacyTextFormatKind.Xmed && existing.Format != BlLegacyTextFormatKind.Xmed)
                map[castId] = candidate;

            return;
        }

        map[castId] = candidate;
    }

    private static Dictionary<int, BlLegacyField> BuildFieldLookup(
        IEnumerable<BlLegacyField> fields,
        IReadOnlyDictionary<int, BlResourceKeyLink> parentByChild)
    {
        var map = new Dictionary<int, BlLegacyField>();
        foreach (var field in fields)
        {
            var registered = false;
            if (parentByChild.TryGetValue(field.ResourceId, out var link))
            {
                RegisterField(map, link.ParentId, field);
                registered = true;
            }

            if (!registered)
                RegisterField(map, field.ResourceId, field);
        }

        return map;
    }

    private static void RegisterField(Dictionary<int, BlLegacyField> map, int castId, BlLegacyField candidate)
    {
        if (castId <= 0)
            return;

        if (map.TryGetValue(castId, out var existing))
        {
            if (existing.Format == BlLegacyFieldFormatKind.Xmed && candidate.Format != BlLegacyFieldFormatKind.Xmed)
                return;

            if (candidate.Format == BlLegacyFieldFormatKind.Xmed && existing.Format != BlLegacyFieldFormatKind.Xmed)
                map[castId] = candidate;

            return;
        }

        map[castId] = candidate;
    }

    private static Dictionary<int, BlLegacyBitmap> BuildBitmapLookup(
        IEnumerable<BlLegacyBitmap> bitmaps,
        IReadOnlyDictionary<int, BlResourceKeyLink> parentByChild)
    {
        var map = new Dictionary<int, BlLegacyBitmap>();
        foreach (var bitmap in bitmaps)
        {
            if (!parentByChild.TryGetValue(bitmap.ResourceId, out var link))
                continue;

            if (bitmap.Format == BlLegacyBitmapFormatKind.AlphaMask || bitmap.Format == BlLegacyBitmapFormatKind.Thumbnail)
                continue;

            if (!map.ContainsKey(link.ParentId))
                map[link.ParentId] = bitmap;
        }

        return map;
    }

    private static Dictionary<int, BlLegacySound> BuildSoundLookup(
        IEnumerable<BlLegacySound> sounds,
        IReadOnlyDictionary<int, BlResourceKeyLink> parentByChild)
    {
        var map = new Dictionary<int, BlLegacySound>();
        foreach (var sound in sounds)
        {
            if (!parentByChild.TryGetValue(sound.ResourceId, out var link))
                continue;

            if (!map.ContainsKey(link.ParentId))
                map[link.ParentId] = sound;
        }

        return map;
    }
}
