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
public class ScoreShunk : RaysChunk
{
    public StreamAnnotatorDecorator? Annotator { get; private set; }
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

    public ScoreShunk(RaysDirectorFile? dir) : base(dir, ChunkType.ScoreChunk) { }

    public override void Read(ReadStream stream)
    {
        // VWSC data is stored big endian regardless of overall movie endianness
        stream.Endianness = Endianness.BigEndian;
        Annotator = new StreamAnnotatorDecorator(stream.Offset);

        var s = new ReadStream(new BufferView(stream.Data, stream.Offset, stream.Size),
            Endianness.BigEndian, stream.Pos, Annotator);

        int totalLength = s.ReadInt32("totalLength");
        int constantMinus3 = s.ReadInt32("constMinus3");
        int constant12 = s.ReadInt32("const12");
        int entryCount = s.ReadInt32("entryCount");
        int entryCountPlus1 = s.ReadInt32("entryCountPlus1");
        int entrySizeSum = s.ReadInt32("entrySizeSum");
        _ = totalLength; _ = constantMinus3; _ = constant12;
        _ = entrySizeSum; _ = entryCountPlus1;

        // Offsets from the start of the entries area
        int[] offsets = new int[entryCount + 1];
        for (int i = 0; i < offsets.Length; i++)
            offsets[i] = s.ReadInt32($"offset[{i}]");

        int entriesStart = s.Pos;

        // Parse framedata header and decode the delta encoded frames
        if (entryCount >= 1)
        {
            var fdView = new BufferView(s.Data,
                entriesStart + offsets[0], offsets[1] - offsets[0]);
            ReadFrameData(fdView);
        }

        // Interval order table
        if (entryCount >= 2)
        {
            var orderView = new BufferView(s.Data,
                entriesStart + offsets[1], offsets[2] - offsets[1]);
            var os = new ReadStream(orderView, Endianness.BigEndian, annotator: new StreamAnnotatorDecorator(orderView.Offset));
            if (os.Size >= 4)
            {
                int count = os.ReadInt32("orderCount");
                for (int i = 0; i < count && os.Pos + 4 <= os.Size; i++)
                    IntervalOrder.Add(os.ReadInt32("order", new() { ["index"] = i }));
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

            var psView = new BufferView(s.Data,
                    entriesStart + offsets[primaryIdx], offsets[primaryIdx + 1] - offsets[primaryIdx]);
            var ps = new ReadStream(psView, Endianness.BigEndian, annotator: new StreamAnnotatorDecorator(psView.Offset));

            var d = new IntervalDescriptor();
            if (ps.Size >= 44)
            {
                var k = new Dictionary<string, int> { ["entry"] = primaryIdx };
                d.StartFrame = ps.ReadInt32("startFrame", k);
                d.EndFrame = ps.ReadInt32("endFrame", k);
                d.Unknown0 = ps.ReadInt32("unk0", k);
                d.Unknown1 = ps.ReadInt32("unk1", k);
                d.SpriteNumber = ps.ReadInt32("sprite", k);
                d.Unknown2 = ps.ReadUint16("unk2", k);
                d.Unknown3 = ps.ReadInt32("unk3", k);
                d.Unknown4 = ps.ReadUint16("unk4", k);
                d.Unknown5 = ps.ReadInt32("unk5", k);
                d.Unknown6 = ps.ReadInt32("unk6", k);
                d.Unknown7 = ps.ReadInt32("unk7", k);
                d.Unknown8 = ps.ReadInt32("unk8", k);
                while (ps.Pos + 4 <= ps.Size)
                    d.ExtraValues.Add(ps.ReadInt32("extra", k));
            }
            else
            {
                // not enough bytes, skip
                continue;
            }

            Intervals.Add(d);

            // Secondary bytestring lists behaviour scripts
            var secView = new BufferView(s.Data,
                entriesStart + offsets[primaryIdx + 1], offsets[primaryIdx + 2] - offsets[primaryIdx + 1]);
            var ss = new ReadStream(secView, Endianness.BigEndian, annotator: new StreamAnnotatorDecorator(secView.Offset));
            var behaviours = new List<BehaviourRef>();
            while (ss.Pos + 8 <= ss.Size)
            {
                var k = new Dictionary<string, int> { ["entry"] = primaryIdx };
                short cl = ss.ReadInt16("castLib", k);
                short cm = ss.ReadInt16("castMmb", k);
                ss.ReadInt32("zero", k); // constant 0
                behaviours.Add(new BehaviourRef { CastLib = cl, CastMmb = cm });
            }
            d.Behaviours.AddRange(behaviours);
            FrameScripts.Add(behaviours);
            // tertiary entry usually empty, skip
        }
    }

    private void ReadFrameData(BufferView view)
    {
        var fs = new ReadStream(view, Endianness.BigEndian, annotator: new StreamAnnotatorDecorator(view.Offset));
        FrameDataActualLength = fs.ReadInt32("frameDataActualLength");
        FrameDataHeaderSize = fs.ReadInt32("frameDataHeaderSize");
        FrameCount = fs.ReadInt32("frameCount");
        Constant13 = fs.ReadInt16("const13");
        SpriteByteSize = fs.ReadInt16("spriteByteSize");
        ChannelCount = fs.ReadInt16("channelCount");
        LastChannelMinus6 = fs.ReadInt16("lastChannelMinus6"); // multiple of 10, often 50
        SpriteCount = LastChannelMinus6 + 6;

        byte[] current = new byte[SpriteByteSize * ChannelCount];
        for (int f = 0; f < FrameCount && !fs.Eof; f++)
        {
            var fk = new Dictionary<string, int> { ["frame"] = f };
            ushort len = fs.ReadUint16("frameLen", fk);
            int start = fs.Pos;
            int end = start + len - 2;
            while (fs.Pos < end)
            {
                ushort deltaLen = fs.ReadUint16("deltaLen", fk);
                ushort offset = fs.ReadUint16("offset", fk);
                byte[] delta = fs.ReadBytes(deltaLen, "deltaBytes", new Dictionary<string,int>{["frame"]=f, ["offset"]=offset});
                Array.Copy(delta, 0, current, offset, deltaLen);
            }
            byte[] copy = new byte[current.Length];
            current.CopyTo(copy, 0);
            FrameData.Add(copy);
        }
    }
    public override void WriteJSON(RaysJSONWriter json)
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
