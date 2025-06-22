using ProjectorRays.Common;
using ProjectorRays.Director;

namespace ProjectorRays.director.Chunks;

public class CastListChunk : ListChunk
{
    public ushort Unk0;
    public ushort CastCount;
    public ushort ItemsPerCast;
    public ushort Unk1;
    public List<CastListEntry> Entries = new();

    public CastListChunk(DirectorFile? dir) : base(dir, ChunkType.CastListChunk) { }

    public override void ReadHeader(ReadStream stream)
    {
        DataOffset = stream.ReadUint32();
        Unk0 = stream.ReadUint16();
        CastCount = stream.ReadUint16();
        ItemsPerCast = stream.ReadUint16();
        Unk1 = stream.ReadUint16();
    }

    public override void Read(ReadStream stream)
    {
        // Use the same byte order as the movie itself. Older Director versions
        // store this table in little endian on Windows, so rely on the parent
        // file to specify the correct endianness.
        stream.Endianness = Dir?.Endianness ?? Endianness.BigEndian;
        base.Read(stream);
        for (int i = 0; i < CastCount; i++)
        {
            var entry = new CastListEntry();
            if (ItemsPerCast >= 1) entry.Name = ReadPascalString(i * ItemsPerCast + 1);
            if (ItemsPerCast >= 2) entry.FilePath = ReadPascalString(i * ItemsPerCast + 2);
            if (ItemsPerCast >= 3) entry.PreloadSettings = ReadUint16(i * ItemsPerCast + 3);
            if (ItemsPerCast >= 4)
            {
                var item = Items[i * ItemsPerCast + 4];
                var isr = new ReadStream(item, ItemEndianness);
                entry.MinMember = isr.ReadUint16();
                entry.MaxMember = isr.ReadUint16();
                entry.Id = isr.ReadInt32();
            }
            Entries.Add(entry);
        }
    }

    /// <summary>
    /// Serialize this cast list chunk. Each entry is written through
    /// <see cref="CastListEntry.WriteJSON"/> so we emit an array manually.
    /// </summary>
    public override void WriteJSON(JSONWriter json)
    {
        json.StartObject();
        json.WriteKey("entries");
        json.StartArray();
        foreach (var e in Entries)
            e.WriteJSON(json);
        json.EndArray();
        json.EndObject();
    }
}
