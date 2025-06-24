using ProjectorRays.Common;
using ProjectorRays.director.Chunks;

namespace ProjectorRays.Director;

/// <summary>
/// Represents the Score (VWSC) timeline information. Only the frame interval
/// descriptors are parsed which provide the start and end frame for each sprite.
/// </summary>
public class ScoreChunk : Chunk
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

    private ChannelSprite ReadChannelSprite(ReadStream stream)
    {
        var sprite = new ChannelSprite();
        _ = stream.ReadUint8(); // flags, unused
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
        sprite.Blend = stream.ReadUint8();
        byte flag2 = stream.ReadUint8();
        sprite.FlipV = (flag2 & 0x04) != 0;
        sprite.FlipH = (flag2 & 0x02) != 0;
        stream.Skip(5);
        sprite.Rotation = stream.ReadFloat32();
        sprite.Skew = stream.ReadFloat32();
        stream.Skip(12);
        return sprite;
    }

    public ScoreChunk(DirectorFile? dir) : base(dir, ChunkType.ScoreChunk) { }

    /// <summary>
    /// Parse the VWSC chunk. This is a simplified reader that mirrors the
    /// structure used by ScummVM. Only the frame interval table is loaded.
    /// </summary>
    public override void Read(ReadStream stream)
    {
        // The VWSC score chunk is always stored in big endian regardless of the
        // overall movie endianness.
        stream.Endianness = Endianness.BigEndian;

        int memHandleSize = stream.ReadInt32();
        int headerType = stream.ReadInt32();
        int offsetsOffset = stream.ReadInt32();
        int offsetsCount = stream.ReadInt32();
        int notationBase = stream.ReadInt32();
        int notationSize = stream.ReadInt32();

        for (int i = 0; i < offsetsOffset; i++)
            stream.ReadInt16();

        int framesEndOffset = stream.ReadInt32();
        stream.ReadUint32();
        int framesCount = stream.ReadInt32();
        short framesType = stream.ReadInt16();
        short channelSize = stream.ReadInt16();
        short lastChannelMax = stream.ReadInt16();
        short lastChannel = stream.ReadInt16();

        // Skip frame definitions and property tables. Each frame entry
        // consists of size/offset records referencing sprite property tables
        // which we currently ignore.
        for (int i = 0; i < framesCount; i++)
        {
            short frameEnd = stream.ReadInt16();
            for (int f = 0; f < frameEnd; f++)
            {
                short size = stream.ReadInt16();
                short offset = stream.ReadInt16();
                short buffer = stream.ReadInt16();
                _ = size; _ = offset; _ = buffer; // undocumented
            }
            int propOffsets = stream.ReadInt32();
            for (int s = 0; s < propOffsets; s++)
                stream.ReadInt32();
        }


        // Frame interval descriptors
        for (int i = 0; i < framesCount; i++)
        {
            var desc = new FrameDescriptor();
            desc.StartFrame = stream.ReadInt32();
            desc.EndFrame = stream.ReadInt32();
            stream.Skip(8);
            desc.Channel = stream.ReadInt32();
            desc.SpriteNumber = stream.ReadInt32();
            stream.Skip(28);
            Frames.Add(desc);
        }

        // Channel default sprite data
        for (int i = 0; i < lastChannelMax; i++)
        {
            ChannelSprites.Add(ReadChannelSprite(stream));
        }

        // Apply default geometry to frame descriptors
        foreach (var desc in Frames)
        {
            int idx = desc.Channel;
            if (idx >= 0 && idx < ChannelSprites.Count)
            {
                ApplyDefaults(desc, ChannelSprites[idx]);
            }
            else if (idx - 1 >= 0 && idx - 1 < ChannelSprites.Count)
            {
                ApplyDefaults(desc, ChannelSprites[idx - 1]);
            }
        }
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
            json.WriteField("flipH", f.FlipH);
            json.WriteField("flipV", f.FlipV);
            json.WriteField("editable", f.Editable);
            json.EndObject();
        }
        json.EndArray();
        json.EndObject();
    }
}