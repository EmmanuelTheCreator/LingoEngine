using System.Collections.Generic;
using System.IO;
using ProjectorRays.Common;
using ProjectorRays.LingoDec;

namespace ProjectorRays.Director;

public class ChunkInfo
{
    public int Id;
    public uint FourCC;
    public uint Len;
    public uint UncompressedLen;
    public int Offset;
    public MoaID CompressionID;
}

public class DirectorFile : ChunkResolver
{
    private int _ilsBodyOffset;
    private byte[] _ilsBuf = Array.Empty<byte>();

    private readonly Dictionary<int, byte[]> _cachedChunkBufs = new();
    private readonly Dictionary<int, BufferView> _cachedChunkViews = new();
    public ReadStream? Stream;
    public KeyTableChunk? KeyTable;
    public ConfigChunk? Config;

    public Endianness Endianness;
    public string FverVersionString = string.Empty;
    public uint Version;
    public bool DotSyntax;
    public uint Codec;
    public bool Afterburned;

    public Dictionary<uint, List<int>> ChunkIDsByFourCC = new();
    public Dictionary<int, ChunkInfo> ChunkInfoMap = new();
    public Dictionary<int, Chunk> DeserializedChunks = new();

    public List<CastChunk> Casts = new();

    public InitialMapChunk? InitialMap;
    public MemoryMapChunk? MemoryMap;

    private static uint FOURCC(char a, char b, char c, char d)
        => ((uint)a << 24) | ((uint)b << 16) | ((uint)c << 8) | (uint)d;

    public DirectorFile() { }

    public virtual bool Read(ReadStream stream)
    {
        Stream = stream;
        stream.Endianness = Endianness.BigEndian;

        uint metaFourCC = stream.ReadUint32();
        if (metaFourCC == FOURCC('X', 'F', 'I', 'R'))
            stream.Endianness = Endianness.LittleEndian;
        Endianness = stream.Endianness;
        stream.ReadUint32(); // meta length
        Codec = stream.ReadUint32();

        if (Codec == FOURCC('M', 'V', '9', '3') || Codec == FOURCC('M', 'C', '9', '5'))
        {
            ReadMemoryMap();
        }
        else if (Codec == FOURCC('F', 'G', 'D', 'M') || Codec == FOURCC('F', 'G', 'D', 'C'))
        {
            Afterburned = true;
            if (!ReadAfterburnerMap())
                return false;
        }
        else
        {
            // unsupported codec
            return false;
        }

        if (!ReadKeyTable()) return false;
        if (!ReadConfig()) return false;
        if (!ReadCasts()) return false;

        return true;
    }

    public override Script? GetScript(int id)
    {
        return null;
    }

    public override ScriptNames? GetScriptNames(int id)
    {
        return null;
    }

    public bool ChunkExists(uint fourCC, int id)
    {
        return ChunkInfoMap.ContainsKey(id) && ChunkInfoMap[id].FourCC == fourCC;
    }

    private void AddChunkInfo(ChunkInfo info)
    {
        ChunkInfoMap[info.Id] = info;
        if (!ChunkIDsByFourCC.TryGetValue(info.FourCC, out var list))
        {
            list = new List<int>();
            ChunkIDsByFourCC[info.FourCC] = list;
        }
        list.Add(info.Id);
    }

    private void ReadMemoryMap()
    {
        InitialMap = (InitialMapChunk)ReadChunk(FOURCC('i','m','a','p'));
        DeserializedChunks[1] = InitialMap;

        Stream!.Seek((int)InitialMap.MmapOffset);
        MemoryMap = (MemoryMapChunk)ReadChunk(FOURCC('m','m','a','p'));
        DeserializedChunks[2] = MemoryMap;

        for (int i = 0; i < MemoryMap.MapArray.Count; i++)
        {
            var entry = MemoryMap.MapArray[i];
            if (entry.FourCC == FOURCC('f','r','e','e') || entry.FourCC == FOURCC('j','u','n','k'))
                continue;

            ChunkInfo info = new()
            {
                Id = i,
                FourCC = entry.FourCC,
                Len = entry.Len,
                UncompressedLen = entry.Len,
                Offset = entry.Offset,
                CompressionID = GuidConstants.NULL_COMPRESSION_GUID
            };
            AddChunkInfo(info);
        }
    }

    private bool ReadAfterburnerMap()
    {
        var s = Stream!;
        if (s.ReadUint32() != FOURCC('F','v','e','r')) return false;
        uint fverLength = s.ReadVarInt();
        int start = s.Pos;
        uint fverVersion = s.ReadVarInt();
        if (fverVersion >= 0x401)
        {
            s.ReadVarInt();
            s.ReadVarInt();
        }
        if (fverVersion >= 0x501)
        {
            byte len = s.ReadUint8();
            FverVersionString = s.ReadString(len);
        }
        int end = s.Pos;
        if (end - start != fverLength)
            s.Seek(start + (int)fverLength);

        if (s.ReadUint32() != FOURCC('F','c','d','r')) return false;
        uint fcdrLength = s.ReadVarInt();
        byte[] fcdrBuf = new byte[fcdrLength * 10];
        int fcdrUncomp = s.ReadZlibBytes((int)fcdrLength, fcdrBuf, fcdrBuf.Length);
        if (fcdrUncomp <= 0) return false;
        var fcdrStream = new ReadStream(fcdrBuf, fcdrUncomp, Endianness);
        ushort compCount = fcdrStream.ReadUint16();
        var compIDs = new List<MoaID>();
        for (int i = 0; i < compCount; i++)
        {
            var id = new MoaID();
            id.Read(fcdrStream);
            compIDs.Add(id);
        }
        for (int i = 0; i < compCount; i++)
            fcdrStream.ReadCString();

        if (s.ReadUint32() != FOURCC('A','B','M','P')) return false;
        uint abmpLength = s.ReadVarInt();
        int abmpEnd = s.Pos + (int)abmpLength;
        uint abmpCompressionType = s.ReadVarInt();
        uint abmpUncompLen = s.ReadVarInt();
        byte[] abmpBuf = new byte[abmpUncompLen];
        int abmpActual = s.ReadZlibBytes(abmpEnd - s.Pos, abmpBuf, abmpBuf.Length);
        if (abmpActual <= 0) return false;
        var abmpStream = new ReadStream(abmpBuf, abmpActual, Endianness);

        abmpStream.ReadVarInt();
        abmpStream.ReadVarInt();
        uint resCount = abmpStream.ReadVarInt();
        for (int i = 0; i < resCount; i++)
        {
            int resId = (int)abmpStream.ReadVarInt();
            int offset = (int)abmpStream.ReadVarInt();
            uint compSize = abmpStream.ReadVarInt();
            uint uncompSize = abmpStream.ReadVarInt();
            uint compType = abmpStream.ReadVarInt();
            uint tag = abmpStream.ReadUint32();

            ChunkInfo info = new()
            {
                Id = resId,
                FourCC = tag,
                Len = compSize,
                UncompressedLen = uncompSize,
                Offset = offset,
                CompressionID = compIDs[(int)compType]
            };
            AddChunkInfo(info);
        }

        if (!ChunkInfoMap.ContainsKey(2)) return false;
        if (s.ReadUint32() != FOURCC('F','G','E','I')) return false;
        var ilsInfo = ChunkInfoMap[2];
        s.ReadVarInt();
        _ilsBodyOffset = s.Pos;
        _ilsBuf = new byte[ilsInfo.UncompressedLen];
        int ilsActual = s.ReadZlibBytes((int)ilsInfo.Len, _ilsBuf, _ilsBuf.Length);
        var ilsStream = new ReadStream(_ilsBuf, ilsActual, Endianness);
        while (!ilsStream.Eof)
        {
            int resId = (int)ilsStream.ReadVarInt();
            var info = ChunkInfoMap[resId];
            _cachedChunkViews[resId] = ilsStream.ReadByteView((int)info.Len);
        }

        return true;
    }

    private bool ReadKeyTable()
    {
        var info = GetFirstChunkInfo(FOURCC('K','E','Y','*'));
        if (info == null) return false;
        KeyTable = (KeyTableChunk)GetChunk(info.FourCC, info.Id);
        return true;
    }

    private bool ReadConfig()
    {
        var info = GetFirstChunkInfo(FOURCC('D','R','C','F')) ?? GetFirstChunkInfo(FOURCC('V','W','C','F'));
        if (info == null) return false;
        Config = (ConfigChunk)GetChunk(info.FourCC, info.Id);
        Version = Util.HumanVersion((uint)Config.DirectorVersion);
        DotSyntax = Version >= 700;
        return true;
    }

    private bool ReadCasts()
    {
        bool internalCast = true;
        if (Version >= 500)
        {
            var info = GetFirstChunkInfo(FOURCC('M','C','s','L'));
            if (info != null)
            {
                var castList = (CastListChunk)GetChunk(info.FourCC, info.Id);
                foreach (var entry in castList.Entries)
                {
                    int sectionID = -1;
                    foreach (var keyEntry in KeyTable!.Entries)
                        if (keyEntry.CastID == entry.Id && keyEntry.FourCC == FOURCC('C','A','S','*'))
                        { sectionID = keyEntry.SectionID; break; }
                    if (sectionID > 0)
                    {
                        var cast = (CastChunk)GetChunk(FOURCC('C','A','S','*'), sectionID);
                        cast.Name = entry.Name;
                        Casts.Add(cast);
                    }
                }
                return true;
            }
            else
            {
                internalCast = false;
            }
        }

        var def = GetFirstChunkInfo(FOURCC('C','A','S','*'));
        if (def != null)
        {
            var cast = (CastChunk)GetChunk(def.FourCC, def.Id);
            cast.Name = internalCast ? "Internal" : "External";
            Casts.Add(cast);
        }

        return true;
    }

    private ChunkInfo? GetFirstChunkInfo(uint fourCC)
    {
        if (ChunkIDsByFourCC.TryGetValue(fourCC, out var list) && list.Count > 0)
            return ChunkInfoMap[list[0]];
        return null;
    }

    public Chunk GetChunk(uint fourCC, int id)
    {
        if (DeserializedChunks.TryGetValue(id, out var chunk))
            return chunk;

        BufferView view = GetChunkData(fourCC, id);
        chunk = MakeChunk(fourCC, view);
        DeserializedChunks[id] = chunk;
        return chunk;
    }

    private BufferView GetChunkData(uint fourCC, int id)
    {
        if (!ChunkInfoMap.TryGetValue(id, out var info))
            throw new IOException($"Could not find chunk {id}");
        if (fourCC != info.FourCC)
            throw new IOException($"Expected chunk {id} to be '{Common.Util.FourCCToString(fourCC)}' but is '{Common.Util.FourCCToString(info.FourCC)}'");

        if (_cachedChunkViews.TryGetValue(id, out var view))
            return view;

        var s = Stream!;
        if (Afterburned)
        {
            s.Seek(info.Offset + _ilsBodyOffset);
            if (info.Len == 0 && info.UncompressedLen == 0)
                _cachedChunkViews[id] = s.ReadByteView((int)info.Len);
            else if (CompressionImplemented(info.CompressionID))
            {
                int actual = -1;
                _cachedChunkBufs[id] = new byte[info.UncompressedLen];
                if (info.CompressionID.Equals(GuidConstants.ZLIB_COMPRESSION_GUID))
                    actual = s.ReadZlibBytes((int)info.Len, _cachedChunkBufs[id], _cachedChunkBufs[id].Length);
                else if (info.CompressionID.Equals(GuidConstants.SND_COMPRESSION_GUID))
                {
                    BufferView chunkView = s.ReadByteView((int)info.Len);
                    var chunkStream = new ReadStream(chunkView, Endianness);
                    var uncompStream = new WriteStream(_cachedChunkBufs[id], _cachedChunkBufs[id].Length, Endianness);
                    actual = Sound.DecompressSnd(chunkStream, uncompStream, id);
                }
                if (actual < 0) throw new IOException($"Chunk {id}: Could not decompress");
                _cachedChunkViews[id] = new BufferView(_cachedChunkBufs[id], actual);
            }
            else if (info.CompressionID.Equals(GuidConstants.FONTMAP_COMPRESSION_GUID))
            {
                _cachedChunkViews[id] = FontMap.GetFontMap((int)Version);
            }
            else
            {
                _cachedChunkViews[id] = s.ReadByteView((int)info.Len);
            }
        }
        else
        {
            s.Seek(info.Offset);
            _cachedChunkViews[id] = ReadChunkData(fourCC, info.Len);
        }

        return _cachedChunkViews[id];
    }

    private Chunk ReadChunk(uint fourCC, uint len = uint.MaxValue)
    {
        BufferView view = ReadChunkData(fourCC, len);
        return MakeChunk(fourCC, view);
    }

    private BufferView ReadChunkData(uint fourCC, uint len)
    {
        var s = Stream!;
        int offset = s.Pos;
        uint validFourCC = s.ReadUint32();
        uint validLen = s.ReadUint32();
        if (len == uint.MaxValue) len = validLen;
        if (fourCC != validFourCC || len != validLen)
            throw new IOException($"At offset {offset} expected '{Common.Util.FourCCToString(fourCC)}' chunk len {len} but got '{Common.Util.FourCCToString(validFourCC)}' len {validLen}");
        return s.ReadByteView((int)len);
    }

    private Chunk MakeChunk(uint fourCC, BufferView view)
    {
        Chunk chunk = fourCC switch
        {
            var v when v == FOURCC('i','m','a','p') => new InitialMapChunk(this),
            var v when v == FOURCC('m','m','a','p') => new MemoryMapChunk(this),
            var v when v == FOURCC('C','A','S','*') => new CastChunk(this),
            var v when v == FOURCC('C','A','S','t') => new CastMemberChunk(this),
            var v when v == FOURCC('K','E','Y','*') => new KeyTableChunk(this),
            var v when v == FOURCC('L','c','t','x') || v == FOURCC('L','c','t','X') => new ScriptContextChunk(this),
            var v when v == FOURCC('L','n','a','m') => new ScriptNamesChunk(this),
            var v when v == FOURCC('L','s','c','r') => new ScriptChunk(this),
            var v when v == FOURCC('V','W','C','F') || v == FOURCC('D','R','C','F') => new ConfigChunk(this),
            var v when v == FOURCC('M','C','s','L') => new CastListChunk(this),
            _ => throw new IOException($"Could not deserialize '{Common.Util.FourCCToString(fourCC)}' chunk")
        };
        var chunkStream = new ReadStream(view, Endianness);
        chunk.Read(chunkStream);
        return chunk;
    }

    public override Script? GetScript(int id) => (ScriptChunk)GetChunk(FOURCC('L','s','c','r'), id);
    public override ScriptNames? GetScriptNames(int id) => (ScriptNamesChunk)GetChunk(FOURCC('L','n','a','m'), id);

    private static bool CompressionImplemented(MoaID id) =>
        id.Equals(GuidConstants.ZLIB_COMPRESSION_GUID) || id.Equals(GuidConstants.SND_COMPRESSION_GUID);
}
