using System.Collections.Generic;
using ProjectorRays.Common;
using ProjectorRays.LingoDec;

namespace ProjectorRays.Director;

public enum ChunkType
{
    CastChunk,
    CastListChunk,
    CastMemberChunk,
    CastInfoChunk,
    ConfigChunk,
    InitialMapChunk,
    KeyTableChunk,
    MemoryMapChunk,
    ScriptChunk,
    ScriptContextChunk,
    ScriptNamesChunk,
    ScoreChunk
}

public abstract class Chunk
{
    public DirectorFile? Dir;
    public ChunkType ChunkType;
    public bool Writable;

    protected Chunk(DirectorFile? dir, ChunkType type)
    {
        Dir = dir;
        ChunkType = type;
    }

    public abstract void Read(ReadStream stream);

    public virtual void WriteJSON(JSONWriter json)
    {
        json.StartObject();
        json.WriteField("chunkType", ChunkType.ToString());
        json.EndObject();
    }
}

public class ListChunk : Chunk
{
    public uint DataOffset;
    public ushort OffsetTableLen;
    public List<uint> OffsetTable = new();
    public uint ItemsLen;
    public Endianness ItemEndianness;
    public List<BufferView> Items = new();

    public ListChunk(DirectorFile? dir, ChunkType type) : base(dir, type) {}

    public override void Read(ReadStream stream)
    {
        ReadHeader(stream);
        ReadOffsetTable(stream);
        ReadItems(stream);
    }

    public virtual void ReadHeader(ReadStream stream)
    {
        DataOffset = stream.ReadUint32();
        OffsetTableLen = stream.ReadUint16();
        ItemsLen = stream.ReadUint32();
        ItemEndianness = stream.Endianness;
    }

    public void ReadOffsetTable(ReadStream stream)
    {
        for (int i = 0; i < OffsetTableLen; i++)
            OffsetTable.Add(stream.ReadUint32());
    }

    public void ReadItems(ReadStream stream)
    {
        for (int i = 0; i < OffsetTableLen; i++)
        {
            uint start = OffsetTable[i];
            uint end = i + 1 < OffsetTableLen ? OffsetTable[i + 1] : ItemsLen;
            Items.Add(stream.ReadByteView((int)(end - start)));
        }
    }

    public string ReadString(int index)
    {
        var view = Items[index];
        return System.Text.Encoding.UTF8.GetString(view.Data, view.Offset, view.Size);
    }

    public string ReadPascalString(int index)
    {
        var view = Items[index];
        var rs = new ReadStream(view);
        var len = rs.ReadUint8();
        return System.Text.Encoding.UTF8.GetString(view.Data, view.Offset + 1, len);
    }

    public ushort ReadUint16(int index)
    {
        var rs = new ReadStream(Items[index], ItemEndianness);
        return rs.ReadUint16();
    }

    public uint ReadUint32(int index)
    {
        var rs = new ReadStream(Items[index], ItemEndianness);
        return rs.ReadUint32();
    }
}

public class CastChunk : Chunk
{
    public List<int> MemberIDs = new();
    public string Name = string.Empty;
    public Dictionary<ushort, CastMemberChunk> Members = new();
    public ScriptContextChunk? Lctx;

    public CastChunk(DirectorFile? dir) : base(dir, ChunkType.CastChunk) {}

    public override void Read(ReadStream stream)
    {
        // The memory map entries follow the same byte order as the parent file.
        // Use the endianness of the owning DirectorFile instead of forcing
        // big endian. Movies created on Windows use little endian here.
        stream.Endianness = Dir?.Endianness ?? Endianness.BigEndian;
        while (!stream.Eof)
            MemberIDs.Add(stream.ReadInt32());
    }

    /// <summary>
    /// Serialize this cast chunk to JSON. The member list is written as an
    /// array of integers since <see cref="JSONWriter"/> only handles
    /// primitives directly.
    /// </summary>
    public override void WriteJSON(JSONWriter json)
    {
        json.StartObject();
        json.WriteKey("memberIDs");
        json.StartArray();
        foreach (var id in MemberIDs)
            json.WriteVal(id);
        json.EndArray();
        json.EndObject();
    }

    /// <summary>
    /// Populate this cast chunk by loading all referenced members and linking
    /// script resources. This mirrors the original C++ populate() method and is
    /// required so that script restoration knows which member owns which script.
    /// </summary>
    public void Populate(string castName, int id, ushort minMember)
    {
        Name = castName;

        // Look up the script context for this cast using the key table.
        foreach (var entry in Dir!.KeyTable!.Entries)
        {
            if (entry.CastID == id &&
               (entry.FourCC == DirectorFile.FOURCC('L','c','t','x') ||
                entry.FourCC == DirectorFile.FOURCC('L','c','t','X')) &&
               Dir.ChunkExists(entry.FourCC, entry.SectionID))
            {
                Lctx = (ScriptContextChunk)Dir.GetChunk(entry.FourCC, entry.SectionID);
                break;
            }
        }

        // Load each member chunk referenced by this cast and assign an ID.
        for (int i = 0; i < MemberIDs.Count; i++)
        {
            int sectionID = MemberIDs[i];
            if (sectionID <= 0) continue;

            var member = (CastMemberChunk)Dir.GetChunk(DirectorFile.FOURCC('C','A','S','t'), sectionID);
            member.Id = (ushort)(i + minMember);
            Members[member.Id] = member;

            uint scriptId = member.GetScriptID();
            if (scriptId != 0 && Dir.ChunkExists(DirectorFile.FOURCC('L','s','c','r'), (int)scriptId))
            {
                var scriptChunk = (ScriptChunk)Dir.GetChunk(DirectorFile.FOURCC('L','s','c','r'), (int)scriptId);
                member.Script = scriptChunk;
                scriptChunk.Member = member;
            }
        }
    }
}

public class CastListChunk : ListChunk
{
    public ushort Unk0;
    public ushort CastCount;
    public ushort ItemsPerCast;
    public ushort Unk1;
    public List<CastListEntry> Entries = new();

    public CastListChunk(DirectorFile? dir) : base(dir, ChunkType.CastListChunk) {}

    public override void ReadHeader(ReadStream stream)
    {
        DataOffset = stream.ReadUint32();
        Unk0 = stream.ReadUint16();
        CastCount = stream.ReadUint16();
        ItemsPerCast = stream.ReadUint16();
        Unk1 = stream.ReadUint16();
    }

    public override void Read(ReadStream stream)
    {
        // Use the same byte order as the movie itself. Older Director versions
        // store this table in little endian on Windows, so rely on the parent
        // file to specify the correct endianness.
        stream.Endianness = Dir?.Endianness ?? Endianness.BigEndian;
        base.Read(stream);
        for (int i = 0; i < CastCount; i++)
        {
            var entry = new CastListEntry();
            if (ItemsPerCast >= 1) entry.Name = ReadPascalString(i * ItemsPerCast + 1);
            if (ItemsPerCast >= 2) entry.FilePath = ReadPascalString(i * ItemsPerCast + 2);
            if (ItemsPerCast >= 3) entry.PreloadSettings = ReadUint16(i * ItemsPerCast + 3);
            if (ItemsPerCast >= 4)
            {
                var item = Items[i * ItemsPerCast + 4];
                var isr = new ReadStream(item, ItemEndianness);
                entry.MinMember = isr.ReadUint16();
                entry.MaxMember = isr.ReadUint16();
                entry.Id = isr.ReadInt32();
            }
            Entries.Add(entry);
        }
    }

    /// <summary>
    /// Serialize this cast list chunk. Each entry is written through
    /// <see cref="CastListEntry.WriteJSON"/> so we emit an array manually.
    /// </summary>
    public override void WriteJSON(JSONWriter json)
    {
        json.StartObject();
        json.WriteKey("entries");
        json.StartArray();
        foreach (var e in Entries)
            e.WriteJSON(json);
        json.EndArray();
        json.EndObject();
    }
}

public class CastMemberChunk : Chunk
{
    public MemberType Type;
    public uint InfoLen;
    public uint SpecificDataLen;
    public CastInfoChunk? Info;
    public BufferView SpecificData;
    public CastMember? Member;
    public bool HasFlags1;
    public byte Flags1;
    public ushort Id;
    public ScriptChunk? Script;

    public CastMemberChunk(DirectorFile? dir) : base(dir, ChunkType.CastMemberChunk)
    {
        Writable = true;
    }

    public override void Read(ReadStream stream)
    {
        // Respect the movie's byte order for configuration data as well.
        stream.Endianness = Dir?.Endianness ?? Endianness.BigEndian;
        Type = (MemberType)stream.ReadUint32();
        InfoLen = stream.ReadUint32();
        SpecificDataLen = stream.ReadUint32();
        if (InfoLen > 0)
        {
            var infoStream = new ReadStream(stream.ReadByteView((int)InfoLen), stream.Endianness);
            Info = new CastInfoChunk(Dir);
            Info.Read(infoStream);
        }
        HasFlags1 = false;
        SpecificData = stream.ReadByteView((int)SpecificDataLen);
    }

    public override void WriteJSON(JSONWriter json)
    {
        json.StartObject();
        json.WriteField("type", Type.ToString());
        json.WriteField("id", Id);
        if (Info != null)
        {
            json.WriteKey("info");
            Info.WriteJSON(json);
        }
        json.EndObject();
    }

    public uint GetScriptID() => Info?.ScriptId ?? 0;
    public string GetScriptText() => Info?.ScriptSrcText ?? string.Empty;
    public void SetScriptText(string val) { if (Info != null) Info.ScriptSrcText = val; }
    public string GetName() => Info?.Name ?? string.Empty;
}

public class CastInfoChunk : ListChunk
{
    public uint Flags;
    public uint ScriptId;
    public string ScriptSrcText = string.Empty;
    public string Name = string.Empty;

    public CastInfoChunk(DirectorFile? dir) : base(dir, ChunkType.CastInfoChunk)
    {
        Writable = true;
    }

    public override void ReadHeader(ReadStream stream)
    {
        DataOffset = stream.ReadUint32();
        Flags = stream.ReadUint32();
        ScriptId = stream.ReadUint32();
    }

    public override void Read(ReadStream stream)
    {
        base.Read(stream);
        if (OffsetTableLen > 0 && Items.Count >= 1)
            ScriptSrcText = ReadString(0);
        if (OffsetTableLen > 1 && Items.Count >= 2)
            Name = ReadPascalString(1);
        if (OffsetTableLen == 0)
        {
            // Ensure there is one entry so decompilation results can be stored
            OffsetTableLen = 1;
            OffsetTable.Add(0);
        }
    }

    public override void WriteJSON(JSONWriter json)
    {
        json.StartObject();
        json.WriteField("dataOffset", DataOffset);
        json.WriteField("flags", Flags);
        json.WriteField("scriptId", ScriptId);
        json.WriteField("scriptSrcText", ScriptSrcText);
        json.WriteField("name", Name);
        json.EndObject();
    }
}

public class ConfigChunk : Chunk
{
    public short FileVersion;
    public short DirectorVersion;
    public short MinMember;

    public ConfigChunk(DirectorFile? dir) : base(dir, ChunkType.ConfigChunk)
    {
        Writable = true;
    }

    public override void Read(ReadStream stream)
    {
        // Use the director file's byte order so configuration values are
        // interpreted correctly on both Windows (little endian) and Mac
        // (big endian) authored movies.
        stream.Endianness = Dir?.Endianness ?? Endianness.BigEndian;
        short len = stream.ReadInt16();
        FileVersion = stream.ReadInt16();
        stream.Skip(8); // movie rect
        MinMember = stream.ReadInt16();
        stream.Skip(2); // maxMember
        stream.Seek(36);
        DirectorVersion = stream.ReadInt16();
        stream.Seek(len); // skip the remainder
    }

    public override void WriteJSON(JSONWriter json)
    {
        json.StartObject();
        json.WriteField("fileVersion", FileVersion);
        json.WriteField("minMember", MinMember);
        json.WriteField("directorVersion", DirectorVersion);
        json.EndObject();
    }
}

public class InitialMapChunk : Chunk
{
    public uint Version;
    public uint MmapOffset;

    public InitialMapChunk(DirectorFile? dir) : base(dir, ChunkType.InitialMapChunk)
    {
        Writable = true;
    }

    public override void Read(ReadStream stream)
    {
        Version = stream.ReadUint32();
        MmapOffset = stream.ReadUint32();
        stream.Skip(12);
    }

    public override void WriteJSON(JSONWriter json)
    {
        json.StartObject();
        json.WriteField("version", Version);
        json.WriteField("mmapOffset", MmapOffset);
        json.EndObject();
    }
}

public class KeyTableChunk : Chunk
{
    public List<KeyTableEntry> Entries = new();

    public KeyTableChunk(DirectorFile? dir) : base(dir, ChunkType.KeyTableChunk) {}

    public override void Read(ReadStream stream)
    {
        // Use the movie's endianness to decode the key table correctly.
        stream.Endianness = Dir?.Endianness ?? Endianness.BigEndian;
        ushort entrySize = stream.ReadUint16();
        stream.Skip(2);
        uint count = stream.ReadUint32();
        uint used = stream.ReadUint32();
        for (int i = 0; i < used; i++)
        {
            var entry = new KeyTableEntry();
            entry.Read(stream);
            Entries.Add(entry);
        }
    }

    public override void WriteJSON(JSONWriter json)
    {
        json.StartObject();
        json.WriteField("entries", Entries.Count);
        json.WriteKey("entriesList");
        json.StartArray();
        foreach (var e in Entries)
        {
            e.WriteJSON(json);
        }
        json.EndArray();
        json.EndObject();
    }
}

public class MemoryMapChunk : Chunk
{
    public List<MemoryMapEntry> MapArray = new();

    public MemoryMapChunk(DirectorFile? dir) : base(dir, ChunkType.MemoryMapChunk)
    {
        Writable = true;
    }

    public override void Read(ReadStream stream)
    {
        // Memory map data uses the same byte order as the movie container.
        stream.Endianness = Dir?.Endianness ?? Endianness.BigEndian;
        short headerLen = stream.ReadInt16();
        short entryLen = stream.ReadInt16();
        int countMax = stream.ReadInt32();
        int countUsed = stream.ReadInt32();
        int junkHead = stream.ReadInt32();
        int junkHead2 = stream.ReadInt32();
        int freeHead = stream.ReadInt32();
        for (int i = 0; i < countUsed; i++)
        {
            var entry = new MemoryMapEntry();
            entry.Read(stream);
            MapArray.Add(entry);
        }
    }

    public override void WriteJSON(JSONWriter json)
    {
        json.StartObject();
        json.WriteField("entries", MapArray.Count);
        json.WriteKey("mapArray");
        json.StartArray();
        foreach (var e in MapArray)
            e.WriteJSON(json);
        json.EndArray();
        json.EndObject();
    }
}

public class ScriptChunk : Chunk
{
    public CastMemberChunk? Member;
    public Script Script;

    public ScriptChunk(DirectorFile? dir) : base(dir, ChunkType.ScriptChunk)
    {
        Script = new Script(dir?.Version ?? 0);
    }

    public override void Read(ReadStream stream)
    {
        Script.Read(stream);
    }

    public override void WriteJSON(JSONWriter json)
    {
        json.StartObject();
        json.WriteField("scriptNumber", Script.ScriptNumber);
        json.WriteField("handlersCount", Script.HandlersCount);
        json.WriteKey("handlers");
        json.StartArray();
        foreach (var h in Script.Handlers)
            WriteHandlerJSON(h, json);
        json.EndArray();
        json.EndObject();
    }

    private static void WriteHandlerJSON(Handler h, JSONWriter json)
    {
        json.StartObject();
        json.WriteField("nameID", h.NameID);
        json.WriteField("argumentCount", h.ArgumentCount);
        json.WriteField("localsCount", h.LocalsCount);
        json.WriteField("globalsCount", h.GlobalsCount);
        json.EndObject();
    }
}

public class ScriptContextChunk : Chunk
{
    public ScriptContext Context;

    public ScriptContextChunk(DirectorFile? dir) : base(dir, ChunkType.ScriptContextChunk)
    {
        Context = new ScriptContext(dir?.Version ?? 0, dir!);
    }

    public override void Read(ReadStream stream)
    {
        Context.Read(stream);
    }

    public override void WriteJSON(JSONWriter json)
    {
        json.StartObject();
        json.WriteField("entryCount", Context.EntryCount);
        json.WriteField("flags", Context.Flags);
        json.EndObject();
    }
}

public class ScriptNamesChunk : Chunk
{
    public ScriptNames Names;

    public ScriptNamesChunk(DirectorFile? dir) : base(dir, ChunkType.ScriptNamesChunk)
    {
        Names = new ScriptNames(dir?.Version ?? 0);
    }

    public override void Read(ReadStream stream)
    {
        Names.Read(stream);
    }

    public override void WriteJSON(JSONWriter json)
    {
        json.StartObject();
        json.WriteField("namesCount", Names.Names.Count);
        json.WriteKey("names");
        json.StartArray();
        foreach (var n in Names.Names)
            json.WriteVal(n);
        json.EndArray();
        json.EndObject();
    }
}
