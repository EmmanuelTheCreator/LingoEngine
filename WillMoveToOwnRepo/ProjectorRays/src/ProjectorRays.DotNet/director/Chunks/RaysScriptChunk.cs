using ProjectorRays.Common;
using ProjectorRays.Director;
using ProjectorRays.LingoDec;

namespace ProjectorRays.director.Chunks;

public class RaysScriptChunk : RaysChunk
{
    public RaysCastMemberChunk? Member;
    public RaysScript Script;

    public RaysScriptChunk(RaysDirectorFile? dir) : base(dir, ChunkType.ScriptChunk)
    {
        Script = new RaysScript(dir?.Version ?? 0);
    }

    public override void Read(ReadStream stream)
    {
        Script.Read(stream);
    }

    public override void WriteJSON(RaysJSONWriter json)
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

    private static void WriteHandlerJSON(RaysHandler h, RaysJSONWriter json)
    {
        json.StartObject();
        json.WriteField("nameID", h.NameID);
        json.WriteField("argumentCount", h.ArgumentCount);
        json.WriteField("localsCount", h.LocalsCount);
        json.WriteField("globalsCount", h.GlobalsCount);
        json.EndObject();
    }
}
