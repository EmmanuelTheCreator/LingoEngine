using Director.Primitives;
using Director.Scripts;
using Director.Tools;

namespace Director
{
    public class LingoArchive
    {
        private readonly Archive _archive;
        private readonly List<LingoScript> _scripts = new();

        public LingoArchive(Archive archive)
        {
            _archive = archive ?? throw new ArgumentNullException(nameof(archive));
        }

        public bool HasResource(uint tag, int id) => _archive.HasResource(tag, id);

        public SeekableReadStreamEndian? OpenResource(uint tag, int id)
        {
            if (!_archive.HasResource(tag, id))
                return null;
            return _archive.GetResource(tag, id);
        }

        public byte[]? LoadResource(uint tag, int id)
        {
            using var stream = OpenResource(tag, id);
            if (stream == null)
                return null;

            using var ms = new MemoryStream();
            stream.BaseStream.CopyTo(ms);  
            return ms.ToArray();
        }

        public IEnumerable<(int id, string name)> ListResources(uint tag)
        {
            foreach (var res in _archive.ListResources(tag))
            {
                yield return (res.Id, res.Name ?? string.Empty);
            }
        }

        public bool TryGetString(uint tag, int id, out string result)
        {
            result = string.Empty;
            var data = LoadResource(tag, id);
            if (data == null) return false;
            result = System.Text.Encoding.ASCII.GetString(data);
            return true;
        }

        public PaletteV4? GetPalette(Cast cast, int castId)
        {
            var paletteId = castId + cast.CastIDOffset;
            if (!HasResource(ResourceTags.CLUT, paletteId))
                return null;

            using var stream = OpenResource(ResourceTags.CLUT, paletteId);
            if (stream == null)
                return null;

            var palData = cast.LoadPalette(stream, paletteId);
            palData.Id = new CastMemberID(castId, cast.CastLibID);
            return new PaletteV4(palData);
        }

        public void AddCode(string source, ScriptType type, int id, string? name = null, ScriptFlags flags = ScriptFlags.None)
        {
            if ((flags & ScriptFlags.TrimGarbage) != 0)
            {
                source = source.TrimEnd('\0', ' ', '\t', '\r', '\n');
            }

            var script = new LingoScript
            {
                Id = id,
                Type = type,
                Name = name ?? $"Script_{id}",
                Source = source,
                Flags = flags
            };

            _scripts[id] = script;
        }
    }


    public class Archive
{
    private readonly Dictionary<(uint tag, int id), byte[]> _resources = new();
    private readonly Dictionary<uint, List<ResourceEntry>> _resourcesByTag = new();
    private string _fileName = string.Empty;
    private bool _isBigEndian;

    public class ResourceEntry
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public uint Tag { get; set; }
    }

    public Archive(bool isBigEndian = true)
    {
        _isBigEndian = isBigEndian;
    }

    public string GetFileName() => _fileName;

    public string GetName(string tag, int id) => GetName(ResourceTags.ToTag(tag), id);
    public string GetName(uint numericTag, int id)
    {
        if (_resourcesByTag.TryGetValue(numericTag, out var entries))
        {
            var entry = entries.FirstOrDefault(e => e.Id == id);
            return entry?.Name ?? string.Empty;
        }
        return string.Empty;
    }

    public IEnumerable<ResourceEntry> ListResources(uint tag)
    {
        if (_resourcesByTag.TryGetValue(tag, out var entries))
        {
            foreach (var entry in entries)
            {
                yield return entry;
            }
        }
    }

    public void AddResource(uint tag, int id, byte[] data)
    {
        _resources[(tag, id)] = data;

        if (!_resourcesByTag.TryGetValue(tag, out var list))
        {
            list = new List<ResourceEntry>();
            _resourcesByTag[tag] = list;
        }

        list.Add(new ResourceEntry
        {
            Id = id,
            Name = null,
            Tag = tag
        });
    }


    public bool HasResource(uint tag, int id)
    {
        return _resources.ContainsKey((tag, id));
    }

    public SeekableReadStreamEndian GetResource(uint tag, int id)
    {
        if (!_resources.TryGetValue((tag, id), out var data))
            throw new InvalidOperationException($"Resource {tag}#{id} not found");
        return new SeekableReadStreamEndian(new MemoryStream(data, writable: false), isBigEndian: _isBigEndian);
    }

    public IEnumerable<(uint Tag, int Id)> GetAllResourceKeys()
    {
        foreach (var key in _resources.Keys)
            yield return key;
    }

    /// <summary>
    /// Retrieves the first resource with the specified tag, or null if not found.
    /// This mimics the Director behavior of tag-only lookups (id defaults to 0).
    /// </summary>
    /// <param name="tag">The 4-byte resource tag.</param>
    /// <returns>A seekable endian-aware stream, or null if not present.</returns>
    public SeekableReadStreamEndian? GetMovieResourceIfPresent(uint tag)
    {
        const int defaultId = 0;
        if (!_resources.TryGetValue((tag, defaultId), out var data))
            return null;

        return new SeekableReadStreamEndian(new MemoryStream(data, writable: false), isBigEndian: _isBigEndian);
    }

}

