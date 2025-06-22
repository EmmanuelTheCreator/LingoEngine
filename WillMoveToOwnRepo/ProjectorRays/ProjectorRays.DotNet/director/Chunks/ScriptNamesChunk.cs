using ProjectorRays.Common;
using ProjectorRays.Director;
using ProjectorRays.LingoDec;

namespace ProjectorRays.director.Chunks;

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
