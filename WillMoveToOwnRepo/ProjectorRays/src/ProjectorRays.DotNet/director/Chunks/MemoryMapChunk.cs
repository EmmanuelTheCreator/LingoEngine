using Microsoft.Extensions.Logging;
using ProjectorRays.Common;
using ProjectorRays.Director;

namespace ProjectorRays.director.Chunks;

public class MemoryMapChunk : Chunk
{
    public List<MemoryMapEntry> MapArray = new();

    public MemoryMapChunk(DirectorFile? dir) : base(dir, ChunkType.MemoryMapChunk)
    {
        Writable = true;
    }

    public override void Read(ReadStream stream)
    {
        // Memory map data uses the same byte order as the movie container.
        stream.Endianness = Dir?.Endianness ?? Endianness.BigEndian;
        short headerLen = stream.ReadInt16();
        short entryLen = stream.ReadInt16();
        int countMax = stream.ReadInt32();
        int countUsed = stream.ReadInt32();
        int junkHead = stream.ReadInt32();
        int junkHead2 = stream.ReadInt32();
        int freeHead = stream.ReadInt32();
        for (int i = 0; i < countUsed; i++)
        {
            var entry = new MemoryMapEntry();
            entry.Read(stream);
            MapArray.Add(entry);
        }
        Dir!.Logger.LogInformation($"MemoryMapChunk: Entries={countUsed}, HeaderLen={headerLen}, EntryLen={entryLen}, CountMax={countMax}, MapArray.Count={MapArray.Count}");

    }

    public override void WriteJSON(JSONWriter json)
    {
        json.StartObject();
        json.WriteField("entries", MapArray.Count);
        json.WriteKey("mapArray");
        json.StartArray();
        foreach (var e in MapArray)
            e.WriteJSON(json);
        json.EndArray();
        json.EndObject();
    }
}
