using Microsoft.Extensions.Logging;
using ProjectorRays.Common;
using ProjectorRays.Director;

namespace ProjectorRays.director.Chunks;

public class RaysListChunk : RaysChunk
{
    public uint DataOffset;
    public ushort OffsetTableLen;
    public List<uint> OffsetTable = new();
    public uint ItemsLen;
    public Endianness ItemEndianness;
    public List<BufferView> Items = new();

    public RaysListChunk(RaysDirectorFile? dir, ChunkType type) : base(dir, type) { }

    public override void Read(ReadStream stream)
    {
        ReadHeader(stream);
        ReadOffsetTable(stream);
        ReadItems(stream);
    }

    public virtual void ReadHeader(ReadStream stream)
    {
        DataOffset = stream.ReadUint32();
        OffsetTableLen = stream.ReadUint16();
        ItemsLen = stream.ReadUint32();
        ItemEndianness = stream.Endianness;
    }

    public void ReadOffsetTable(ReadStream stream)
    {
        
        for (int i = 0; i < OffsetTableLen; i++)
            OffsetTable.Add(stream.ReadUint32());

        //Dir?.Logger.LogInformation($"ReadOffsetTable: {string.Join(',', OffsetTable)}");
    }

    public void ReadItems(ReadStream stream)
    {
        for (int i = 0; i < OffsetTableLen; i++)
        {
            uint start = OffsetTable[i];
            uint end = i + 1 < OffsetTableLen ? OffsetTable[i + 1] : ItemsLen;
            int len = (int)(end - start);
            if (len <= 0)
            {
                Dir?.Logger.LogWarning($"Skipping item {i} due to invalid or zero-length span: start={start}, end={end}");
                Items.Add(BufferView.Empty);
                continue;
            }
            Items.Add(stream.ReadByteView(len));
        }
    }










    public string ReadString(int index)
    {
        var view = Items[index];
        return System.Text.Encoding.UTF8.GetString(view.Data, view.Offset, view.Size);
    }

    public string ReadPascalString(int index)
    {
        var view = Items[index];
        var rs = new ReadStream(view);
        var len = rs.ReadUint8();
        return System.Text.Encoding.UTF8.GetString(view.Data, view.Offset + 1, len);
    }

    public ushort ReadUint16(int index)
    {
        var rs = new ReadStream(Items[index], ItemEndianness);
        return rs.ReadUint16();
    }

    public uint ReadUint32(int index)
    {
        var rs = new ReadStream(Items[index], ItemEndianness);
        return rs.ReadUint32();
    }
}
