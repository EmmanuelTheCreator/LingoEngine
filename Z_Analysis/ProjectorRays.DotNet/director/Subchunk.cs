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

public struct KeyTableEntry
{
    public int SectionID;
    public int CastID;
    public uint FourCC;

    public void Read(ReadStream stream)
    {
        SectionID = stream.ReadInt32();
        CastID = stream.ReadInt32();
        FourCC = stream.ReadUint32();
    }

    public void WriteJSON(JSONWriter json)
    {
        json.StartObject();
        json.WriteField("sectionID", SectionID);
        json.WriteField("castID", CastID);
        json.WriteFourCCField("fourCC", FourCC);
        json.EndObject();
    }
}

