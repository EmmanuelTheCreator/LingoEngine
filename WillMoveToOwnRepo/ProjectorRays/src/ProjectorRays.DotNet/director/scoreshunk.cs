using System.Collections.Generic;
using ProjectorRays.Common;
using ProjectorRays.director.Chunks;

namespace ProjectorRays.Director;

/// <summary>
/// Experimental parser for the VWSC score chunk based on the documentation
/// in Z_Analysis/Director_FileFormat/section-VWSC.md. This implementation
/// focuses on reading the header and frame interval descriptor table.
/// It is intentionally simplified and only exposes the start/end frame,
/// channel and sprite number for each interval.
/// </summary>
public class ScoreShunk : Chunk
{
    /// <summary>Simple representation of a frame interval descriptor.</summary>
    public class IntervalDescriptor
    {
        public int StartFrame;
        public int EndFrame;
        // The original parser exposed a Channel field here which we no longer
        // use. The specification does not make it clear whether this value is
        // meaningful, so it is kept only for reference.
        // public int Channel;

        public int Unknown0; // constant 0
        public int Unknown1; // constant 0
        public int SpriteNumber;
        public ushort Unknown2; // constant 1
        public int Unknown3;    // near constant 15/0
        public ushort Unknown4; // near constant 57853/24973
        public int Unknown5;    // constant 0
        public int Unknown6;    // constant 0
        public int Unknown7;    // constant 0
        public int Unknown8;    // constant 0
        public List<int> ExtraValues { get; } = new();

        /// <summary>Behaviour scripts attached to this interval.</summary>
        public List<BehaviourRef> Behaviours { get; } = new();
    }

    /// <summary>Reference to a frame behaviour script.</summary>
    public class BehaviourRef
    {
        public int CastLib;
        public int CastMmb;
    }

    /// <summary>Frame order and additional metadata parsed from the chunk.</summary>
    public List<int> IntervalOrder { get; } = new();


    /// <summary>Total number of sprite channels (usually +6 compared to the raw header field).</summary>
    public int SpriteCount;

    public int FrameCount;
    public int ChannelCount;
    public int SpriteByteSize;
    // Additional header fields kept for completeness
    public int FrameDataActualLength;
    public int FrameDataHeaderSize;
    public short Constant13;
    public short LastChannelMinus6;

    /// <summary>Raw decoded frame data per frame.</summary>
    public List<byte[]> FrameData { get; } = new();

    /// <summary>List of parsed interval descriptors.</summary>
    public List<IntervalDescriptor> Intervals { get; } = new();

    /// <summary>List of behaviour script lists in the same order as the
    /// interval table.</summary>
    public List<List<BehaviourRef>> FrameScripts { get; } = new();

    public ScoreShunk(DirectorFile? dir) : base(dir, ChunkType.ScoreChunk) { }

    public override void Read(ReadStream stream)
    {
        // VWSC data is stored big endian regardless of overall movie endianness
        stream.Endianness = Endianness.BigEndian;

        int totalLength = stream.ReadInt32();
        int constantMinus3 = stream.ReadInt32();
        int constant12 = stream.ReadInt32();
        int entryCount = stream.ReadInt32();
        int entryCountPlus1 = stream.ReadInt32();
        int entrySizeSum = stream.ReadInt32();
        _ = totalLength; _ = constantMinus3; _ = constant12;
        _ = entrySizeSum; _ = entryCountPlus1;

        // Offsets from the start of the entries area
        int[] offsets = new int[entryCount + 1];
        for (int i = 0; i < offsets.Length; i++)
            offsets[i] = stream.ReadInt32();

        int entriesStart = stream.Pos;

        // Parse framedata header and decode the delta encoded frames
        if (entryCount >= 1)
        {
            var fdView = new BufferView(stream.Data,
                entriesStart + offsets[0], offsets[1] - offsets[0]);
            ReadFrameData(fdView);
        }

        // Interval order table
        if (entryCount >= 2)
        {
            var orderView = new BufferView(stream.Data,
                entriesStart + offsets[1], offsets[2] - offsets[1]);
            var os = new ReadStream(orderView, Endianness.BigEndian);
            if (os.Size >= 4)
            {
                int count = os.ReadInt32();
                for (int i = 0; i < count && os.Pos + 4 <= os.Size; i++)
                    IntervalOrder.Add(os.ReadInt32());
            }
        }

        // Beginning at entry[3] the entries form triples that describe the
        // frame interval descriptors. Parse only the entries referenced by the
        // interval order list when available.
        var entryIndices = IntervalOrder.Count > 0 ? IntervalOrder : null;
        if (entryIndices == null)
        {
            entryIndices = new List<int>();
            for (int i = 3; i + 2 < offsets.Length; i += 3)
                entryIndices.Add(i);
        }

        foreach (int primaryIdx in entryIndices)
        {
            if (primaryIdx + 2 >= offsets.Length)
                continue;

            var ps = new ReadStream(new BufferView(stream.Data,
                    entriesStart + offsets[primaryIdx], offsets[primaryIdx + 1] - offsets[primaryIdx]),
                Endianness.BigEndian);

            var d = new IntervalDescriptor();
            if (ps.Size >= 44)
            {
                d.StartFrame = ps.ReadInt32();
                d.EndFrame = ps.ReadInt32();
                d.Unknown0 = ps.ReadInt32();
                d.Unknown1 = ps.ReadInt32();
                d.SpriteNumber = ps.ReadInt32();
                d.Unknown2 = ps.ReadUint16();
                d.Unknown3 = ps.ReadInt32();
                d.Unknown4 = ps.ReadUint16();
                d.Unknown5 = ps.ReadInt32();
                d.Unknown6 = ps.ReadInt32();
                d.Unknown7 = ps.ReadInt32();
                d.Unknown8 = ps.ReadInt32();
                while (ps.Pos + 4 <= ps.Size)
                    d.ExtraValues.Add(ps.ReadInt32());
            }
            else
            {
                // not enough bytes, skip
                continue;
            }

            Intervals.Add(d);

            // Secondary bytestring lists behaviour scripts
            var secView = new BufferView(stream.Data,
                entriesStart + offsets[primaryIdx + 1], offsets[primaryIdx + 2] - offsets[primaryIdx + 1]);
            var ss = new ReadStream(secView, Endianness.BigEndian);
            var behaviours = new List<BehaviourRef>();
            while (ss.Pos + 8 <= ss.Size)
            {
                short cl = ss.ReadInt16();
                short cm = ss.ReadInt16();
                ss.ReadInt32(); // constant 0
                behaviours.Add(new BehaviourRef { CastLib = cl, CastMmb = cm });
            }
            d.Behaviours.AddRange(behaviours);
            FrameScripts.Add(behaviours);
            // tertiary entry usually empty, skip
        }
    }

    private void ReadFrameData(BufferView view)
    {
        var fs = new ReadStream(view, Endianness.BigEndian);
        FrameDataActualLength = fs.ReadInt32();
        FrameDataHeaderSize = fs.ReadInt32();
        FrameCount = fs.ReadInt32();
        Constant13 = fs.ReadInt16();
        SpriteByteSize = fs.ReadInt16();
        ChannelCount = fs.ReadInt16();
        LastChannelMinus6 = fs.ReadInt16(); // multiple of 10, often 50
        SpriteCount = LastChannelMinus6 + 6;

        byte[] current = new byte[SpriteByteSize * ChannelCount];
        for (int f = 0; f < FrameCount && !fs.Eof; f++)
        {
            ushort len = fs.ReadUint16();
            int start = fs.Pos;
            int end = start + len - 2;
            while (fs.Pos < end)
            {
                ushort deltaLen = fs.ReadUint16();
                ushort offset = fs.ReadUint16();
                byte[] delta = fs.ReadBytes(deltaLen);
                Array.Copy(delta, 0, current, offset, deltaLen);
            }
            byte[] copy = new byte[current.Length];
            current.CopyTo(copy, 0);
            FrameData.Add(copy);
        }
    }
    public override void WriteJSON(JSONWriter json)
    {
        json.StartObject();
        json.WriteField("frameCount", FrameCount);
        json.WriteField("channelCount", ChannelCount);
        json.WriteField("spriteCount", SpriteCount);
        json.WriteField("frameDataActualLength", FrameDataActualLength);
        json.WriteField("frameDataHeaderSize", FrameDataHeaderSize);
        json.WriteField("const13", Constant13);
        json.WriteField("lastChannelMinus6", LastChannelMinus6);
        json.WriteKey("frameScripts");
        json.StartArray();
        foreach (var list in FrameScripts)
        {
            json.StartArray();
            foreach (var b in list)
            {
                json.StartObject();
                json.WriteField("castLib", b.CastLib);
                json.WriteField("castMmb", b.CastMmb);
                json.EndObject();
            }
            json.EndArray();
        }
        json.EndArray();
        json.WriteKey("intervals");
        json.StartArray();
        for (int i = 0; i < Intervals.Count; i++)
        {
            var d = Intervals[i];
            json.StartObject();
            json.WriteField("start", d.StartFrame);
            json.WriteField("end", d.EndFrame);
            json.WriteField("sprite", d.SpriteNumber);
            json.WriteField("unk0", d.Unknown0);
            json.WriteField("unk1", d.Unknown1);
            json.WriteField("unk2", d.Unknown2);
            json.WriteField("unk3", d.Unknown3);
            json.WriteField("unk4", d.Unknown4);
            json.WriteField("unk5", d.Unknown5);
            json.WriteField("unk6", d.Unknown6);
            json.WriteField("unk7", d.Unknown7);
            json.WriteField("unk8", d.Unknown8);
            json.WriteKey("extra");
            json.StartArray();
            foreach (var v in d.ExtraValues)
                json.WriteVal(v);
            json.EndArray();
            json.WriteKey("behaviours");
            json.StartArray();
            foreach (var b in d.Behaviours)
            {
                json.StartObject();
                json.WriteField("castLib", b.CastLib);
                json.WriteField("castMmb", b.CastMmb);
                json.EndObject();
            }
            json.EndArray();
            json.EndObject();
        }
        json.EndArray();
        json.WriteKey("intervalOrder");
        json.StartArray();
        foreach (var o in IntervalOrder)
            json.WriteVal(o);
        json.EndArray();
        json.EndObject();
    }
}
