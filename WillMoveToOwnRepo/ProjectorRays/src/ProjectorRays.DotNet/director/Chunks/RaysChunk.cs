using System.Buffers.Binary;
using System.Collections.Generic;
using ProjectorRays.Common;
using ProjectorRays.Director;

namespace ProjectorRays.director.Chunks;

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
    ScoreChunk,
    XmedChunk,
    StyledText
}

public abstract class RaysChunk
{
    public RaysDirectorFile? Dir;
    public ChunkType ChunkType;
    public bool Writable;

    protected RaysChunk(RaysDirectorFile? dir, ChunkType type)
    {
        Dir = dir;
        ChunkType = type;
    }

    public abstract void Read(ReadStream stream);

    public virtual void WriteJSON(RaysJSONWriter json)
    {
        json.StartObject();
        json.WriteField("chunkType", ChunkType.ToString());
        json.EndObject();
    }
}
