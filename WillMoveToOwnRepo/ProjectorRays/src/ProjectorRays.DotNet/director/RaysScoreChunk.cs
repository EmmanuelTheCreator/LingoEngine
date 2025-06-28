using Microsoft.Extensions.Logging;
using ProjectorRays.Common;
using ProjectorRays.director;
using ProjectorRays.director.Chunks;
using System;
using System.ComponentModel.DataAnnotations;

namespace ProjectorRays.Director;

/// <summary>
/// Represents the Score (VWSC) timeline information. Only the frame interval
/// descriptors are parsed which provide the start and end frame for each sprite.
/// </summary>
public class RaysScoreChunk : RaysChunk
{
    /// <summary>Descriptor of a sprite on the score timeline.</summary>
    public class FrameDescriptor
    {
        public int StartFrame;
        public int EndFrame;
        public int Channel;
        public int SpriteNumber;
        /// <summary>Cast member displayed by this sprite.</summary>
        public int DisplayMember;
        /// <summary>Offset to sprite property table for behaviors.</summary>
        public int SpritePropertiesOffset;
        public int LocH;
        public int LocV;
        public int Width;
        public int Height;
        public float Rotation;
        public float Skew;
        public int Ink;
        public int ForeColor;
        public int BackColor;
        public int ScoreColor;
        public int Blend;
        public bool FlipH;
        public bool FlipV;
        public bool Editable;

        public int Unknown1 { get; internal set; }
        public int Unknown2 { get; internal set; }
        public int Unknown3 { get; internal set; }
        public int Unknown4 { get; internal set; }
        public int Unknown5 { get; internal set; }
        public int Unknown6 { get; internal set; }
    }

    /// <summary>Default sprite data for a channel.</summary>
    public class ChannelSprite
    {
        /// <summary>Cast member displayed when no frame overrides.</summary>
        public int DisplayMember;
        /// <summary>Offset to the property descriptor list for behaviors.</summary>
        public int SpritePropertiesOffset;
        public int LocH;
        public int LocV;
        public int Width;
        public int Height;
        public float Rotation;
        public float Skew;
        public int Ink;
        public int ForeColor;
        public int BackColor;
        public int ScoreColor;
        public int Blend;
        public bool FlipH;
        public bool FlipV;
        public bool Editable;
    }

    /// <summary>List of parsed frame descriptors.</summary>
    public List<FrameDescriptor> Frames { get; } = new();
    /// <summary>Default sprite geometry for each channel.</summary>
    public List<ChannelSprite> ChannelSprites { get; } = new();

    private static void ApplyDefaults(FrameDescriptor desc, ChannelSprite s)
    {
        desc.DisplayMember = s.DisplayMember;
        desc.SpritePropertiesOffset = s.SpritePropertiesOffset;
        desc.LocH = s.LocH;
        desc.LocV = s.LocV;
        desc.Width = s.Width;
        desc.Height = s.Height;
        desc.Rotation = s.Rotation;
        desc.Skew = s.Skew;
        desc.Ink = s.Ink;
        desc.ForeColor = s.ForeColor;
        desc.BackColor = s.BackColor;
        desc.ScoreColor = s.ScoreColor;
        desc.Blend = s.Blend;
        desc.FlipH = s.FlipH;
        desc.FlipV = s.FlipV;
        desc.Editable = s.Editable;
    }

  

    public RaysScoreChunk(RaysDirectorFile? dir) : base(dir, ChunkType.ScoreChunk) { }

    /// <summary>
    /// Parse the VWSC chunk. This is a simplified reader that mirrors the
    /// structure used by ScummVM. Only the frame interval table is loaded.
    /// </summary>
    public override void Read(ReadStream stream)
    {


        // The VWSC score chunk is always stored in big endian regardless of the
        // overall movie endianness.
        stream.Endianness = Endianness.BigEndian;

        //var bytes22 = stream.ReadBytes(stream.Size);
        //var bytes3raw = Utilities.LogHex(bytes22, bytes22.Length);



        ////var bytes = stream.LogHex(stream.Size);
        //stream.Endianness = Endianness.BigEndian;
        //var target = new byte[stream.Data.Length - 0x7580];
        ////Array.Copy(stream.Data, 12170, target, 0, target.Length);
        //Array.Copy(stream.Data, 0x7580, target, 0, target.Length);
        //var bytes2raw = Utilities.DumpHexWithAscii(target, 0, target.Length, false);



        int memHandleSize = stream.ReadInt32();
        int headerType = stream.ReadInt32();
        int offsetsOffset = stream.ReadInt32();
        int offsetsCount = stream.ReadInt32();
        int notationBase = stream.ReadInt32();
        int notationSize = stream.ReadInt32();
        Dir?.Logger.LogInformation($"headerType={headerType},offsetsOffset={offsetsOffset},offsetsCount={offsetsCount},notationBase={notationBase},notationSize={notationSize}");
        //for (int i = 0; i < offsetsOffset; i++)
        //    stream.ReadInt16();

        //int framesEndOffset = stream.ReadInt32();
        //stream.ReadUint32();
        //int framesCount = stream.ReadInt32();
        //short framesType = stream.ReadInt16();
        //short channelSize = stream.ReadInt16();
        //short lastChannelMax = stream.ReadInt16();
        //short lastChannel = stream.ReadInt16();

        //// Skip frame definitions and property tables. Each frame entry
        //// consists of size/offset records referencing sprite property tables
        //// which we currently ignore.
        //for (int i = 0; i < framesCount; i++)
        //{
        //    short frameEnd = stream.ReadInt16();
        //    for (int f = 0; f < frameEnd; f++)
        //    {
        //        short size = stream.ReadInt16();
        //        short offset = stream.ReadInt16();
        //        short buffer = stream.ReadInt16();
        //        _ = size; _ = offset; _ = buffer; // undocumented
        //    }
        //    int propOffsets = stream.ReadInt32();
        //    for (int s = 0; s < propOffsets; s++)
        //        stream.ReadInt32();
        //}
        //var framesCount = 165;
        //var spriteCount = 149;

        List<int> numbers;
        short framesCount;

        // Header
        HeaderNumbers(stream, out numbers, out framesCount);
        Dir?.Logger.LogInformation(string.Join(" ", numbers.Select(n => n.ToString().PadLeft(5))));

        var sprite = ReadChannelSprite(stream);
        var bloks = new List<List<int>>();

        ChannelSprites.Add(sprite);
        bloks.Add(ReadDataBlock(1, stream));
        ChannelSprites.Add(ReadChannelSprite(stream));
        bloks.Add(ReadDataBlock(2, stream));
        ChannelSprites.Add(ReadChannelSprite(stream));
        bloks.Add(ReadDataBlock(3, stream));
        bloks.Add(ReadDataBlock2(3, stream));

        ChannelSprites.Add(ReadChannelSprite(stream));
        bloks.Add(ReadDataBlock3(4, stream));
        ChannelSprites.Add(ReadChannelSprite(stream));

        foreach (var row in bloks)
        {
            string line = string.Join(" ", row.Select(n => n.ToString().PadLeft(5)));
            Dir?.Logger.LogInformation(line);
        }

        // splitter in between
        var numbersSplit = SplitterNumbers(stream);
        Dir?.Logger.LogInformation(string.Join(" ", numbersSplit.Select(n => n.ToString().PadLeft(5))));



        //var bytes230 = stream.ReadBytes(stream.Size - stream.Pos);
        //var bytes3raw = Utilities.LogHex(bytes230, bytes230.Length,50);


        framesCount = ReadFrameIntervalDescriptor(stream);

        //// Channel default sprite data
        //for (int i = 0; i < lastChannelMax; i++)
        //{
        //    ChannelSprites.Add(ReadChannelSprite(stream));
        //}

        // Apply default geometry to frame descriptors
        //foreach (var desc in Frames)
        //{
        //    int idx = desc.Channel;
        //    if (idx >= 0 && idx < ChannelSprites.Count)
        //    {
        //        ApplyDefaults(desc, ChannelSprites[idx]);
        //    }
        //    else if (idx - 1 >= 0 && idx - 1 < ChannelSprites.Count)
        //    {
        //        ApplyDefaults(desc, ChannelSprites[idx - 1]);
        //    }
        //}
    }
    // 3C = 60
    // 68 = 104
    // 94 = 148
    // C4 = 196

    private ChannelSprite ReadChannelSprite(ReadStream stream)
    {
        WriteAddress(stream);
        var sprite = new ChannelSprite();
        var val1 = stream.ReadUint8();
        var val2 = stream.ReadUint8();
        var val3 = stream.ReadUint8();// flags, unused
        byte inkByte = stream.ReadUint8();
        sprite.Ink = inkByte & 0x7F;
        sprite.ForeColor = stream.ReadUint8();
        sprite.BackColor = stream.ReadUint8();
        sprite.DisplayMember = (int)stream.ReadUint32();
        stream.Skip(2); // unknown
        sprite.SpritePropertiesOffset = stream.ReadUint16();
        sprite.LocV = stream.ReadInt16();
        sprite.LocH = stream.ReadInt16();
        sprite.Height = stream.ReadInt16();
        sprite.Width = stream.ReadInt16();
        byte colorcode = stream.ReadUint8();
        sprite.Editable = (colorcode & 0x40) != 0;
        sprite.ScoreColor = colorcode & 0x0F;
        var blend = stream.ReadUint8();
        sprite.Blend = (int)Math.Round(100f - (blend / 255f) * 100f);
        byte flag2 = stream.ReadUint8();
        sprite.FlipV = (flag2 & 0x04) != 0;
        sprite.FlipH = (flag2 & 0x02) != 0;
        stream.Skip(5);
        //var test = stream.ReadInt16();
        sprite.Rotation = stream.ReadUint32() / 100f;
        sprite.Skew = stream.ReadUint32() / 100f;
        stream.Skip(12);
        Dir?.Logger.LogInformation($"{sprite.LocH}x{sprite.LocV}:Ink={sprite.Ink}:Blend={sprite.Blend}:Skew={sprite.Skew}:Rot={sprite.Rotation}:PropOffset={sprite.SpritePropertiesOffset}:Member={sprite.DisplayMember}");
        return sprite;
    }

    private int lastOffset;
    private short ReadFrameIntervalDescriptor(ReadStream stream)
    {
        short framesCount = 5;
        //Frame interval descriptors
        for (int i = 0; i < framesCount; i++)
        {
            if (i == 3) // some strange bug, because one key frame more, one value more of '2'
            {
                var numberOfKeyFrames = stream.ReadInt32();// ??? is this correct?
            }
            WriteAddress(stream);
            var desc = new FrameDescriptor();
            desc.StartFrame = stream.ReadInt32();
            desc.EndFrame = stream.ReadInt32();
            desc.Unknown1 = stream.ReadInt32();
            desc.Unknown2 = stream.ReadInt32();
            desc.Channel = stream.ReadInt32(); // after top channels , so start at 6 if 2 audio channels
            desc.Unknown3 = stream.ReadInt16(); // seems always 1
            desc.Unknown4 = stream.ReadInt32();  // always 0F
            desc.Unknown5 = stream.ReadUint8();  // always E1
            desc.Unknown6 = stream.ReadUint8();  // always FD
            //desc.SpriteNumber = stream.ReadInt32(); <- not found
            stream.Skip(16);
            Frames.Add(desc);
            Dir?.Logger.LogInformation($"Frame {i}: Start={desc.StartFrame}, End={desc.EndFrame}, Channel={desc.Channel}, U1={desc.Unknown1}, U2={desc.Unknown2}, U3={desc.Unknown3}, U4={desc.Unknown4}, U5={desc.Unknown5}, U6={desc.Unknown6}");

        }

        return framesCount;
    }

    private void WriteAddress(ReadStream stream)
    {
        Dir?.Logger.LogInformation($"Address:{stream.Pos:X2} {stream.Pos} , offset={stream.Pos-lastOffset}");
        lastOffset = stream.Pos;
    }

    private static List<int> SplitterNumbers(ReadStream stream)
    {
        // 00 08 00 02 01 F6 91 00 00 02
        var numbers2 = new List<int>();
        numbers2.Add(stream.ReadInt16());
        numbers2.Add(stream.ReadInt16());
        numbers2.Add(stream.ReadInt8());
        numbers2.Add(stream.ReadInt8());
        numbers2.Add(stream.ReadInt8());
        numbers2.Add(stream.ReadInt8());
        numbers2.Add(stream.ReadInt16());
        // 00 00 00 00 00 05 00 00 00 12
        numbers2.Add(stream.ReadInt16());
        numbers2.Add(stream.ReadInt32());
        numbers2.Add(stream.ReadInt32());
        // 00 00 00 1B 00 00 00 1E 00 00 00 21 00 00 00 24 
        numbers2.Add(stream.ReadInt32());
        numbers2.Add(stream.ReadInt32());
        numbers2.Add(stream.ReadInt32());
        numbers2.Add(stream.ReadInt32());
        return numbers2;
    }

    private static void HeaderNumbers(ReadStream stream, out List<int> numbers, out short framesCount)
    {
        numbers = new List<int>();
        for (int i = 0; i < 10 * 3; i++)
        {
            numbers.Add(stream.ReadInt32());
        }
        // 00 00 00 0F 00 0D 00 30 03 EE 00 96 00 36 00 30 
        numbers.Add(stream.ReadInt32());
        numbers.Add(stream.ReadInt16());
        numbers.Add(stream.ReadInt16());

        numbers.Add(stream.ReadInt16());
        framesCount = stream.ReadInt16();
        numbers.Add(stream.ReadInt16());
        numbers.Add(stream.ReadInt16());
    }

    private List<int> ReadDataBlock(int index, ReadStream stream)
    {
        var numbers = new List<int>
        {
            stream.ReadInt16(),
            stream.ReadInt16(),
            stream.ReadInt8(),
            stream.ReadInt8(),
            stream.ReadInt8(),
            stream.ReadInt8()
        };
        for (int i = 0; i < 27; i++)
        {
            numbers.Add(stream.ReadInt16());
        }
        //var values = string.Join(',', numbers);
        return numbers;

    }
    private List<int> ReadDataBlock2(int index, ReadStream stream)
    {
        // 16
        var numbers = new List<int>
        {
            stream.ReadInt16(),
            stream.ReadInt16(),
            stream.ReadInt16(),
            stream.ReadInt16(),

            stream.ReadInt16(),
            stream.ReadInt16(),
            stream.ReadInt16(),
            stream.ReadInt16(),

            stream.ReadInt16(),
            stream.ReadInt16()
        };
        
        return numbers;

    }
    private List<int> ReadDataBlock3(int index, ReadStream stream)
    {
        var numbers = new List<int>
        {
            stream.ReadInt16(),
            stream.ReadInt16(),
            stream.ReadInt8(),
            stream.ReadInt8(),
            stream.ReadInt8(),
            stream.ReadInt8()
        };
        for (int i = 0; i < 6; i++)
        {
            numbers.Add(stream.ReadInt32());
            numbers.Add(stream.ReadInt32());
        }
        //var values = string.Join(',', numbers);
        return numbers;

    }
    private void ReadKeyFrame(ReadStream stream)
    {
        var numbers = new List<int>();
    }


    

    /// <summary>
    /// Write the parsed frame descriptors to JSON so callers can inspect the
    /// sprite timeline. Only the simplified list of start/end frames is
    /// exported.
    /// </summary>
    public override void WriteJSON(RaysJSONWriter json)
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
            json.WriteField("displayMember", f.DisplayMember);
            json.WriteField("spritePropertiesOffset", f.SpritePropertiesOffset);
            json.WriteField("locH", f.LocH);
            json.WriteField("locV", f.LocV);
            json.WriteField("width", f.Width);
            json.WriteField("height", f.Height);
            json.WriteField("rotation", f.Rotation);
            json.WriteField("skew", f.Skew);
            json.WriteField("ink", f.Ink);
            json.WriteField("foreColor", f.ForeColor);
            json.WriteField("backColor", f.BackColor);
            json.WriteField("scoreColor", f.ScoreColor);
            json.WriteField("blend", f.Blend);
            json.WriteField("flipH", f.FlipH.ToString());
            json.WriteField("flipV", f.FlipV.ToString());
            json.WriteField("editable", f.Editable.ToString());
            json.EndObject();
        }
        json.EndArray();
        json.EndObject();
    }
}