
    using global::Director.Primitives;

    namespace Director
    {
        public class LingoArchive
        {
            private readonly Archive _archive;

            public LingoArchive(Archive archive)
            {
                _archive = archive ?? throw new ArgumentNullException(nameof(archive));
            }

            public bool HasResource(uint tag, int id) => _archive.HasResource(tag, id);

            public Stream? OpenResource(uint tag, int id)
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
                stream.CopyTo(ms);
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
        }
    }

public class Archive
{
    private readonly Dictionary<(uint tag, int id), byte[]> _resources = new();
    private readonly Dictionary<uint, List<ResourceEntry>> _resourcesByTag = new();
    public class ResourceEntry
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public uint Tag { get; set; }
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

    public Stream GetResource(uint tag, int id)
    {
        if (!_resources.TryGetValue((tag, id), out var data))
            throw new InvalidOperationException($"Resource {tag}#{id} not found");
        return new MemoryStream(data, writable: false);
    }

    public IEnumerable<(uint Tag, int Id)> GetAllResourceKeys()
    {
        foreach (var key in _resources.Keys)
            yield return key;
    }
}

