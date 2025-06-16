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
    ScriptNamesChunk
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
        stream.Endianness = Endianness.BigEndian;
        while (!stream.Eof)
            MemberIDs.Add(stream.ReadInt32());
    }

    public override void WriteJSON(JSONWriter json)
    {
        json.StartObject();
        json.WriteField("memberIDs", MemberIDs);
        json.EndObject();
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
        stream.Endianness = Endianness.BigEndian;
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

    public override void WriteJSON(JSONWriter json)
    {
        json.StartObject();
        json.WriteField("entries", Entries);
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
        stream.Endianness = Endianness.BigEndian;
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
        json.EndObject();
    }
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
    }
}

public class ConfigChunk : Chunk
{
    public short FileVersion;
    public short DirectorVersion;

    public ConfigChunk(DirectorFile? dir) : base(dir, ChunkType.ConfigChunk)
    {
        Writable = true;
    }

    public override void Read(ReadStream stream)
    {
        stream.Endianness = Endianness.BigEndian;
        FileVersion = stream.ReadInt16();
        stream.Skip(66);
        DirectorVersion = stream.ReadInt16();
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
}

public class KeyTableChunk : Chunk
{
    public List<KeyTableEntry> Entries = new();

    public KeyTableChunk(DirectorFile? dir) : base(dir, ChunkType.KeyTableChunk) {}

    public override void Read(ReadStream stream)
    {
        stream.Endianness = Endianness.BigEndian;
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
        stream.Endianness = Endianness.BigEndian;
        short headerLen = stream.ReadInt16();
        short entryLen = stream.ReadInt16();
        int countMax = stream.ReadInt32();
        int countUsed = stream.ReadInt32();
        stream.Skip(12);
        for (int i = 0; i < countUsed; i++)
        {
            var entry = new MemoryMapEntry();
            entry.Read(stream);
            MapArray.Add(entry);
        }
    }
}

public class ScriptChunk : Chunk, Script
{
    public CastMemberChunk? Member;

    public ScriptChunk(DirectorFile? dir) : base(dir, ChunkType.ScriptChunk) {}

    public override void Read(ReadStream stream)
    {
        // not implemented
    }

    public override void WriteJSON(JSONWriter json)
    {
        json.StartObject();
        json.EndObject();
    }
}

public class ScriptContextChunk : Chunk, ScriptContext
{
    public ScriptContextChunk(DirectorFile? dir) : base(dir, ChunkType.ScriptContextChunk) {}

    public override void Read(ReadStream stream)
    {
        // placeholder
    }
}

public class ScriptNamesChunk : Chunk, ScriptNames
{
    public List<string> Names = new();
    public ScriptNamesChunk(DirectorFile? dir) : base(dir, ChunkType.ScriptNamesChunk) {}

    public override void Read(ReadStream stream)
    {
        // placeholder
    }
}
