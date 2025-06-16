using System.Collections.Generic;
using ProjectorRays.Common;

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
    }

    /// <summary>List of parsed frame descriptors.</summary>
    public List<FrameDescriptor> Frames { get; } = new();

    public ScoreChunk(DirectorFile? dir) : base(dir, ChunkType.ScoreChunk) {}

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

        // Skip frame definitions and property tables.
        for (int i = 0; i < framesCount; i++)
        {
            short frameEnd = stream.ReadInt16();
            for (int f = 0; f < frameEnd; f++)
            {
                stream.ReadInt16();
                stream.ReadInt16();
                stream.ReadInt16();
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
