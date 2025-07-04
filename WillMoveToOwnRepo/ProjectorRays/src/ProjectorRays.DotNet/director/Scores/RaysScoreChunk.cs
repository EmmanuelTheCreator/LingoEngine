using Microsoft.Extensions.Logging;
using ProjectorRays.Common;
using ProjectorRays.director.Chunks;
using ProjectorRays.Director;
using static ProjectorRays.director.Scores.RaysScoreFrameParserV2;

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
    public static RayStreamAnnotatorDecorator Annotator;
    /// <summary>
    /// Defines flags for keyframe properties in Director's sprite and animation system.
    /// These flags correspond to properties like position, size, rotation, color, tweening, and more.
    /// </summary>
    [Flags]
    public enum RayKeyframeEnabled: int
    {
        /// <summary>Indicates no properties are enabled.</summary>
        None = 0,

        /// <summary>Indicates tweening is enabled.</summary>
        TweeningEnabled = 1 << 0,

        /// <summary>Indicates position (LocH + LocV) is enabled.</summary>
        Path = 1 << 1,

        /// <summary>Indicates size (Width + Height) is enabled.</summary>
        Size = 1 << 2,

        /// <summary>Indicates rotation is enabled.</summary>
        Rotation = 1 << 3,

        /// <summary>Indicates skew is enabled.</summary>
        Skew = 1 << 4,

        /// <summary>Indicates blend is enabled.</summary>
        Blend = 1 << 5,

        /// <summary>Indicates fore color is enabled.</summary>
        ForeColor = 1 << 6,        // ForeColor (2 bytes)

        /// <summary>Indicates back color is enabled.</summary>
        BackColor = 1 << 7,        // BackColor (2 bytes)

        /// <summary>Indicates continuous tweening at the endpoint.</summary>
        ContinuousAtEndpoint = 1 << 8, // For Continuous checkbox

        /// <summary>Indicates speed (used in tweening).</summary>
        Speed = 1 << 9,              // For Speed setting (1 bit)

        /// <summary>Indicates ease-in for the tweening.</summary>
        EaseIn = 1 << 10,            // Ease-in (1 byte)

        /// <summary>Indicates ease-out for the tweening.</summary>
        EaseOut = 1 << 11,           // Ease-out (1 byte)

        /// <summary>Indicates curvature for the tweening.</summary>
        Curvature = 1 << 12          // Curvature (1 byte)
    }
    public class RayKeyFrame
    {
        public RayKeyframeEnabled EnabledProperties { get; set; } = RayKeyframeEnabled.None;
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
        public int Duration { get; set; }
        /// <summary>Absolute frame this keyframe applies to.</summary>
        public int Frame { get; set; }
        public float ScaleX { get; internal set; }
        public float ScaleY { get; internal set; }
        public List<UnknownTag> UnknownTags { get; } = new();
    }

    /// <summary>Unknown tag entry encountered during keyframe parsing.</summary>
    public record UnknownTag(ushort Tag, byte[] Data);
    /// <summary>Descriptor of a sprite on the score timeline.</summary>
    public class RaySprite
    {
        /// <summary>Cast member displayed by this sprite.</summary>
        /// <summary>Offset to sprite property table for behaviors.</summary>
        public int StartFrame {get; internal set; }
        public int EndFrame {get; internal set; }
        public int SpriteNumber {get; internal set; }
        public int MemberCastLib { get; set; }
        public int MemberNum { get; set; }
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

        public byte EaseIn { get; set; }
        public byte EaseOut { get; set; }
        public ushort Curvature { get; set; }
        public byte TweenFlags { get; set; }


        public List<int> ExtraValues { get; internal set; } = new();
        public List<RaysBehaviourRef> Behaviors { get; internal set; } = new();
        public List<RayKeyFrame> Keyframes { get; internal set; } = new();
        public int LocZ { get; set; }
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
        

        //Dir?.Logger.LogInformation($"headerType={headerType},offsetsOffset={offsetsOffset},entryCount={entryCount},notationBase={notationBase},entrySizeSum={entrySizeSum}");

        int entriesStart = stream.Pos;
        Annotator = new RayStreamAnnotatorDecorator(stream.Offset);
        var parser = new RaysScoreFrameParserV2(Dir.Logger, Annotator);
        Sprites = parser.ParseScore(stream, entryCount);
        return;


        //var spriteStates = scoreFrameParser.ParseAllFrameDeltasSafe();

        //System.IO.File.WriteAllText("c:\\temp\\director\\tes.md",StreamAnnotationMarkdownWriter.WriteMarkdown(annotator, stream.Data));
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
            json.WriteField("MemberCastLib", f.MemberCastLib.ToString());
            json.WriteField("MemberNum", f.MemberNum.ToString());
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