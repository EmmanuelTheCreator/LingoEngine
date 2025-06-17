using ProjectorRays.Common;
using ProjectorRays.Director;

namespace ProjectorRays.director.Chunks;

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
