using ProjectorRays.Common;
using ProjectorRays.Director;
using ProjectorRays.LingoDec;

namespace ProjectorRays.director.Chunks;

public class RaysScriptContextChunk : RaysChunk
{
    public ScriptContext Context;

    public RaysScriptContextChunk(RaysDirectorFile? dir) : base(dir, ChunkType.ScriptContextChunk)
    {
        Context = new ScriptContext(dir?.Version ?? 0, dir!);
    }

    public override void Read(ReadStream stream)
    {
        Context.Read(stream);
    }

    public override void WriteJSON(RaysJSONWriter json)
    {
        json.StartObject();
        json.WriteField("entryCount", Context.EntryCount);
        json.WriteField("flags", Context.Flags);
        json.EndObject();
    }
}
