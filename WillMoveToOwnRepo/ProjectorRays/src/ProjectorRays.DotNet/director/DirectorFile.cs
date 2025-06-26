using System.Collections.Generic;
using System.IO;
using ProjectorRays.Common;
using ProjectorRays.LingoDec;
using ProjectorRays.IO;
using Microsoft.Extensions.Logging;
using ProjectorRays.director.Chunks;
using ProjectorRays.director;

namespace ProjectorRays.Director;

public class ChunkInfo
{
    public int Id;
    public uint FourCC;
    public uint Len;
    public uint UncompressedLen;
    public int Offset;
    public RayGuid CompressionID;
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

    public ScoreChunk? Score;

    public InitialMapChunk? InitialMap;
    public MemoryMapChunk? MemoryMap;
    public ILogger Logger;
    public static uint FOURCC(char a, char b, char c, char d)
        => ((uint)a << 24) | ((uint)b << 16) | ((uint)c << 8) | (uint)d;

    public DirectorFile(ILogger logger) { Logger = logger; }

    /// <summary>
    /// Entry point for loading a Director movie. Depending on the codec this
    /// will either read the standard memory map or the Afterburner tables.
    /// Returns <c>true</c> on success.
    /// </summary>
    public virtual bool Read(ReadStream stream)
    {
        //var rawBytes = stream.ReadByteView(stream.Size);
        //Logger.LogInformation.WriteLine("Raw CASt chunk bytes: " + BitConverter.ToString(rawBytes.Data, rawBytes.Offset, rawBytes.Size));
        //stream.Seek(0); // reset
        Stream = stream;
        stream.Endianness = Endianness.BigEndian;
        uint metaFourCC = stream.ReadUint32();
        if (metaFourCC == FOURCC('X', 'F', 'I', 'R'))
            stream.Endianness = Endianness.LittleEndian;
        Endianness = stream.Endianness;
        Logger.LogInformation($"File endianness: {Endianness}");

        

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
        ReadScore();

        return true;
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

    /// <summary>
    /// Load the standard movie memory map. This resolves the initial and
    /// memory map chunks so subsequent chunk lookups know their offsets.
    /// </summary>
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
                CompressionID = LingoGuidConstants.NULL_COMPRESSION_GUID
            };
            AddChunkInfo(info);
            Logger.LogInformation($"Registering chunk {Common.Util.FourCCToString(info.FourCC)} with ID {info.Id}");
        }
    }

    /// <summary>
    /// Parse the Afterburner resource table used by protected movies. This
    /// mirrors the C++ implementation and decodes the zlib-compressed mapping
    /// tables describing every chunk in the file.
    /// </summary>
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
        var compIDs = new List<RayGuid>();
        for (int i = 0; i < compCount; i++)
        {
            var id = new RayGuid();
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
        Version = Utilities.HumanVersion((uint)Config.DirectorVersion);
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
                        cast.Populate(entry.Name, entry.Id, entry.MinMember);
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
        //var info5 = ChunkInfoMap[7];
        //Logger.LogTrace($"CAS* Compression: {info5.CompressionID}");
        var def = GetFirstChunkInfo(FOURCC('C','A','S','*'));
        //if (def != null)
        //{
        //    var cast = (CastChunk)GetChunk(def.FourCC, def.Id);
        //    cast.Populate(internalCast ? "Internal" : "External", 1024, (ushort) Config!.MinMember);
        //    Casts.Add(cast);
        //}
        if (def != null && def.FourCC == FOURCC('C', 'A', 'S', '*'))
        {
            // You expect a CastChunk, but chunk type must actually be 'CAS*'
            var cast = (CastChunk)GetChunk(FOURCC('C', 'A', 'S', '*'), def.Id);
            cast.Populate(internalCast ? "Internal" : "External", 1024, (ushort)Config!.MinMember);
            Casts.Add(cast);
        }

        return true;
    }

    /// <summary>
    /// Attempt to load the VWSC score chunk if present. Failure is ignored
    /// as some files may omit this information.
    /// </summary>
    private void ReadScore()
    {
        var info = GetFirstChunkInfo(FOURCC('V','W','S','C'));
        if (info != null)
        {
            Score = (ScoreChunk)GetChunk(info.FourCC, info.Id);
        }
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
                if (info.CompressionID.Equals(LingoGuidConstants.ZLIB_COMPRESSION_GUID))
                    actual = s.ReadZlibBytes((int)info.Len, _cachedChunkBufs[id], _cachedChunkBufs[id].Length);
                else if (info.CompressionID.Equals(LingoGuidConstants.SND_COMPRESSION_GUID))
                {
                    // Sound decompression not implemented
                    BufferView chunkView = s.ReadByteView((int)info.Len);
                    _cachedChunkViews[id] = chunkView;
                    actual = chunkView.Size;
                }
                if (actual < 0) throw new IOException($"Chunk {id}: Could not decompress");
                _cachedChunkViews[id] = new BufferView(_cachedChunkBufs[id], actual);
            }
            else if (info.CompressionID.Equals(LingoGuidConstants.FONTMAP_COMPRESSION_GUID))
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
        //Logger.LogInformation($"Reading chunk FourCC={ProjectorRays.Common.Util.FourCCToString(fourCC)}, Offset={view.Offset}, Length={view.Size}");
        //Logger.LogInformation($"Chunk bytes:\n{view.LogHex(256,view.Size)}");

        Chunk chunk = fourCC switch
        {
            var v when v == FOURCC('i', 'm', 'a', 'p') => new InitialMapChunk(this),
            var v when v == FOURCC('m', 'm', 'a', 'p') => new MemoryMapChunk(this),
            var v when v == FOURCC('C', 'A', 'S', '*') => new CastChunk(this),
            var v when v == FOURCC('C', 'A', 'S', 't') => new CastMemberChunk(this),
            var v when v == FOURCC('K', 'E', 'Y', '*') => new KeyTableChunk(this),
            var v when v == FOURCC('L', 'c', 't', 'x') || v == FOURCC('L', 'c', 't', 'X') => new ScriptContextChunk(this),
            var v when v == FOURCC('L', 'n', 'a', 'm') => new ScriptNamesChunk(this),
            var v when v == FOURCC('L', 's', 'c', 'r') => new ScriptChunk(this),
            var v when v == FOURCC('V', 'W', 'C', 'F') || v == FOURCC('D', 'R', 'C', 'F') => new ConfigChunk(this),
            var v when v == FOURCC('M', 'C', 's', 'L') => new CastListChunk(this),
            var v when v == FOURCC('V', 'W', 'S', 'C') => new ScoreChunk(this),
            var v when v == FOURCC('X', 'M', 'E', 'D') => new XmedChunk(this, ChunkType.StyledText),
            _ => throw new IOException($"Could not deserialize '{Common.Util.FourCCToString(fourCC)}' chunk")
        };


        var isBig = IsAlwaysBigEndian(fourCC) ;
        if (isBig)
        {

        }
        var chunkStream = new ReadStream(view, isBig ? Endianness.BigEndian : Endianness);
        chunk.Read(chunkStream);
        return chunk;
    }
    private static bool IsAlwaysBigEndian(uint fourCC) => fourCC switch
    {
        var v when v == FOURCC('C', 'A', 'S', '*') => true,
        var v when v == FOURCC('C', 'A', 'S', 't') => true,
        var v when v == FOURCC('M', 'C', 's', 'L') => true,
        _ => false
    };
    public Script? GetScript(int id)
    {
        var chunk = (ScriptChunk)GetChunk(FOURCC('L','s','c','r'), id);
        return chunk.Script;
    }

    public ScriptNames? GetScriptNames(int id)
    {
        var chunk = (ScriptNamesChunk)GetChunk(FOURCC('L','n','a','m'), id);
        return chunk.Names;
    }

    /// <summary>
    /// Parse all scripts referenced by every cast so the bytecode is converted
    /// into a minimal AST. This mirrors the C++ <c>parseScripts()</c> helper.
    /// </summary>
    public void ParseScripts()
    {
        foreach (var cast in Casts)
        {
            cast.Lctx?.Context.ParseScripts();
        }
    }

    /// <summary>
    /// Restore the source text for script members by decompiling the compiled
    /// bytecode and storing it back into the cast member info structures.
    /// </summary>
    public void RestoreScriptText()
    {
        foreach (var cast in Casts)
        {
            var ctx = cast.Lctx?.Context;
            if (ctx == null) continue;

            foreach (var pair in ctx.Scripts)
            {
                uint id = pair.Key;
                var script = pair.Value;
                if (!ChunkExists(FOURCC('L','s','c','r'), (int)id))
                    continue;
                var scriptChunk = (ScriptChunk)GetChunk(FOURCC('L','s','c','r'), (int)id);
                var member = scriptChunk.Member;
                if (member != null)
                {
                    member.SetScriptText(script.ScriptText(FileIO.PlatformLineEnding, DotSyntax));
                }
            }
        }
    }

    private static bool CompressionImplemented(RayGuid id) =>
        id.Equals(LingoGuidConstants.ZLIB_COMPRESSION_GUID) || id.Equals(LingoGuidConstants.SND_COMPRESSION_GUID);
}
