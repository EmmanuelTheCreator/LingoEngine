using Director.Fonts;
using Director.Graphics;
using Director.IO;
using Director.Members;
using Director.Primitives;
using Director.Scripts;
using Director.ScummVM;
using Director.Texts;
using Director.Tools;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.PortableExecutable;
using static Director.IO.ArchiveFileLoader;

namespace Director
{

    public class Cast : IDisposable
    {
        public const int DEFAULT_CAST_LIB = -1;
        public const int KClutSystemMac = 36;
        private readonly Movie _movie;
        //private readonly DirectorEngine _vm;
        //private readonly Lingo _lingo;

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
        private ushort _frameRate;

        private Archive? _castArchive;
        private ushort _version;
        private Platform _platform = Platform.Macintosh;
        private bool _isProtected;

        private readonly TilePatternEntry[] _tiles = new TilePatternEntry[8];

        //private LingoDec.ScriptContext? _lingodec;
        //private LingoDec.ChunkResolver? _chunkResolver;
        private byte[] _macCharsToWin;
        private byte[] _winCharsToMac;
        private ChunkResolver _chunkResolver;
        private LingoScriptContext _lingodec;

        public Dictionary<ushort, FontMapEntry> FontMap => _fontMap;
        public Platform Platform => _platform;
        public ushort Version => _version;
        public ushort CastIDOffset => _castIDoffset;
        public ushort CastLibID => _castLibID;
        public ushort FrameRate => _frameRate;
        public Rect MovieRect => _movieRect;
        public ushort StageColor => _stageColor;
        public CastMemberID DefaultPalette => _defaultPalette;
        public LingoArchive LingoArchive => _lingoArchive;
        public Archive CastArchive => _castArchive;
        public int CastLibId => _castLibID;
        public byte[] MacCharsToWin => _macCharsToWin;
        public byte[] WinCharsToMac => _winCharsToMac;


        public Cast()
        {
            _macCharsToWin = new byte[256];
            _winCharsToMac = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                _macCharsToWin[i] = (byte)i;
                _winCharsToMac[i] = (byte)i;
            }
        }

        //public Cast(Movie movie, ushort castLibID, bool isShared = false, bool isExternal = false, ushort libResourceId = 1024)
        public Cast(Archive archive, ushort castLibID, bool isShared = false, bool isExternal = false, ushort libResourceId = 1024)
            :this()
        {
            _castLibID = castLibID;
            _libResourceId = libResourceId;
            _isShared = isShared;
            _isExternal = isExternal;
            _loadMutex = true;

            _lingoArchive = new LingoArchive(archive);
            _loadedCast = new();
        }

        public void Dispose()
        {
            //foreach (var stxt in _loadedStxts.Values)
            //    stxt?.Dispose();

            foreach (var cast in _loadedCast.Values)
                cast?.DecRefCount();
            _loadedCast.Clear();

            //foreach (var info in _castsInfo.Values)
            //    info?.Dispose();

            //foreach (var map in _fontMap.Values)
            //    map?.Dispose();

            //foreach (var map in _fontXPlatformMap.Values)
            //    map?.Dispose();

            //foreach (var rte0 in _loadedRTE0s.Values)
            //    rte0?.Dispose();

            //foreach (var rte1 in _loadedRTE1s.Values)
            //    rte1?.Dispose();

            //foreach (var rte2 in _loadedRTE2s.Values)
            //    rte2?.Dispose();

            //_lingoArchive?.Dispose();
            //_chunkResolver?.Dispose();
            //_lingodec?.Dispose();
        }

        public void SetArchive(Archive archive)
        {
            _castArchive = archive;
            var tag = ResourceTags.ToTag("MCNM");
            _macName = archive.HasResource(tag, 0) ? archive.GetName(tag, 0) : archive.GetFileName();
        }

        public Archive? GetArchive() => _castArchive;

        public string GetMacName() => _macName;
        public string GetCastName() => _castName;
        public void SetCastName(string name) => _castName = name;

        public void LoadArchive()
        {
            if (!LoadConfig())
            {
                LogHelper.DebugWarning("Cast config VWCF#1024 not found. Proceeding without it.");
                // Fallback defaults:
                _version = 0x0A; // Assume Director 4+
                _platform = Platform.Macintosh; // Or Windows
                _isProtected = false;
                _castArrayStart = 1;
                _castArrayEnd = 1024;
                _frameRate = 30;
                _stageColor = 0;
            }

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
                //info.Dispose();
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

            if (_castArchive.HasResource(ResourceTags.VWCF, 1024))
            {
                using var stream = _castArchive.GetResource(ResourceTags.VWCF, 1024);
                _version = stream.ReadByte();
                _platform = (Platform)stream.ReadByte();
                _isProtected = stream.ReadByte() != 0;

                stream.ReadByte(); // Reserved
                _castArrayStart = stream.ReadUInt16();
                _castArrayEnd = stream.ReadUInt16();
                _frameRate = (ushort)stream.ReadInt16();
                _stageColor = stream.ReadUInt16();

                return true;
            }

            return false;
        }

        public void LoadCast()
        {
            if (_castArchive == null)
                return;

           
            // XFIR / chunk-based format: use CASt chunk
            if (_castArchive.HasResource(ResourceTags.CASt, 0))
            {
                using SeekableReadStreamEndian stream = _castArchive.GetResource(ResourceTags.CASt, 0);
                LoadCastIndexCASt(stream);
                return;
            }

            // MV93 legacy formats
            if (_castArchive.HasResource(ResourceTags.MV93, 0))
            {
                using var stream = _castArchive.GetResource(ResourceTags.MV93, 0);
                LoadCastDataVWCR(stream);
                return;
            }

            // VWCR-based format (older Director versions)
            if (_castArchive.HasResource(ResourceTags.VWCR, 1024))
            {
                using var stream = _castArchive.GetResource(ResourceTags.VWCR, 1024);
                LoadCastDataVWCR(stream);
                return;
            }

            // Fallback legacy resource loading loop
            for (int id = _castArrayStart; id <= _castArrayEnd; id++)
            {
                if (_castArchive.HasResource(ResourceTags.VWCI, (ushort)id))
                    LoadCastInfo(_castArchive.GetResource(ResourceTags.VWCI, (ushort)id), (ushort)id);

                if (_castArchive.HasResource(ResourceTags.VWSC, (ushort)id))
                    LoadCastLibInfo(_castArchive.GetResource(ResourceTags.VWSC, (ushort)id), (ushort)id);
            }
        }
        private void LoadCastIndexCASt(SeekableReadStreamEndian stream)
        {
            //using var mmap = _castArchive.GetResource(1835884912, 0);
            //List<MMapEntry> entries = ReadMMapEntries(mmap);

            var reader = new BinaryReader(stream.BaseStream);

            // Read CASt header
            ushort fieldCount = reader.ReadUInt16BE(); // always 0?
            ushort maxCastId = reader.ReadUInt16BE();  // maximum used cast ID
            ushort itemCount = reader.ReadUInt16BE();  // number of items in this table
            ushort fieldSize = reader.ReadUInt16BE();  // bytes per table entry
            ushort unknown = reader.ReadUInt16BE();    // unused, often 0

            for (int i = 0; i < itemCount; i++)
            {
                int castId = reader.ReadUInt16BE();
                int offset = reader.ReadInt32BE();
                int length = reader.ReadInt32BE();
                byte flags = reader.ReadByte(); // type flags (bitmap, text, script, etc.)

                stream.Seek(1, SeekOrigin.Current); // skip padding byte
                ushort type = reader.ReadUInt16BE(); // cast type

                LogHelper.DebugWarning($"CASt[{i}] load: {castId}: {offset}");
                if (offset > 0 && length > 0)
                {
                    var slice = new SliceStream(stream.BaseStream, offset, length);
                    //var memberStream = new SeekableReadStreamEndian(slice, isBigEndian: true);
                    //CastMember member = CastMemberFactory.Create((CastType)type,this,i+);
                    //member.ReadFromStream(memberStream);

                    ///member.CastId = castId;
                    //member.Type = (CastMemberType)type;

                    //_loadedCast[castId] = member;
                }
            }
        }
        private CastType DetectCastType(BinaryReader reader)
        {
            long mark = reader.BaseStream.Position;

            uint typeMarker = reader.ReadUInt32BE();
            reader.BaseStream.Position = mark;

            return typeMarker switch
            {
                0xFE030001 => CastType.Bitmap,
                0xFE040001 => CastType.Text,
                0xFE050001 => CastType.Script,
                _ => CastType.Empty
            };
        }

        public void LoadCastDataVWCR(SeekableReadStreamEndian stream)
        {
            while (stream.Position < stream.Length)
            {
                ushort id = stream.ReadUInt16BE();
                ushort size = stream.ReadUInt16BE();
                long pos = stream.Position;

                if (_castArchive != null && _castArchive.HasResource(ResourceTags.VWCI, id))
                    LoadCastInfo(_castArchive.GetResource(ResourceTags.VWCI, id), id);

                stream.Position = pos + size;
            }
        }

   
        public void LoadCastInfo(SeekableReadStreamEndian stream, ushort id)
        {
            if (!_loadedCast.ContainsKey(id))
                return;

            var castInfo = _movie.LoadInfoEntries(stream, _version);

            var ci = new CastMemberInfo();
            MemoryReadStreamEndian entryStream = null;
            var member = _loadedCast[id];

            switch (castInfo.Strings.Count)
            {
                default:
                    LogHelper.DebugWarning($"Cast::loadCastInfo(): BUILDBOT: extra {castInfo.Strings.Count - 15} strings for castid {id}");
                    goto case 15;
                case 15:
                    if (castInfo.Strings[14].Length > 0)
                    {
                        LogHelper.DebugWarning($"Cast::loadCastInfo(): BUILDBOT: string #14 for castid {id}");
                        LogHelper.DebugHexdump(castInfo.Strings[14].Data, castInfo.Strings[14].Length);
                    }
                    goto case 14;
                case 14:
                    if (castInfo.Strings[13].Length > 0)
                    {
                        LogHelper.DebugWarning($"Cast::loadCastInfo(): BUILDBOT: string #13 for castid {id}");
                        LogHelper.DebugHexdump(castInfo.Strings[13].Data, castInfo.Strings[13].Length);
                    }
                    goto case 13;
                case 13:
                    if (castInfo.Strings[12].Length > 0)
                    {
                        LogHelper.DebugWarning($"Cast::loadCastInfo(): BUILDBOT: string #12 for castid {id}");
                        LogHelper.DebugHexdump(castInfo.Strings[12].Data, castInfo.Strings[12].Length);
                    }
                    goto case 12;
                case 12:
                    if (castInfo.Strings[11].Length > 0)
                    {
                        LogHelper.DebugWarning($"Cast::loadCastInfo(): BUILDBOT: string #11 for castid {id}");
                        LogHelper.DebugHexdump(castInfo.Strings[11].Data, castInfo.Strings[11].Length);
                    }
                    goto case 11;
                case 11:
                    if (castInfo.Strings[10].Length > 0)
                    {
                        LogHelper.DebugWarning($"Cast::loadCastInfo(): BUILDBOT: string #10 for castid {id}");
                        LogHelper.DebugHexdump(castInfo.Strings[10].Data, castInfo.Strings[10].Length);
                    }
                    goto case 10;
                case 10:
                    if (castInfo.Strings[9].Length > 0)
                    {
                        LogHelper.DebugWarning($"Cast::loadCastInfo(): BUILDBOT: string #9 for castid {id}");
                        LogHelper.DebugHexdump(castInfo.Strings[9].Data, castInfo.Strings[9].Length);
                    }
                    goto case 9;
                case 9:
                    if (castInfo.Strings[8].Length > 0)
                    {
                        entryStream = new MemoryReadStreamEndian(castInfo.Strings[8].Data, castInfo.Strings[8].Length, stream.IsBigEndian);
                        ReadEditInfo(ci.TextEditInfo, entryStream);
                        entryStream.Dispose();
                    }
                    goto case 8;
                case 8:
                    if (castInfo.Strings[7].Length > 0)
                    {
                        entryStream = new MemoryReadStreamEndian(castInfo.Strings[7].Data, castInfo.Strings[7].Length, stream.IsBigEndian);
                        var count = entryStream.ReadUInt16();
                        for (short i = 0; i < count; i++)
                            ci.ScriptStyle.Read(entryStream, this);
                        entryStream.Dispose();
                    }
                    goto case 7;
                case 7:
                    if (castInfo.Strings[6].Length > 0)
                    {
                        entryStream = new MemoryReadStreamEndian(castInfo.Strings[6].Data, castInfo.Strings[6].Length, stream.IsBigEndian);
                        ReadEditInfo(ci.ScriptEditInfo, entryStream);
                        entryStream.Dispose();
                    }
                    goto case 6;
                case 6:
                    ci.Type = Enum.Parse< CastType>(castInfo.Strings[4].ReadString());
                    goto case 5;
                case 5:
                    ci.FileName = castInfo.Strings[3].ReadString();
                    goto case 4;
                case 4:
                    ci.Directory = castInfo.Strings[2].ReadString();
                    goto case 3;
                case 3:
                    ci.Name = castInfo.Strings[1].ReadString();
                    goto case 2;
                case 2:
                    ci.Script = castInfo.Strings[0].ReadString(false);
                    break;
            }

            LogHelper.DebugLog(4, DebugChannel.Loading, $"Cast::loadCastInfo(): castId: {id}, size: {castInfo.Strings.Count}, script: {ci.Script}, name: {ci.Name}, directory: {ci.Directory}, fileName: {ci.FileName}, type: {ci.Type}");

            if (_version < FileVersion.Ver400 || LogHelper.DebugChannelSet(-1, DebugChannel.NoBytecode))
            {
                if (!string.IsNullOrEmpty(ci.Script))
                {
                    var scriptType = ScriptType.Cast;
                    if (member.Type == CastType.Script)
                    {
                        scriptType = ((ScriptCastMember)member).ScriptType;
                    }

                    if (ConfMan.GetBool("dump_scripts"))
                        DumpScript(ci.Script, scriptType, id);

                    _lingoArchive.AddCode(ci.Script, scriptType, id, ci.Name);
                }
            }

            if (_version >= FileVersion.Ver400 && _version < FileVersion.Ver600 && member.Type == CastType.Sound)
            {
                ((SoundCastMember)member).Looping = (castInfo.Flags & 16) == 0 ? 1 : 0;
            }
            else if (_version >= FileVersion.Ver600 && member.Type == CastType.Sound)
            {
                LogHelper.DebugWarning($"STUB: Cast::loadCastInfo(): Sound cast member info not yet supported for version {_version}");
            }

            if (member.Type == CastType.Palette)
                member.Load();

            ci.AutoHilite = (castInfo.Flags & 2) != 0;
            ci.ScriptId = castInfo.ScriptId;
            if (ci.ScriptId != 0)
                _castsScriptIds[(int)ci.ScriptId] = id;

            _castsInfo[id] = ci;
        }
       


        public void LoadCastLibInfo(SeekableReadStreamEndian stream, ushort id)
        {
            // Dummy placeholder - real logic depends on your LibInfo format
            var info = _castsInfo.GetValueOrDefault(id);
            if (info != null)
            {
                info.LoadScriptMetadata(stream, this);
            }
        }
        

        public void LoadCastData(SeekableReadStreamEndian stream, ushort id, Resource res)
        {
            // Actual implementation would depend on the data structure of cast data
            if (_castsInfo.TryGetValue(id, out var info))
            {
                var member = CastMemberFactory.Create(info.Type, this, id);
                member.LoadFromStream(stream, res);
                SetCastMember(id, member);
            }
        }

        public void LoadLingoContext(SeekableReadStreamEndian stream, ushort id)
        {
            if (!_loadedCast.ContainsKey(id))
                return;

            var member = _loadedCast[id];
            if (member is not ScriptCastMember script)
                return;

            var resolver = new ChunkResolver(this);
            resolver.Resolve(stream, script, _version);
        }


        public void LoadExternalSound(SeekableReadStreamEndian stream)
        {
            while (stream.Position < stream.Length)
            {
                ushort id = stream.ReadUInt16BE();
                ushort size = stream.ReadUInt16BE();
                long pos = stream.Position;

                var res = new Resource(stream.ReadBytes(size));
                LoadCastData(new SeekableReadStreamEndian(new MemoryStream(res.Data),true), id, res);

                stream.Position = pos + size;
            }
        }

        public void LoadSord(SeekableReadStreamEndian stream)
        {
            while (stream.Position < stream.Length)
            {
                ushort id = stream.ReadUInt16BE();
                ushort size = stream.ReadUInt16BE();
                stream.Position += size;
            }
        }
        private void LoadScriptV2(SeekableReadStreamEndian stream, ushort id)
        {
            if (_castsInfo.TryGetValue(id, out var info))
            {
                var member = CastMemberFactory.Create(info.Type, this, id);
                member.LoadFromScriptStream(stream, version: 2);
                SetCastMember(id, member);
            }
        }

        private void LoadFontMap(SeekableReadStreamEndian stream)
        {
            ushort entryCount = stream.ReadUInt16BE();
            for (int i = 0; i < entryCount; i++)
            {
                ushort fromFont = stream.ReadUInt16BE();
                ushort toFont = stream.ReadUInt16BE();
                bool remapChars = stream.ReadUInt16BE() != 0;

                var entry = new FontMapEntry(toFont, remapChars);

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

        private void LoadFontMapV4(SeekableReadStreamEndian stream)
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

        private void LoadFXmp(SeekableReadStreamEndian stream)
        {
            while (stream.Position < stream.Length)
            {
                if (!ReadFXmpLine(stream))
                    break;
            }
        }

        private bool ReadFXmpLine(SeekableReadStreamEndian stream)
        {
            if (stream.Position + 4 > stream.Length)
                return false;

            ushort from = stream.ReadUInt16BE();
            ushort to = stream.ReadUInt16BE();
            _macCharsToWin[(byte)from] = (byte)to;
            _winCharsToMac[(byte)to] = (byte)from;
            return true;
        }

        private void LoadVWTL(SeekableReadStreamEndian stream)
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

        public PaletteV4 LoadPalette(SeekableReadStreamEndian stream, int id)
        {
            var palette = new PaletteV4();
            palette.LoadFromStream(stream, id);
            return palette;
        }


        /// <summary>
        /// Reads EditInfo from the given stream into the provided info structure.
        /// </summary>
        public static void ReadEditInfo(EditInfo info, SeekableReadStreamEndian stream)
        {
            info.Rect = stream.ReadRect();
            info.SelStart = (int)stream.ReadUInt32();
            info.SelEnd = (int)stream.ReadUInt32();
            info.Version = stream.ReadByte();
            info.RulerFlag = stream.ReadByte();

            if (LogHelper.DebugChannelSet(3, DebugChannel.Loading))
            {
                LogHelper.DebugLog(3, DebugChannel.Loading,
                    $"EditInfo: Rect={info.Rect}, SelStart={info.SelStart}, SelEnd={info.SelEnd}, Version={info.Version}, RulerFlag={info.RulerFlag}");
            }
        }

        public void LoadLingoContext(SeekableReadStreamEndian stream)
        {
            if (_version < FileVersion.Ver400)
                throw new InvalidOperationException($"Unsupported Director version ({_version})");

            LogHelper.DebugLog(1, DebugChannel.Compile, "Add D4 script context");

            if (LogHelper.DebugChannelSet(5, DebugChannel.Loading))
            {
                LogHelper.DebugLog(5, DebugChannel.Loading, "Lctx header:");
                stream.HexDump(0x2A);
            }

            stream.ReadUInt16();
            stream.ReadUInt16();
            stream.ReadUInt16();
            stream.ReadUInt16();
            int itemCount = stream.ReadInt32();
            stream.ReadInt32(); // itemCount2
            ushort itemsOffset = stream.ReadUInt16();
            stream.ReadUInt16(); // entrySize
            stream.ReadUInt32(); // unk1
            stream.ReadUInt32(); // fileType
            stream.ReadUInt32(); // unk2
            int nameTableId = stream.ReadInt32();
            stream.ReadInt16(); // validCount
            stream.ReadUInt16(); // flags
            short firstUnused = stream.ReadInt16();

            LogHelper.DebugLog(2, DebugChannel.Loading, $"****** Loading Lnam resource ({nameTableId})");
            using (var nameStream = _castArchive.GetResource(ResourceTags.MKTAG('L', 'n', 'a', 'm'), nameTableId))
            {
                _lingoArchive.AddNamesV4(nameStream);
            }

            var entries = new List<LingoContextEntry>();
            stream.Seek(itemsOffset);
            for (short i = 1; i <= itemCount; i++)
            {
                if (LogHelper.DebugChannelSet(5, DebugChannel.Loading))
                {
                    LogHelper.DebugLog(5, DebugChannel.Loading, $"Context entry {i}:");
                    stream.HexDump(0xC);
                }

                stream.ReadUInt32();
                int index = stream.ReadInt32();
                stream.ReadUInt16(); // entryFlags
                short nextUnused = stream.ReadInt16();

                entries.Add(new LingoContextEntry(index, nextUnused));
            }

            int next = firstUnused;
            while (next >= 0 && next < entries.Count)
            {
                var entry = entries[next];
                entry.Unused = true;
                next = entry.NextUnused;
            }

            for (short i = 1; i <= entries.Count; i++)
            {
                var entry = entries[i - 1];
                if (entry.Unused && entry.Index < 0)
                {
                    LogHelper.DebugLog(1, DebugChannel.Compile, $"Cast::loadLingoContext: Script {i} is unused and empty");
                    continue;
                }
                if (entry.Unused)
                {
                    LogHelper.DebugLog(1, DebugChannel.Compile, $"Cast::loadLingoContext: Script {i} is unused but not empty");
                    continue;
                }
                if (entry.Index < 0)
                {
                    LogHelper.DebugLog(1, DebugChannel.Compile, $"Cast::loadLingoContext: Script {i} is used but empty");
                    continue;
                }
                var resolver = new ChunkResolver(this); 
                var context = new LingoScriptContext(_version, resolver);

                using (var scrStream = _castArchive.GetResource(ResourceTags.Lscr, entry.Index))
                {
                    _lingoArchive.AddCodeV4(scrStream, i, _macName, _version, context, _loadedCast);
                }

                _lingoArchive.LctxContexts[i] = context;
            }

            foreach (var (contextId, context) in _lingoArchive.LctxContexts)
            {
                _lingoArchive.AddScriptContext(ScriptType.Score, contextId, context); // or another type if appropriate

                foreach (var script in context.Scripts.Values)
                {
                    if (script.Id >= 0 && !script.IsFactory)
                    {
                        _lingoArchive.PatchScriptHandler(script.Type, new CastMemberID(script.Id, _castLibID));
                    }
                    else
                    {
                        script.SetOnlyInLctxContexts();
                    }
                }
            }
            if (LogHelper.DebugChannelSet(-1, DebugChannel.ImGui) || ConfMan.GetBool("dump_scripts"))
            {
                stream.Seek(0);
                _chunkResolver = new ChunkResolver(this);
                _lingodec = new LingoScriptContext(_version, _chunkResolver);
                _lingodec.Read(stream);
                _lingodec.ParseScripts();

                foreach (var pair in _lingodec.Scripts)
                {
                    LogHelper.DebugLog(9, DebugChannel.Compile, $"[{pair.Value.CastId}/{pair.Key}] {pair.Value.ScriptText("\n", false)}");
                }

                if (ConfMan.GetBool("dump_scripts"))
                {
                    foreach (var pair in _lingodec.Scripts)
                    {
                        ScriptType type = ScriptType.Movie;
                        if (_loadedCast.ContainsKey(pair.Value.CastId))
                        {
                            var member = _loadedCast[pair.Value.CastId];
                            if (member is ScriptCastMember scriptMember)
                                type = scriptMember.ScriptType;
                        }

                        string lingoPath = LingoArchive.DumpScriptName(_macName, type, pair.Value.CastId, "lingo");
                        File.WriteAllText(lingoPath, pair.Value.ScriptText("\n", true));
                    }
                }
            }
        }

    }

}