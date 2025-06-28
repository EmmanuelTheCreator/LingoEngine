using Microsoft.Extensions.Logging;
using ProjectorRays.Common;
using ProjectorRays.Director;
using System.Text;

namespace ProjectorRays.director.Chunks;

public class RaysCastListChunk : RaysListChunk
{
    public ushort Unk0;
    public ushort CastCount;
    public ushort ItemsPerCast;
    public ushort Unk1;
    public List<CastListEntry> Entries = new();

    public RaysCastListChunk(RaysDirectorFile? dir) : base(dir, ChunkType.CastListChunk) { }

    public override void ReadHeader(ReadStream stream)
    {
        var check = stream.Endianness; // =  Dir?.Endianness ?? Endianness.BigEndian;

        DataOffset = stream.ReadUint32();
        //Dir?.Logger.LogInformation($"CastListChunk: DataOffset = {DataOffset}");
        Unk0 = stream.ReadUint16();
        CastCount = stream.ReadUint16();
        ItemsPerCast = stream.ReadUint16();
        Unk1 = stream.ReadUint16();
        OffsetTableLen = stream.ReadUint16();
        //Dir?.Logger.LogInformation($"CastListChunk: OffsetTableLen = {OffsetTableLen}");
        ItemsLen = stream.ReadUint32();
        //Dir?.Logger.LogInformation($"CastListChunk: ItemsLen = {ItemsLen}");
        //ItemEndianness = (Endianness)stream.ReadUint8(); <!- not part of the header

        Dir?.Logger.LogInformation($"CastListChunk: DataOffset={DataOffset}, CastCount={CastCount}, ItemsPerCast={ItemsPerCast}, OffsetTableLen={OffsetTableLen},ItemsLen={ItemsLen}, ItemEndianness={ItemEndianness}");

    }


    public override void Read(ReadStream stream)
    {
        // Use the same byte order as the movie itself. Older Director versions
        // store this table in little endian on Windows, so rely on the parent
        // file to specify the correct endianness.
        base.Read(stream);

        for (int i = 0; i < CastCount; i++)
        {
            if (i >= Items.Count || Items[i].Size == 0)
            {
                Dir?.Logger.LogWarning($"Skipping cast entry {i}: no data available.");
                Entries.Add(new CastListEntry());
                continue;
            }

            var entry = new CastListEntry();

            if (ItemsPerCast >= 1)
            {
                entry.Name = ReadPascalString(i * ItemsPerCast + 0);
            }

            if (ItemsPerCast >= 2 && (i * ItemsPerCast + 1) < Items.Count && Items[i * ItemsPerCast + 1].Size > 0)
            {
                entry.FilePath = ReadPascalString(i * ItemsPerCast + 1);
            }


            if (ItemsPerCast >= 3)
            {
                var preloadStream = new ReadStream(Items[i * ItemsPerCast + 2], ItemEndianness);
                entry.PreloadSettings = preloadStream.ReadUint16();
            }

            if (ItemsPerCast >= 4)
            {
                var extraStream = new ReadStream(Items[i * ItemsPerCast + 3], ItemEndianness);
                entry.MinMember = extraStream.ReadUint16();
                entry.MaxMember = extraStream.ReadUint16();
                entry.Id = extraStream.ReadInt32();
            }

            Entries.Add(entry);
        }
    }
    private string ReadAsciiString(BufferView buffer)
    {
        return Encoding.ASCII.GetString(buffer.Data, buffer.Offset, buffer.Size).TrimEnd('\0');
    }


    /// <summary>
    /// Serialize this cast list chunk. Each entry is written through
    /// <see cref="CastListEntry.WriteJSON"/> so we emit an array manually.
    /// </summary>
    public override void WriteJSON(RaysJSONWriter json)
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
