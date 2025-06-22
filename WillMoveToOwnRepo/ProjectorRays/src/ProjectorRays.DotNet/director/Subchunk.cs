using ProjectorRays.Common;

namespace ProjectorRays.Director;

public class CastListEntry
{
    public string Name = string.Empty;
    public string FilePath = string.Empty;
    public ushort PreloadSettings;
    public ushort MinMember;
    public ushort MaxMember;
    public int Id;

    public void WriteJSON(JSONWriter json)
    {
        json.StartObject();
        json.WriteField("name", Name);
        json.WriteField("filePath", FilePath);
        json.WriteField("preloadSettings", PreloadSettings);
        json.WriteField("minMember", MinMember);
        json.WriteField("maxMember", MaxMember);
        json.WriteField("id", Id);
        json.EndObject();
    }
}

public struct MemoryMapEntry
{
    public uint FourCC;
    public uint Len;
    public int Offset;
    public ushort Flags;
    public short Unknown0;
    public int Next;

    public void Read(ReadStream stream)
    {
        FourCC = stream.ReadUint32();
        Len = stream.ReadUint32();
        Offset = stream.ReadInt32();
        Flags = stream.ReadUint16();
        Unknown0 = stream.ReadInt16();
        Next = stream.ReadInt32();
    }

    public void Write(WriteStream stream)
    {
        stream.WriteUint32(FourCC);
        stream.WriteUint32(Len);
        stream.WriteInt32(Offset);
        stream.WriteUint16(Flags);
        stream.WriteInt16(Unknown0);
        stream.WriteInt32(Next);
    }

    public void WriteJSON(JSONWriter json)
    {
        json.StartObject();
        json.WriteFourCCField("fourCC", FourCC);
        json.WriteField("len", Len);
        json.WriteField("offset", Offset);
        json.WriteField("flags", Flags);
        json.WriteField("unknown0", Unknown0);
        json.WriteField("next", Next);
        json.EndObject();
    }
}

public class KeyTableEntry
{
    public uint FourCC;
    public ushort ResourceID;
    public ushort SectionID;
    public int CastID;

    public void Read(ReadStream stream)
    {
        FourCC = stream.ReadUint32();
        ResourceID = stream.ReadUint16();   // ← this line is **likely missing**
        SectionID = stream.ReadUint16();
        CastID = stream.ReadInt32();
    }

    public void WriteJSON(JSONWriter json)
    {
        json.StartObject();
        json.WriteField("fourCC", Common.Util.FourCCToString(FourCC));
        json.WriteField("resourceID", ResourceID);
        json.WriteField("sectionID", SectionID);
        json.WriteField("castID", CastID);
        json.EndObject();
    }
}

