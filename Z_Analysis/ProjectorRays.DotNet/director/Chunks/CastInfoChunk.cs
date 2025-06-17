using ProjectorRays.Common;
using ProjectorRays.Director;

namespace ProjectorRays.director.Chunks;

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
