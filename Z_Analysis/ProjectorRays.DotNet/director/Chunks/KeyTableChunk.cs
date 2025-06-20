using ProjectorRays.Common;
using ProjectorRays.Director;

namespace ProjectorRays.director.Chunks;

public class KeyTableChunk : Chunk
{
    public List<KeyTableEntry> Entries = new();

    public KeyTableChunk(DirectorFile? dir) : base(dir, ChunkType.KeyTableChunk) { }

    public override void Read(ReadStream stream)
    {
        // Use the movie's endianness to decode the key table correctly.
        stream.Endianness = Dir?.Endianness ?? Endianness.BigEndian;
        ushort entrySize = stream.ReadUint16();
        stream.Skip(2);
        uint count = stream.ReadUint32();
        uint used = stream.ReadUint32();
        for (int i = 0; i < used; i++)
        {
            var entry = new KeyTableEntry();
            entry.Read(stream);
            Entries.Add(entry);
        }
    }

    public override void WriteJSON(JSONWriter json)
    {
        json.StartObject();
        json.WriteField("entries", Entries.Count);
        json.WriteKey("entriesList");
        json.StartArray();
        foreach (var e in Entries)
        {
            e.WriteJSON(json);
        }
        json.EndArray();
        json.EndObject();
    }
}
