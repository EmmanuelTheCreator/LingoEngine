using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using ProjectorRays.Common;
using ProjectorRays.director;
using ProjectorRays.director.Chunks;

namespace ProjectorRays.Director;

/// <summary>
/// Represents the Score (VWSC) timeline information. Only the frame interval
/// descriptors are parsed which provide the start and end frame for each sprite.
/// </summary>
public class ScoreChunk : Chunk
{
    public class FrameDescriptor
    {
        public int StartFrame;
        public int EndFrame;
        public int Channel;
        public int SpriteNumber;
        public byte[] RawData;  // store all raw bytes for this descriptor (48 bytes)
    }

    // Store all header fields explicitly for inspection
    public int MemHandleSize;
    public int HeaderType;
    public int OffsetsOffset;
    public int OffsetsCount;
    public int NotationBase;
    public int NotationSize;

    public int FramesEndOffset;
    public uint UnknownUint32;
    public int FramesCount;

    public short FramesType;
    public short ChannelSize;
    public short LastChannelMax;
    public short LastChannel;

    // Store frame definitions and property tables raw
    public List<byte[]> FrameDefinitionsRaw = new();
    public List<byte[]> PropertyTablesRaw = new();

    public List<FrameDescriptor> Frames { get; } = new();

    public ScoreChunk(DirectorFile? dir) : base(dir, ChunkType.ScoreChunk) { }

/*
Bytes type 1 : 6E: 02 2C 00 00 02 2C 00 00 02 2C 00 00 02 2C 00 00 02 2C 00 00 02 2C 00 00 02 2C 00 00 02 58 00 00 02 58 00 00 02 58 00 00 02 84 00 00 02 84 00 00 02 84 00 00 02 B0 00 00 02 B0 00 00 02 B0 00 00 02 DC 00 00 02 DC 00 00 02 DC 00 00 01 E6 00 00 00 14 00 00 00 0F 00 0D 00 30 03 EE 00 96 00 36 00 30 01 20 10 00  |.,...,...,...,...,...,...,...X...X...X...................................................0.....6.0. ..|
Bytes type 1 : D4: FF 00 00 01 00 02 00 00 00 12 00 0A 00 09 00 11 00 11 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 08 00 02 01 36 81 00 00 08 00 02 01 66 01 00 00 56 00 08 01 20 00 00 00 00 00 00 00 00 00 0A 01 2A 00 00 00 00 00 00 00 00 00 00 00 02 01 36 00 00 00 30 01 50 10 00  |...................................................6.......f...V... ...........*.............6...0.P..|
Bytes type 1 :13A: FF 00 00 01 00 02 00 00 00 1B 00 0C 00 0B 00 11 00 11 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 0E 00 02 01 66 81 00 00 02 01 96 01 00 00 02 00 56 00 08 01 50 00 00 00 00 00 00 00 00 00 0A 01 5A 00 00 00 00 00 00 00 00 00 00 00 02 01 66 01 00 00 30 01 80 10 00  |...................................................f...........V...P...........Z.............f...0....|
Bytes type 1 :1A0: FF 00 00 01 00 02 00 00 00 1E 00 0E 00 0D 00 11 00 11 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 0E 00 02 01 66 00 00 00 02 01 96 81 00 00 02 00 56 00 08 01 80 00 00 00 00 00 00 00 00 00 0A 01 8A 00 00 00 00 00 00 00 00 00 00 00 02 01 96 00 00 00 30 01 B0 10 00  |...................................................f...........V.................................0....|
Bytes type 1 :206: FF 00 00 01 00 02 00 00 00 21 00 10 00 0F 00 11 00 11 00 00 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 08 00 02 01 C6 81 00 00 02 00 56 00 08 01 B0 00 00 00 00 00 00 00 00 00 0A 01 BA 00 00 00 00 00 00 00 00 00 00 00 02 01 C6 00 00 00 30 01 E0 10 00 FF 00 00 01 00 02  |.........!...............................................V.................................0..........|
Bytes type 2 :2B8: 00 00 00 01 00 00 00 03 00 00 00 00 00 00 00 00 00 00 00 06 00 01 00 00 00 0F E1 FD 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00  |............................................|
Bytes type 2 :2E4: 00 00 00 04 00 00 00 06 00 00 00 00 00 00 00 00 00 00 00 07 00 01 00 00 00 0F E1 FD 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00  |............................................|
Bytes type 2 :310: 00 00 00 07 00 00 00 09 00 00 00 00 00 00 00 00 00 00 00 08 00 01 00 00 00 0F E1 FD 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00  |............................................|
Bytes type 2 :33C: 00 00 00 0A 00 00 00 0C 00 00 00 00 00 00 00 00 00 00 00 09 00 01 00 00 00 0F E1 FD 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00  |............................................|
Bytes type 2 :368: 00 00 00 0D 00 00 00 0F 00 00 00 00 00 00 00 00 00 00 00 0A 00 01 00 00 00 0F E1 FD 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00  |............................................|

*/
    public override void Read(ReadStream stream)
    {
        var bytes = stream.LogHex(stream.Size);
        stream.Endianness = Endianness.BigEndian;
        var target = new byte[stream.Data.Length- 0x7580];
        //Array.Copy(stream.Data, 12170, target, 0, target.Length);
        Array.Copy(stream.Data, 0x7580, target, 0, target.Length);
        var bytes2raw = Utilities.DumpHexWithAscii(target,0,target.Length,false);
        //Dir.Logger.LogInformation(Utilities.LogHex(target, target.Length));

        var blockSize = 7*16 ;
        var start = 0xD4 - blockSize;
        for (int i = start; i < 0x26C; i= i+ blockSize)
        {
            //var bytes1 = stream.PeekBytesAt(i, blockSize);
            // Dir.Logger.LogInformation($"Type 1 :{i:X2}: "+Utilities.DumpHexWithAscii(target, i, blockSize,false));
            var sb = LogType1Block(target, i);
            Dir.Logger.LogInformation("" + sb);
            //var forecolorColor = target[i + 4];
            //var backgroundcolor = target[i + 5];
            //var endSprite = BinaryPrimitives.ReadUInt16BigEndian(target.AsSpan(i + 14)); 
            //var startSprite = BinaryPrimitives.ReadUInt16BigEndian(target.AsSpan(i + 16));
            //ushort rotationRaw = BinaryPrimitives.ReadUInt16BigEndian(target.AsSpan(i + 18));
            //ushort skewRaw = BinaryPrimitives.ReadUInt16BigEndian(target.AsSpan(i + 20));
            //sb.AppendLine($"  Rotation Raw: 0x{rotationRaw:X4} ({rotationRaw / 256.0:F2} approx)"); // Example: fixed point 8.8
            //sb.AppendLine($"  Skew Raw: 0x{skewRaw:X4} ({skewRaw / 256.0:F2} approx)");
            //ushort ink = BinaryPrimitives.ReadUInt16BigEndian(target.AsSpan(i + 22));
            //ushort blend = BinaryPrimitives.ReadUInt16BigEndian(target.AsSpan(i + 24));

        }

        blockSize = 32+ 12;
        start = 0x2B8;
        for (int i = start; i < 0x380; i= i+ blockSize)
        {
            //var bytes1 = stream.PeekBytesAt(i, blockSize);
             Dir.Logger.LogInformation($"Type 2 :{i:X2}: "+Utilities.DumpHexWithAscii(target, i, blockSize, false));
        }
        return;

        #region Old
        MemHandleSize = stream.ReadInt32();
        HeaderType = stream.ReadInt32();
        OffsetsOffset = stream.ReadInt32();     // Number of Int16 frame definitions to read
        OffsetsCount = stream.ReadInt32();      // Possibly count of something else, validate
        NotationBase = stream.ReadInt32();
        NotationSize = stream.ReadInt32();

        // Read OffsetsOffset number of Int16 frame definitions first
        for (int i = 0; i < OffsetsOffset; i++)
        {
            short val = stream.ReadInt16();
            // Optionally store or skip these frame definitions raw data
        }

        // Now read these fields (after skipping the frame definitions):
        FramesEndOffset = stream.ReadInt32();
        UnknownUint32 = stream.ReadUint32();
        FramesCount = stream.ReadInt32();

        FramesType = stream.ReadInt16();
        ChannelSize = stream.ReadInt16();
        LastChannelMax = stream.ReadInt16();
        LastChannel = stream.ReadInt16();

        // DEBUG
        Dir?.Logger.LogInformation($"ScoreChunk: memHandleSize={MemHandleSize}, headerType={HeaderType}, offsetsOffset={OffsetsOffset}, offsetsCount={OffsetsCount}, notationBase={NotationBase}, notationSize={NotationSize}");
        Dir?.Logger.LogInformation($"ScoreChunk: framesCount={FramesCount}, framesType={FramesType}, channelSize={ChannelSize}, lastChannelMax={LastChannelMax}, lastChannel={LastChannel}");

        // Now read the frames and property tables (variable length per frame)
        for (int i = 0; i < FramesCount; i++)
        {
            short frameEnd = stream.ReadInt16();

            // Read frameEnd * 3 Int16 values per frame (frame definitions)
            for (int f = 0; f < frameEnd; f++)
            {
                stream.ReadInt16(); // skip / store
                stream.ReadInt16();
                stream.ReadInt16();
            }
            var raw = stream.PeekBytesAt(stream.Position, 4);
            Dir?.Logger.LogInformation($"Raw bytes at propOffsets read: {BitConverter.ToString(raw)}");

            // Before reading property offsets count:
            short propOffsets = stream.ReadInt16();  // Read as 16-bit big endian
            if (propOffsets < 0) propOffsets = 0;    // Safety check

            // Log propOffsets and remaining bytes
            long bytesLeft = stream.Size - stream.Pos;
            if (propOffsets * 4 > bytesLeft)
            {
                Dir?.Logger.LogWarning($"propOffsets {propOffsets} too large for remaining bytes {bytesLeft}. Adjusting to 0.");
                propOffsets = 0;
            }

            // Read propOffsets number of Int32 values (property offsets)
            for (int s = 0; s < propOffsets; s++)
            {
                int propOffsetVal = stream.ReadInt32();
                // Process or store property offset val
            }

        }

        // Finally, read frame interval descriptors
        for (int i = 0; i < FramesCount; i++)
        {
            var desc = new FrameDescriptor();
            desc.StartFrame = stream.ReadInt32();
            desc.EndFrame = stream.ReadInt32();
            stream.Skip(8); // skip 8 bytes
            desc.Channel = stream.ReadInt32();
            desc.SpriteNumber = stream.ReadInt32();
            stream.Skip(28); // skip 28 bytes

            Frames.Add(desc);
        }

        Dir?.Logger.LogInformation($"ScoreChunk: Parsed {Frames.Count} frame descriptors."); 
        #endregion
    }
    /*
    var fp = new FrameProperties();
    fp.PosY = stream.ReadInt16();
    fp.PosX = stream.ReadInt16();
    fp.BeginFrame = stream.ReadInt16();
    fp.EndFrame = stream.ReadInt16();
    fp.ForeColor = stream.ReadUint8();
    fp.BackColor = stream.ReadUint8();
    fp.InkEffect = stream.ReadUint8();
    fp.ScaleX = stream.ReadUint8();
    fp.ScaleY = stream.ReadUint8();
    fp.Rotation = stream.ReadUint8();
    fp.SkewX = stream.ReadUint8();
    fp.SkewY = stream.ReadUint8();
    */

    public string LogType1Block(byte[] target, int offset)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Block at offset 0x{offset:X}");

        // Basic sanity check: make sure offset + expected size fits inside target
        if (offset + 32 > target.Length)
        {
            sb.AppendLine("  ERROR: Not enough bytes for full block.");
            return sb.ToString();
        }

        // Foreground and Background color - single bytes
        byte foreColor = target[offset + 4];
        byte backColor = target[offset + 5];

        sb.AppendLine($"  Foreground Color: {foreColor}");
        sb.AppendLine($"  Background Color: {backColor}");

        ushort numberOfKeyFrames = BinaryPrimitives.ReadUInt16BigEndian(target.AsSpan(offset + 6));
        sb.AppendLine($"    numbers A: {number1}, {number2}, {number3}, {number4}");
        ushort number2 = BinaryPrimitives.ReadUInt16BigEndian(target.AsSpan(offset + 8));
        ushort number3 = BinaryPrimitives.ReadUInt16BigEndian(target.AsSpan(offset + 10));
        ushort number4 = BinaryPrimitives.ReadUInt16BigEndian(target.AsSpan(offset + 12));
        sb.AppendLine($"    numbers A: {number2}, {number3}, {number4}");

        var locH = BinaryPrimitives.ReadUInt16BigEndian(target.AsSpan(offset + 14));
        var locV = BinaryPrimitives.ReadUInt16BigEndian(target.AsSpan(offset + 16));
        sb.AppendLine($"  Position Color: {locH}x{locV}");

        ushort number5 = BinaryPrimitives.ReadUInt16BigEndian(target.AsSpan(offset + 18));
        ushort number6 = BinaryPrimitives.ReadUInt16BigEndian(target.AsSpan(offset + 20));
        ushort number7 = BinaryPrimitives.ReadUInt16BigEndian(target.AsSpan(offset + 22));
        ushort number8 = BinaryPrimitives.ReadUInt16BigEndian(target.AsSpan(offset + 24));
        sb.AppendLine($"    numbers B: {number5}, {number6}, {number7}, {number8}");

        ushort number12 = BinaryPrimitives.ReadUInt16BigEndian(target.AsSpan(offset + 26));
        ushort number13 = BinaryPrimitives.ReadUInt16BigEndian(target.AsSpan(offset + 28));
        ushort number14 = BinaryPrimitives.ReadUInt16BigEndian(target.AsSpan(offset + 30));
        sb.AppendLine($"    numbers C: {number12}, {number13}, {number14}");
        ushort rotation = BinaryPrimitives.ReadUInt16BigEndian(target.AsSpan(offset + 32));
        sb.AppendLine($"  Rotation: {rotation/100f}");
        ushort number16 = BinaryPrimitives.ReadUInt16BigEndian(target.AsSpan(offset + 34));
        sb.AppendLine($"    number16: {number16}");
        ushort skew = BinaryPrimitives.ReadUInt16BigEndian(target.AsSpan(offset + 36));
        sb.AppendLine($"  Skew: {skew / 100f}");
        ushort number18 = BinaryPrimitives.ReadUInt16BigEndian(target.AsSpan(offset + 38));
        ushort number19 = BinaryPrimitives.ReadUInt16BigEndian(target.AsSpan(offset + 40));
        ushort number20 = BinaryPrimitives.ReadUInt16BigEndian(target.AsSpan(offset + 42));
        ushort number21 = BinaryPrimitives.ReadUInt16BigEndian(target.AsSpan(offset + 44));
        sb.AppendLine($"    numbers D: {number18}, {number19}, {number20}, {number21}");

        return sb.ToString();
    }
    private byte[] CombineBytes(params byte[][] arrays)
    {
        int length = 0;
        foreach (var a in arrays)
            length += a.Length;

        byte[] result = new byte[length];
        int offset = 0;
        foreach (var a in arrays)
        {
            Buffer.BlockCopy(a, 0, result, offset, a.Length);
            offset += a.Length;
        }
        return result;
    }






    /// <summary>
    /// Write the parsed frame descriptors to JSON so callers can inspect the
    /// sprite timeline. Only the simplified list of start/end frames is
    /// exported.
    /// </summary>
    public override void WriteJSON(JSONWriter json)
    {
        json.StartObject();
        json.WriteKey("frames");
        json.StartArray();
        foreach (var f in Frames)
        {
            json.StartObject();
            json.WriteField("start", f.StartFrame);
            json.WriteField("end", f.EndFrame);
            json.WriteField("channel", f.Channel);
            json.WriteField("sprite", f.SpriteNumber);
            json.EndObject();
        }
        json.EndArray();
        json.EndObject();
    }
}
