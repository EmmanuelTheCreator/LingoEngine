using ProjectorRays.Common;
using ProjectorRays.Director;
using ProjectorRays.LingoDec;

namespace ProjectorRays.director.Chunks;

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
