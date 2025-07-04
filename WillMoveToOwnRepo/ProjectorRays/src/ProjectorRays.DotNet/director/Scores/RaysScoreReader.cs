using Microsoft.Extensions.Logging;
using ProjectorRays.Common;
using static ProjectorRays.director.Scores.RaysScoreChunk;

namespace ProjectorRays.director.Scores;

internal class RaysScoreReader
{
    private readonly ILogger _logger;

    internal record ScoreHeader(
        int ActualSize,
        byte UnkA1,
        byte UnkA2,
        byte UnkA3,
        byte UnkA4,
        int HighestFrame,
        byte UnkB1,
        byte UnkB2,
        short SpriteSize,
        byte UnkC1,
        byte UnkC2,
        short ChannelCount,
        short FirstBlockSize);

    public RaysScoreReader(ILogger logger)
    {
        _logger = logger;
    }

    internal ScoreHeader ReadHeader(ReadStream stream)
    {
        int actualSize = stream.ReadInt32("actualSize");
        byte unkA1 = stream.ReadUint8("unkA1");
        byte unkA2 = stream.ReadUint8("unkA2");
        byte unkA3 = stream.ReadUint8("unkA3");
        byte unkA4 = stream.ReadUint8("unkA4");
        int highestFrame = stream.ReadInt32("highestFrame");
        byte unkB1 = stream.ReadUint8("unkB1");
        byte unkB2 = stream.ReadUint8("unkB2");
        short spriteSize = stream.ReadInt16("spriteSize");
        byte unkC1 = stream.ReadUint8("unkC1");
        byte unkC2 = stream.ReadUint8("unkC2");
        short channelCount = stream.ReadInt16("channelCount");
        short firstBlockSize = stream.ReadInt16("firstBlockSize");

        return new ScoreHeader(actualSize, unkA1, unkA2, unkA3, unkA4,
            highestFrame, unkB1, unkB2, spriteSize, unkC1, unkC2, channelCount,
            firstBlockSize);
    }

    //private static RayKeyframeBlock ReadSprite(ReadStream stream, RaysScoreReader.ScoreHeader header, RayScoreParseContext ctx, int length)
    //{
    //    int size = length > 0 ? length : header.SpriteSize;
    //    var view = stream.ReadByteView(size, "spriteBlock", ctx.ToDict());
    //    var rs = new ReadStream(view, Endianness.BigEndian, annotator: ctx.Annotator);

    //    RayKeyframeBlock block = new()
    //    {
    //        Offset = rs.ReadUint16("offset", ctx.ToDict()),
    //        TimeMarker = rs.ReadUint16("timeMarker", ctx.ToDict()),
    //        Width = rs.ReadUint16("width", ctx.ToDict()),
    //        Height = rs.ReadUint16("height", ctx.ToDict()),
    //        LocH = rs.ReadUint16("locH", ctx.ToDict()),
    //        LocV = rs.ReadUint16("locV", ctx.ToDict()),
    //        Rotation = rs.ReadUint16("rot", ctx.ToDict()),
    //        BlendRaw = rs.ReadUint8("blendRaw", ctx.ToDict()),
    //        ForeColor = rs.ReadUint8("foreColor", ctx.ToDict()),
    //        BackColor = rs.ReadUint8("backColor", ctx.ToDict()),
    //        Padding1 = rs.ReadUint8("pad1", ctx.ToDict()),
    //        Padding2 = rs.ReadUint8("pad2", ctx.ToDict())
    //    };

    //    var remaining = size - rs.Position;
    //    if (remaining > 0)
    //        rs.Skip(remaining);
    //    return block;
    //}

    public RaySprite ReadChannelSprite(ReadStream stream, RayScoreParseContext ctx)
    {
        var sprite = new RaySprite();
        var keys = ctx.GetAnnotationKeys();
        var flags1 = stream.ReadUint8();
        _logger.LogInformation("Sprite flags=" + flags1);

        if ((flags1 & 0xFF) != 0 && (flags1 & 0x10) == 0)
        {
            var keyframeType = stream.ReadUint8();
            var keyframeSize = stream.ReadUint8();
            _logger.LogInformation($"something1={keyframeType}, something2={keyframeSize}");
        }

        byte inkByte = stream.ReadUint8("ink", keys);
        sprite.Ink = inkByte & 0x7F;
        sprite.ForeColor = stream.ReadUint8("foreColor", keys);
        sprite.BackColor = stream.ReadUint8("backColor", keys);
        sprite.MemberCastLib = stream.ReadUint16("memberCastLib", keys);
        sprite.MemberNum = stream.ReadUint16("memberNum", keys);
        stream.Skip(2);
        sprite.SpritePropertiesOffset = stream.ReadUint16("propOffset", keys);
        sprite.LocV = stream.ReadInt16("locV", keys);
        sprite.LocH = stream.ReadInt16("locH", keys);
        sprite.Height = stream.ReadInt16("height", keys);
        sprite.Width = stream.ReadInt16("width", keys);
        byte colorcode = stream.ReadUint8("color", keys);
        sprite.Editable = (colorcode & 0x40) != 0;
        sprite.ScoreColor = colorcode & 0x0F;
        var blend = stream.ReadUint8("blendRaw", keys);
        sprite.Blend = (int)Math.Round(100f - blend / 255f * 100f);
        byte flag2 = stream.ReadUint8("flipFlags", keys);
        sprite.FlipV = (flag2 & 0x04) != 0;
        sprite.FlipH = (flag2 & 0x02) != 0;
        stream.Skip(5);
        if (stream.Size > 28)
        {
            sprite.Rotation = stream.ReadInt32("rotation", keys) / 100f;
            sprite.Skew = stream.ReadInt32("skew", keys) / 100f;
        }

        _logger.LogInformation($"{sprite.LocH}x{sprite.LocV}:Ink={sprite.Ink}:Blend={sprite.Blend}:Skew={sprite.Skew}:Rot={sprite.Rotation}:PropOffset={sprite.SpritePropertiesOffset}:Member={sprite.MemberCastLib},{sprite.MemberNum}");
        return sprite;
    }

    internal IntervalDescriptor? ReadFrameIntervalDescriptor(int index, ReadStream stream)
    {
        if (stream.Size < 44)
            return null;

        var desc = new IntervalDescriptor();
        var k = new Dictionary<string, int> { ["entry"] = index };
        desc.StartFrame = stream.ReadInt32("startFrame", k);
        desc.EndFrame = stream.ReadInt32("endFrame", k);
        desc.Unknown1 = stream.ReadInt32("unk1", k);

        // Correctly cast to flag enum and extract bitfield
        var flags = (SpriteFlags)stream.ReadInt32("spriteFlagsBitfield", k);

        desc.FlipH = flags.HasFlag(SpriteFlags.FlipH);
        desc.FlipV = flags.HasFlag(SpriteFlags.FlipV);
        desc.Editable = flags.HasFlag(SpriteFlags.Editable);
        desc.Moveable = flags.HasFlag(SpriteFlags.Moveable);
        desc.Trails = flags.HasFlag(SpriteFlags.Trails);
        desc.IsLocked = flags.HasFlag(SpriteFlags.Locked);


        desc.Channel = stream.ReadInt32("channel", k);
        desc.UnknownAlwaysOne = stream.ReadInt16("const1", k);
        desc.UnknownNearConstant15_0 = stream.ReadInt32("const15", k);
        desc.UnknownE1 = stream.ReadUint8("e1", k);
        desc.UnknownFD = stream.ReadUint8("fd", k);
        desc.Unknown7 = stream.ReadInt16("unk7", k);
        desc.Unknown8 = stream.ReadInt32("unk8", k);
        while (stream.Pos + 4 <= stream.Size)
            desc.ExtraValues.Add(stream.ReadInt32("extra", k));
        _logger.LogInformation($"Item Desc. {index}: Start={desc.StartFrame}, End={desc.EndFrame}, Channel={desc.Channel}, U1={desc.Unknown1}, Flip={desc.FlipH},{desc.FlipV},🔒={desc.IsLocked}, , U3={desc.UnknownAlwaysOne}, U4={desc.UnknownNearConstant15_0}, U5={desc.UnknownE1}, U6={desc.UnknownFD}");
        return desc;
    }

    internal static List<RaysBehaviourRef> ReadBehaviors(int ind, ReadStream ss)
    {
        var behaviours = new List<RaysBehaviourRef>();
        while (ss.Pos + 8 <= ss.Size)
        {
            var k = new Dictionary<string, int> { ["entry"] = ind };
            short cl = ss.ReadInt16("castLib", k);
            short cm = ss.ReadInt16("castMmb", k);
            ss.ReadInt32("zero", k);
            behaviours.Add(new RaysBehaviourRef { CastLib = cl, CastMmb = cm });
        }
        return behaviours;
    }

    internal class IntervalDescriptor
    {
        public int StartFrame { get; internal set; }
        public int EndFrame { get; internal set; }
        public int Unknown1 { get; internal set; }
        public int Unknown2 { get; internal set; }
        public int SpriteNumber { get; internal set; }
        public int UnknownAlwaysOne { get; internal set; }
        public int UnknownNearConstant15_0 { get; internal set; }
        public int UnknownE1 { get; internal set; }
        public int UnknownFD { get; internal set; }
        public int Unknown7 { get; internal set; }
        public int Unknown8 { get; internal set; }
        public List<int> ExtraValues { get; } = new();
        public int Channel { get; internal set; }
        public List<RaysBehaviourRef> Behaviors { get; internal set; } = new List<RaysBehaviourRef>();
        public bool FlipH { get; internal set; }
        public bool FlipV { get; internal set; }
        public bool Editable { get; internal set; }
        public bool Moveable { get; internal set; }
        public bool Trails { get; internal set; }
        public bool IsLocked { get; internal set; }
    }
}
