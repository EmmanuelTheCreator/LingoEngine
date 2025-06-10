using Director.Fonts;
using Director.Graphics;
using Director.Members;
using Director.Primitives;

namespace Director
{

    public class Cast : IDisposable
    {
        public const int DEFAULT_CAST_LIB = -1;
        public const int KClutSystemMac = 36;
        private readonly Movie _movie;
        private readonly DirectorEngine _vm;
        private readonly Lingo _lingo;

        public readonly ushort _castLibID;
        private readonly ushort _libResourceId;
        private readonly bool _isShared;
        private bool _loadMutex;
        private readonly bool _isExternal;

        private readonly LingoArchive _lingoArchive;
        private readonly Dictionary<int, CastMember> _loadedCast;
        private readonly Dictionary<int, Stxt> _loadedStxts = new();
        private readonly Dictionary<int, RTE0> _loadedRTE0s = new();
        private readonly Dictionary<int, RTE1> _loadedRTE1s = new();
        private readonly Dictionary<int, RTE2> _loadedRTE2s = new();
        private readonly Dictionary<int, CastMemberInfo> _castsInfo = new();
        private readonly Dictionary<string, int> _castsNames = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<int, int> _castsScriptIds = new();
        private readonly Dictionary<ushort, FontMapEntry> _fontMap = new();
        private readonly Dictionary<string, FontXPlatformInfo> _fontXPlatformMap = new();

        private readonly List<CastMember> _loadQueue = new();

        private string _macName = string.Empty;
        private string _castName = string.Empty;

        private ushort _castArrayStart;
        private ushort _castArrayEnd;
        private ushort _castIDoffset;

        private Rect _movieRect;
        private ushort _stageColor;
        private CastMemberID _defaultPalette = new(-1, -1);
        private int _frameRate;

        private Archive? _castArchive;
        private int _version;
        private Platform _platform = Platform.Macintosh;
        private bool _isProtected;

        private readonly TilePatternEntry[] _tiles = new TilePatternEntry[8];

        private LingoDec.ScriptContext? _lingodec;
        private LingoDec.ChunkResolver? _chunkResolver;


        public Dictionary<ushort, FontMapEntry> FontMap => _fontMap;
        public Platform Platform => _platform;
        public int Version => _version;
        public ushort CastIDOffset => _castIDoffset;
        public ushort CastLibID => _castLibID;

        public Cast(Movie movie, ushort castLibID, bool isShared = false, bool isExternal = false, ushort libResourceId = 1024)
        {
            _movie = movie;
            _vm = _movie.GetVM();
            _lingo = _vm.GetLingo();

            _castLibID = castLibID;
            _libResourceId = libResourceId;
            _isShared = isShared;
            _isExternal = isExternal;
            _loadMutex = true;

            _lingoArchive = new LingoArchive(this);
            _loadedCast = new();
        }

        public void Dispose()
        {
            foreach (var stxt in _loadedStxts.Values)
                stxt?.Dispose();

            foreach (var cast in _loadedCast.Values)
                cast?.DecRefCount();
            _loadedCast.Clear();

            //foreach (var info in _castsInfo.Values)
            //    info?.Dispose();

            foreach (var map in _fontMap.Values)
                map?.Dispose();

            foreach (var map in _fontXPlatformMap.Values)
                map?.Dispose();

            foreach (var rte0 in _loadedRTE0s.Values)
                rte0?.Dispose();

            foreach (var rte1 in _loadedRTE1s.Values)
                rte1?.Dispose();

            foreach (var rte2 in _loadedRTE2s.Values)
                rte2?.Dispose();

            _lingoArchive?.Dispose();
            _chunkResolver?.Dispose();
            _lingodec?.Dispose();
        }

        public void SetArchive(Archive archive)
        {
            _castArchive = archive;
            _macName = archive.HasResource("MCNM", 0) ? archive.GetName("MCNM", 0) : archive.GetFileName();
        }

        public Archive? GetArchive() => _castArchive;

        public string GetMacName() => _macName;
        public string GetCastName() => _castName;
        public void SetCastName(string name) => _castName = name;

        public void LoadArchive()
        {
            if (!LoadConfig())
                throw new InvalidOperationException("Failed to load cast config");
            LoadCast();
        }

        public int GetCastSize() => _loadedCast.Count;

        public int GetCastMaxID() => _loadedCast.Count == 0 ? 0 : Math.Max(0, _loadedCast.Keys.Max());

        public int GetNextUnusedID()
        {
            int id = 1;
            while (_loadedCast.ContainsKey(id))
                id++;
            return id;
        }

        public CastMember? GetCastMember(int castId, bool load = true)
        {
            if (_loadedCast.TryGetValue(castId, out var result))
            {
                if (load && _loadMutex)
                {
                    _loadMutex = false;
                    result.Load();
                    while (_loadQueue.Count > 0)
                    {
                        var queued = _loadQueue[^1];
                        _loadQueue.RemoveAt(_loadQueue.Count - 1);
                        queued.Load();
                    }
                    _loadMutex = true;
                }
                else if (load)
                {
                    _loadQueue.Add(result);
                }
                return result;
            }
            return null;
        }

        public CastMember? GetCastMemberByNameAndType(string name, CastType type)
        {
            if (type == CastType.Any)
            {
                if (_castsNames.TryGetValue(name, out int castId))
                    return GetCastMember(castId);
            }
            else
            {
                string key = $"{name}:{(int)type}";
                if (_castsNames.TryGetValue(key, out int castId))
                    return GetCastMember(castId);
            }
            return null;
        }

        public CastMember? GetCastMemberByScriptId(int scriptId)
        {
            if (_castsScriptIds.TryGetValue(scriptId, out int castId))
                return GetCastMember(castId);
            return null;
        }

        public CastMemberInfo? GetCastMemberInfo(int castId) =>
            _castsInfo.TryGetValue(castId, out var info) ? info : null;

        public Stxt? GetStxt(int castId) =>
            _loadedStxts.TryGetValue(castId, out var stxt) ? stxt : null;

        public void SetCastMemberModified(int castId)
        {
            if (_loadedCast.TryGetValue(castId, out var cast))
                cast.SetModified(true);
        }

        public CastMember SetCastMember(int castId, CastMember cast)
        {
            EraseCastMember(castId);
            cast.IncRefCount();
            _loadedCast[castId] = cast;
            return cast;
        }

        public bool EraseCastMember(int castId)
        {
            if (_loadedCast.TryGetValue(castId, out var member))
            {
                member.DecRefCount();
                _loadedCast.Remove(castId);
            }

            if (_castsInfo.TryGetValue(castId, out var info))
            {
                info.Dispose();
                _castsInfo.Remove(castId);
            }
            return true;
        }

        public void ReleaseCastMemberWidget()
        {
            foreach (var cast in _loadedCast.Values)
                cast?.ReleaseWidget();
        }
        public void RebuildCastNameCache()
        {
            _castsNames.Clear();

            foreach (var (id, info) in _castsInfo)
            {
                if (!string.IsNullOrEmpty(info.Name))
                {
                    _castsNames[info.Name] = id;
                    string key = $"{info.Name}:{(int)info.Type}";
                    _castsNames[key] = id;
                }
            }
        }

        public string GetLinkedPath(int castId)
        {
            return _castsInfo.TryGetValue(castId, out var info) ? info.LinkedPath : string.Empty;
        }

        public string GetVideoPath(int castId)
        {
            return _castsInfo.TryGetValue(castId, out var info) ? info.VideoPath : string.Empty;
        }

        public void DumpScript(string script, ScriptType type, ushort id)
        {
            Console.WriteLine($"[Script Dump] ID: {id}, Type: {type}, Content:\\n{script}");
        }
        public bool LoadConfig()
        {
            if (_castArchive == null)
                return false;

            if (_castArchive.HasResource("VWCF", 1024))
            {
                using var stream = _castArchive.GetResourceStream("VWCF", 1024);
                _version = stream.ReadByte();
                _platform = (Platform)stream.ReadByte();
                _isProtected = stream.ReadByte() != 0;

                stream.ReadByte(); // Reserved
                _castArrayStart = stream.ReadUInt16();
                _castArrayEnd = stream.ReadUInt16();
                _frameRate = stream.ReadInt16();
                _stageColor = stream.ReadUInt16();

                return true;
            }

            return false;
        }

        public void LoadCast()
        {
            if (_castArchive == null)
                return;

            for (int id = _castArrayStart; id <= _castArrayEnd; id++)
            {
                if (_castArchive.HasResource("VWCI", (ushort)id))
                    LoadCastInfo(_castArchive.GetResourceStream("VWCI", (ushort)id), (ushort)id);

                if (_castArchive.HasResource("VWSC", (ushort)id))
                    LoadCastLibInfo(_castArchive.GetResourceStream("VWSC", (ushort)id), (ushort)id);
            }
        }

        public void LoadCastInfo(Stream stream, ushort id)
        {
            // Dummy placeholder - real logic depends on your CastMemberInfo parser
            var info = CastMemberInfo.LoadFromStream(stream, id, _version);
            _castsInfo[id] = info;
        }

        public void LoadCastLibInfo(Stream stream, ushort id)
        {
            // Dummy placeholder - real logic depends on your LibInfo format
            var info = _castsInfo.GetValueOrDefault(id);
            if (info != null)
            {
                info.LoadScriptMetadata(stream);
            }
        }
        public void LoadCastDataVWCR(Stream stream)
        {
            // Handle versioned cast record loading
            while (stream.Position < stream.Length)
            {
                ushort id = stream.ReadUInt16BE();
                ushort size = stream.ReadUInt16BE();
                long pos = stream.Position;

                if (_castArchive != null && _castArchive.HasResource("VWCI", id))
                    LoadCastInfo(_castArchive.GetResourceStream("VWCI", id), id);

                stream.Position = pos + size;
            }
        }

        public void LoadCastData(Stream stream, ushort id, Resource res)
        {
            // Actual implementation would depend on the data structure of cast data
            if (_castsInfo.TryGetValue(id, out var info))
            {
                var member = CastMemberFactory.Create(info.Type, this, id);
                member.LoadFromStream(stream, res);
                SetCastMember(id, member);
            }
        }

        public void LoadLingoContext(Stream stream)
        {
            _lingodec = new LingoDec.ScriptContext(stream);
        }

        public void LoadExternalSound(Stream stream)
        {
            while (stream.Position < stream.Length)
            {
                ushort id = stream.ReadUInt16BE();
                ushort size = stream.ReadUInt16BE();
                long pos = stream.Position;

                var res = new Resource(stream.ReadBytes(size));
                LoadCastData(new MemoryStream(res.Data), id, res);

                stream.Position = pos + size;
            }
        }

        public void LoadSord(Stream stream)
        {
            while (stream.Position < stream.Length)
            {
                ushort id = stream.ReadUInt16BE();
                ushort size = stream.ReadUInt16BE();
                stream.Position += size;
            }
        }
        private void LoadScriptV2(Stream stream, ushort id)
        {
            if (_castsInfo.TryGetValue(id, out var info))
            {
                var member = CastMemberFactory.Create(info.Type, this, id);
                member.LoadFromScriptStream(stream, version: 2);
                SetCastMember(id, member);
            }
        }

        private void LoadFontMap(Stream stream)
        {
            ushort entryCount = stream.ReadUInt16BE();
            for (int i = 0; i < entryCount; i++)
            {
                ushort fromFont = stream.ReadUInt16BE();
                ushort toFont = stream.ReadUInt16BE();
                bool remapChars = stream.ReadUInt16BE() != 0;

                var entry = new FontMapEntry
                {
                    ToFont = toFont,
                    RemapChars = remapChars,
                    SizeMap = new Dictionary<ushort, ushort>()
                };

                ushort sizeCount = stream.ReadUInt16BE();
                for (int j = 0; j < sizeCount; j++)
                {
                    ushort fromSize = stream.ReadUInt16BE();
                    ushort toSize = stream.ReadUInt16BE();
                    entry.SizeMap[fromSize] = toSize;
                }

                _fontMap[fromFont] = entry;
            }
        }

        private void LoadFontMapV4(Stream stream)
        {
            ushort entryCount = stream.ReadUInt16BE();
            for (int i = 0; i < entryCount; i++)
            {
                string fontName = stream.ReadPascalString();
                string toFont = stream.ReadPascalString();
                bool remapChars = stream.ReadUInt16BE() != 0;

                var info = new FontXPlatformInfo
                {
                    ToFont = toFont,
                    RemapChars = remapChars,
                    SizeMap = new Dictionary<ushort, ushort>()
                };

                ushort sizeCount = stream.ReadUInt16BE();
                for (int j = 0; j < sizeCount; j++)
                {
                    ushort fromSize = stream.ReadUInt16BE();
                    ushort toSize = stream.ReadUInt16BE();
                    info.SizeMap[fromSize] = toSize;
                }

                _fontXPlatformMap[fontName] = info;
            }
        }

        private void LoadFXmp(Stream stream)
        {
            while (stream.Position < stream.Length)
            {
                if (!ReadFXmpLine(stream))
                    break;
            }
        }

        private bool ReadFXmpLine(Stream stream)
        {
            if (stream.Position + 4 > stream.Length)
                return false;

            ushort from = stream.ReadUInt16BE();
            ushort to = stream.ReadUInt16BE();
            _macCharsToWin[(byte)from] = (byte)to;
            _winCharsToMac[(byte)to] = (byte)from;
            return true;
        }

        private void LoadVWTL(Stream stream)
        {
            for (int i = 0; i < 8; i++)
            {
                ushort id = stream.ReadUInt16BE();
                short top = stream.ReadInt16BE();
                short left = stream.ReadInt16BE();
                short bottom = stream.ReadInt16BE();
                short right = stream.ReadInt16BE();

                _tiles[i].BitmapId = new CastMemberID(id, _castLibID);
                _tiles[i].Rect = new Rect(left, top, right - left, bottom - top);
            }
        }
        public CodePage GetFileEncoding()
        {
            return _platform == Platform.Windows ? CodePage.WindowsLatin1 : CodePage.MacRoman;
        }

        public string DecodeString(string str)
        {
            var encoding = GetFileEncoding();
            return TextDecoder.Decode(str, encoding);
        }

        public string FormatCastSummary(int castId)
        {
            if (!_castsInfo.TryGetValue(castId, out var info))
                return $"[Cast {castId}] <undefined>";

            return $"[Cast {castId}] {info.Type}: {info.Name}";
        }

        public PaletteV4 LoadPalette(Stream stream, int id)
        {
            var palette = new PaletteV4();
            palette.LoadFromStream(stream, id);
            return palette;
        }

        

    }

}