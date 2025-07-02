using Microsoft.Extensions.Logging;
using ProjectorRays.Common;
using ProjectorRays.director.Chunks;
using ProjectorRays.Director;
using static ProjectorRays.director.Scores.RaysScoreFrameParser;

namespace ProjectorRays.director.Scores;

/// <summary>
/// Represents the Score (VWSC) timeline information. Only the frame interval
/// descriptors are parsed which provide the start and end frame for each sprite.
/// </summary>
public class RaysScoreChunk : RaysChunk
{

  
    /// <summary>Reference to a frame behaviour script.</summary>
    public class RaysBehaviourRef
    {
        public int CastLib {get;set;}
        public int CastMmb { get; set; }
    }

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


    public class RayKeyFrame
    {
        public int LocH { get; set; }
        public int LocV { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public float Rotation { get; set; }
        public float Skew { get; set; }
        public int Blend { get; set; }
        public int Ink { get; set; }
        public int ForeColor { get; set; }
        public int BackColor { get; set; }
        public int MemberCastLib { get; set; }
        public int MemberNum { get; set; }


    }
    /// <summary>Descriptor of a sprite on the score timeline.</summary>
    public class RaySprite
    {
        /// <summary>Cast member displayed by this sprite.</summary>
        /// <summary>Offset to sprite property table for behaviors.</summary>
        public int StartFrame {get; internal set; }
        public int EndFrame {get; internal set; }
        public int SpriteNumber {get; internal set; }
        public int DisplayMember {get; internal set; }
        public int SpritePropertiesOffset {get; internal set; }
        public int LocH {get; internal set; }
        public int LocV {get; internal set; }
        public int Width {get; internal set; }
        public int Height {get; internal set; }
        public float Rotation {get; internal set; }
        public float Skew {get; internal set; }
        public int Ink {get; internal set; }
        public int ForeColor {get; internal set; }
        public int BackColor {get; internal set; }
        public int ScoreColor {get; internal set; }
        public int Blend {get; internal set; }
        public bool FlipH {get; internal set; }
        public bool FlipV {get; internal set; }
        public bool Editable { get; internal set; }

        public int MemberCastLib { get; set; }
        public int MemberNum { get; set; }

        public List<int> ExtraValues { get; internal set; } = new();
        public List<RaysBehaviourRef> Behaviors { get; internal set; } = new();
        public List<RayKeyFrame> Keyframes { get; internal set; } = new();
    }

    public record RayKeyframeBlock
    {
        /// <summary>
        /// Offset from previous key frame
        /// </summary>
        public ushort Offset;          
        public ushort TimeMarker;      // maybe frame
        public ushort Width;
        public ushort Height;
        public ushort LocH;
        public ushort LocV;
        public ushort Rotation;
        public byte BlendRaw;          // blend = 100 - BlendRaw
        public byte ForeColor;
        public byte BackColor;
        public byte Padding1;
        public byte Padding2;
    }



    /// <summary>Default sprite data for a channel.</summary>
    //public class RaysChannelSprite
    //{
    //    /// <summary>Cast member displayed when no frame overrides.</summary>
    //    public int DisplayMember {get; internal set; }
    //    /// <summary>Offset to the property descriptor list for behaviors.</summary>
    //    public int SpritePropertiesOffset {get; internal set; }
    //    public int LocH {get; internal set; }
    //    public int LocV {get; internal set; }
    //    public int Width {get; internal set; }
    //    public int Height {get; internal set; }
    //    public float Rotation {get; internal set; }
    //    public float Skew {get; internal set; }
    //    public int Ink {get; internal set; }
    //    public int ForeColor {get; internal set; }
    //    public int BackColor {get; internal set; }
    //    public int ScoreColor {get; internal set; }
    //    public int Blend {get; internal set; }
    //    public bool FlipH {get; internal set; }
    //    public bool FlipV {get; internal set; }
    //    public bool Editable { get;  internal set; }
    //    public int SpriteNum { get; internal set; }
    //}

    /// <summary>List of parsed frame descriptors.</summary>
    public List<RaySprite> Sprites { get; private set; } = new();

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
        //var bytes3raw = RaysUtilities.LogHex(bytes22, bytes22.Length + 1200);


        int totalLength = stream.ReadInt32();
        int headerType = stream.ReadInt32(); // constantMinus3
        int offsetsOffset = stream.ReadInt32(); // constant12
        int entryCount = stream.ReadInt32();
        int notationBase = stream.ReadInt32(); // entryCountPlus1
        int entrySizeSum = stream.ReadInt32();


        Dir?.Logger.LogInformation($"headerType={headerType},offsetsOffset={offsetsOffset},entryCount={entryCount},notationBase={notationBase},entrySizeSum={entrySizeSum}");

        int entriesStart = stream.Pos;
        var annotator = new RayStreamAnnotatorDecorator(stream.Offset);
        var scoreFrameParser = new RaysScoreFrameParser(Dir.Logger, annotator);
        scoreFrameParser.ReadAllIntervals(entryCount,stream);
        scoreFrameParser.ReadFrameData();
        scoreFrameParser.ReadFrameDescriptors();
        scoreFrameParser.ReadBehaviors();
        Sprites = scoreFrameParser.ReadAllFrameSprites();

        //var spriteStates = scoreFrameParser.ParseAllFrameDeltasSafe();

        System.IO.File.WriteAllText("c:\\temp\\director\\tes.md",StreamAnnotationMarkdownWriter.WriteMarkdown(annotator, stream.Data));
        //var keyFrames = scoreFrameParser.ReadKeyFramesPerChannel();
    }
    /// <summary>
    /// Represents a parsed keyframe, including the actual frame number and delta data.
    /// </summary>
    public record ParsedKeyframe(int AbsoluteFrame, int SpriteChannel, byte[] DeltaData);

    /// <summary>
    /// Maps keyframe deltas to their correct absolute frames, based on Interval start frame.
    /// </summary>
    private List<ParsedKeyframe> ParseKeyframes(
        int intervalStartFrame,
        int spriteChannel,
        IReadOnlyList<FrameDeltaItem> deltaItems)
    {
        var keyframes = new List<ParsedKeyframe>();

        foreach (var item in deltaItems)
        {
            int relativeOffset = item.Offset;
            int absoluteFrame = intervalStartFrame + relativeOffset;

            // Usually relativeOffset = 0 is the initial keyframe, so skip if you already handled it
            if (relativeOffset == 0)
                continue;

            keyframes.Add(new ParsedKeyframe(absoluteFrame, spriteChannel, item.Data));
        }

        return keyframes;
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
        foreach (var f in Sprites)
        {
            json.StartObject();
            json.WriteField("start", f.StartFrame);
            json.WriteField("end", f.EndFrame);
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
            json.WriteField("extrValues", string.Join(',', f.ExtraValues));
            json.EndObject();
        }
        json.EndArray();
        json.EndObject();
    }
}