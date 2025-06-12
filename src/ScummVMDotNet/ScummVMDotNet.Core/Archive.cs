using Director.IO;
using Director.Primitives;

namespace Director
{


    public class Archive
    {
        private readonly Dictionary<(uint tag, int id), byte[]> _resources = new();
        private readonly Dictionary<uint, List<ResourceEntry>> _resourcesByTag = new();
        private readonly Dictionary<(uint tag, int id), Func<SeekableReadStreamEndian>> _loaders = new();


        private string _fileName = string.Empty;
        private bool _isBigEndian;

        public class ResourceEntry
        {
            public int Id { get; set; }
            public uint Tag { get; set; }
            public long Offset { get; set; }
            public int Size { get; set; }
            public Func<SeekableReadStreamEndian> CreateReadStream { get; set; } = null!;
            public string? Name { get; set; }
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
        // This should be called during archive parsing
        public void RegisterResource(uint tag, int id, int offset, int size, Func<SeekableReadStreamEndian> loader)
        {
            var key = (tag, id);
            _loaders[key] = loader;

            // Ensure placeholder for tag presence (even if lazy)
            if (!_resources.ContainsKey(key))
                _resources[key] = null!;

            if (!_resourcesByTag.TryGetValue(tag, out var list))
            {
                list = new List<ResourceEntry>();
                _resourcesByTag[tag] = list;
            }

            list.Add(new ResourceEntry
            {
                Id = id,
                Tag = tag,
                Offset = offset,
                Size = size,
                CreateReadStream = loader,
                Name = null
            });
        }



        public bool HasResource(uint tag, int id)
        {
            return _resources.ContainsKey((tag, id));
        }

        public SeekableReadStreamEndian GetResource(uint tag, int id)
        {
            var key = (tag, id);
            if (!_resources.TryGetValue(key, out var data) || data == null)
            {
                if (_loaders.TryGetValue(key, out var loader))
                {
                    var stream = loader();
                    using var ms = new MemoryStream();
                    stream.BaseStream.CopyTo(ms);
                    data = ms.ToArray();
                    _resources[key] = data;
                }
                else
                {
                    throw new InvalidOperationException($"Resource {ResourceTags.FromTag(tag)}#{id} not found");
                }
            }

            return new SeekableReadStreamEndian(new MemoryStream(data, writable: false), isBigEndian: _isBigEndian);
        } 
        public SeekableReadStreamEndian? TryGetResource(uint tag, int id)
        {
            var key = (tag, id);
            if (!_resources.TryGetValue(key, out var data) || data == null)
            {
                if (_loaders.TryGetValue(key, out var loader))
                {
                    var stream = loader();
                    using var ms = new MemoryStream();
                    stream.BaseStream.CopyTo(ms);
                    data = ms.ToArray();
                    _resources[key] = data;
                }
                else
                {
                    return null;
                }
            }

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
        public SeekableReadStreamEndian? GetResourceOrNull(uint tag, int id)
        {
            try
            {
                return GetResource(tag, id);
            }
            catch
            {
                return null;
            }
        }

 
        public List<ResourceEntry> GetChunkList(uint tag)
        {
            if (_resourcesByTag.TryGetValue(tag, out var entries))
                return entries;
            
            return new List<ResourceEntry>();
        }

      

        public IEnumerable<uint> ListTags()
        {
            return _resources.Keys.Select(k => k.tag).Distinct();
        }
        public IEnumerable<string> ListTagStrings()
        {
            return _resources.Keys.Select(k => ResourceTags.FromTag(k.tag)).Distinct();
        }
        public IEnumerable<string> ListTagStringsWithId()
        {
            return _resources.Select(k => ResourceTags.FromTag(k.Key.tag)+"="+ k.Key.id).Distinct();
        }

        /// <summary>
        /// Gets the file offset of a resource if available.
        /// </summary>
        public long? GetResourceOffset(uint tag, int id)
        {
            if (_resourcesByTag.TryGetValue(tag, out var list))
            {
                var entry = list.FirstOrDefault(e => e.Id == id);
                return entry?.Offset;
            }

            return null;
        }

    }
}

