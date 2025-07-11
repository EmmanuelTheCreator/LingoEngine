using Microsoft.Extensions.Logging;
using ProjectorRays.Common;
using ProjectorRays.director.Scores.Data;
using static ProjectorRays.director.Scores.RayScoreTagsV2;
using static ProjectorRays.director.Scores.RaysScoreChunk;

namespace ProjectorRays.director.Scores;
internal class RaysScoreReader
{
    private readonly ILogger _logger;

   

    public RaysScoreReader(ILogger logger)
    {
        _logger = logger;
    }


    #region Header
    internal void ReadAllIntervals(int entryCount, ReadStream stream, RayScoreParseContext ctx)
    {
        var s = new ReadStream(new BufferView(stream.Data, stream.Offset, stream.Size),
            stream.Endianness, stream.Pos, Annotator);

        int[] offsets = new int[entryCount + 1];
        for (int i = 0; i < offsets.Length; i++)
            offsets[i] = s.ReadInt32($"offset[{i}]");

        int entriesStart = s.Pos;

        if (entryCount < 1)
            return;

        var size = offsets[1] - offsets[0];
        int absoluteStart = stream.Offset + entriesStart + offsets[0];

        ctx.SetFrameDataBufferView(stream.Data, absoluteStart, size);
        List<int> intervalOrder = new();
        if (entryCount >= 2)
        {
            size = offsets[2] - offsets[1];
            int absoluteStart2 = stream.Offset + entriesStart + offsets[1];
            var orderView = new BufferView(stream.Data, absoluteStart2, offsets[2] - offsets[1]);
            var os = new ReadStream(orderView, Endianness.BigEndian, annotator: Annotator);
            if (os.Size >= 4)
            {
                int count = os.ReadInt32("orderCount");
                for (int i = 0; i < count && os.Pos + 4 <= os.Size; i++)
                    intervalOrder.Add(os.ReadInt32("order", new() { ["index"] = i }));
            }
        }
        
        var entryIndices = intervalOrder.Count > 0 ? intervalOrder : null;
        if (entryIndices == null)
        {
            entryIndices = new List<int>();
            for (int i = 3; i + 2 < offsets.Length; i += 3)
                entryIndices.Add(i);
        }
        ctx.ResetFrameDescriptorBuffers();

        foreach (int primaryIdx in entryIndices)
        {
            if (primaryIdx + 2 >= offsets.Length)
                continue;

            size = offsets[primaryIdx + 1] - offsets[primaryIdx];
            int absoluteStart2 = stream.Offset + entriesStart + offsets[primaryIdx];
            ctx.AddFrameDescriptorBuffer(new BufferView(stream.Data, absoluteStart2, size));


            var secSize = offsets[primaryIdx + 2] - offsets[primaryIdx + 1];
            if (secSize > 0)
            {
                int absoluteStart3 = stream.Offset + entriesStart + offsets[primaryIdx + 1];
                var secView = new BufferView(stream.Data, absoluteStart3, secSize);
                ctx.AddBehaviorScriptBuffer(secView);
            }
        }
        
    }
    internal RayScoreHeader ReadMainHeader(ReadStream stream)
    {
        return new RayScoreHeader
        {
            TotalLength = stream.ReadInt32(),
            HeaderType = stream.ReadInt32(), // constantMinus3
            OffsetsOffset = stream.ReadInt32(), // constant12
            EntryCount = stream.ReadInt32(),
            NotationBase = stream.ReadInt32(), // entryCountPlus1
            EntrySizeSum = stream.ReadInt32(),
        };
    }

    internal void ReadHeader(ReadStream stream, RayScoreHeader header)
    {
        header.ActualSize = stream.ReadInt32("actualSize");
        header.UnkA1 = stream.ReadUint8("unkA1");
        header.UnkA2 = stream.ReadUint8("unkA2");
        header.UnkA3 = stream.ReadUint8("unkA3");
        header.UnkA4 = stream.ReadUint8("unkA4");
        header.HighestFrame = stream.ReadInt32("highestFrame");
        header.UnkB1 = stream.ReadUint8("unkB1");
        header.UnkB2 = stream.ReadUint8("unkB2");
        header.SpriteSize = stream.ReadInt16("spriteSize");
        header.UnkC1 = stream.ReadUint8("unkC1");
        header.UnkC2 = stream.ReadUint8("unkC2");
        header.ChannelCount = stream.ReadInt16("channelCount");
        //header.FirstBlockSize = stream.ReadInt16("firstBlockSize");
    }


    #endregion



    public ScoreTagMain? ReadMainTag(ushort data, ReadStream stream,RayScoreParseContext ctx)
    {
        var tag = RayScoreTagsV2.TryParseTagMain(data);
        return tag;
    }


    public void ApplyKnownTag(RaySprite sprite, ScoreTagV2 tag, byte[] data, RayScoreParseContext ctx)
    {
        var rs = new ReadStream(data, data.Length, Endianness.BigEndian, annotator: ctx.Annotator);
        int expectedSize = GetDataLength(tag);
        if (expectedSize > 0 && data.Length != expectedSize)
            ctx.Logger.LogWarning($"Tag {tag:X4} expected {expectedSize} bytes, got {data.Length}");
        _logger.LogInformation($"Sprite Tag parse:{tag}:{string.Join(',', data.Select(x => $"{x.ToString("X2")}({x})"))}");
        switch (tag)
        {
            case ScoreTagV2.Ease:
                sprite.EaseIn = rs.ReadUint8("easeIn", ctx.GetAnnotationKeys());
                sprite.EaseOut = rs.ReadUint8("easeOut", ctx.GetAnnotationKeys());
                break;
            case ScoreTagV2.Curvature:
                sprite.Curvature = rs.ReadUint16("curv", ctx.GetAnnotationKeys());
                break;
            case ScoreTagV2.AdvanceFrame:
                rs.ReadUint16("adv", ctx.GetAnnotationKeys());
                break;
            case ScoreTagV2.Flags:
                rs.ReadUint16("flags", ctx.GetAnnotationKeys());
                break;
            case ScoreTagV2.FlagsControl:
                rs.ReadUint16("flagsCtrl", ctx.GetAnnotationKeys());
                break;
            case ScoreTagV2.TweenFlags:
                sprite.TweenFlags = ParseTweenFlags(rs.ReadUint8("tweenFlags", ctx.GetAnnotationKeys()));
                break;
            default:
                break;
        }
    }
    
    
    public void ApplyKnownTag(RayScoreKeyFrame kf, ScoreTagV2 tag, byte[] data, RayScoreParseContext ctx)
    {
        var rs = new ReadStream(data, data.Length, Endianness.BigEndian, annotator: ctx.Annotator);
        int expectedSize = GetDataLength(tag);
        if (expectedSize > 0 && data.Length != expectedSize)
            ctx.Logger.LogWarning($"Tag {tag:X4} expected {expectedSize} bytes, got {data.Length}");
        _logger.LogInformation($"KeyFrame Tag parse:{tag}:{string.Join(',', data.Select(x => $"{x.ToString("X2")}({x})"))}");
        switch (tag)
        {
            case ScoreTagV2.Size:
                kf.Width = rs.ReadInt16("width", ctx.GetAnnotationKeys());
                kf.Height = rs.ReadInt16("height", ctx.GetAnnotationKeys());
                break;
            case ScoreTagV2.Position:
                kf.LocH = rs.ReadInt16("locH", ctx.GetAnnotationKeys());
                kf.LocV = rs.ReadInt16("locV", ctx.GetAnnotationKeys());
                break;
            case ScoreTagV2.Colors:
                kf.ForeColor = rs.ReadUint8("foreColor", ctx.GetAnnotationKeys());
                kf.BackColor = rs.ReadUint8("backColor", ctx.GetAnnotationKeys());
                break;
            case ScoreTagV2.Ink:
                kf.Ink = rs.ReadInt16("ink", ctx.GetAnnotationKeys());
                break;
            case ScoreTagV2.Rotation:
                kf.Rotation = rs.ReadInt16("rot", ctx.GetAnnotationKeys()) / 100f;
                break;
            case ScoreTagV2.Skew:
                kf.Skew = rs.ReadInt16("skew", ctx.GetAnnotationKeys()) / 100f;
                break;
            case ScoreTagV2.AdvanceFrame:
                rs.ReadUint16("adv", ctx.GetAnnotationKeys());
                break;
            case ScoreTagV2.Composite:
                kf.Width = rs.ReadInt16("width", ctx.GetAnnotationKeys());
                kf.Height = rs.ReadInt16("height", ctx.GetAnnotationKeys());
                byte blendRaw = rs.ReadUint8("blendRaw", ctx.GetAnnotationKeys());
                kf.Blend = 100 - blendRaw * 100 / 255;
                kf.Ink = rs.ReadUint8("ink", ctx.GetAnnotationKeys());
                break;
            case ScoreTagV2.FrameRect:
                kf.LocH = rs.ReadInt16("locH", ctx.GetAnnotationKeys());
                kf.LocV = rs.ReadInt16("locV", ctx.GetAnnotationKeys());
                kf.Width = rs.ReadInt16("width", ctx.GetAnnotationKeys());
                kf.Height = rs.ReadInt16("height", ctx.GetAnnotationKeys());
                break;
            case ScoreTagV2.Flags:
                rs.ReadUint16("flags", ctx.GetAnnotationKeys());
                break;
            case ScoreTagV2.FlagsControl:
                rs.ReadUint16("flagsCtrl", ctx.GetAnnotationKeys());
                break;
            default:
                break;
        }
    }
   
    public RaySprite CreateChannelSprite(ReadStream stream, RayScoreParseContext ctx)
    {
        var startPos = stream.Pos;
        var sprite = new RaySprite();
        var keys = ctx.GetAnnotationKeys();
        //var flags1 = stream.ReadUint8();

        byte flagByte = stream.ReadUint8("tweenFlags", keys);
        sprite.TweenFlags = ParseTweenFlags(flagByte);
        
        var something2 = stream.ReadInt16();

        //_logger.LogInformation("Sprite flags=" + flags1);

        //if ((flags1 & 0xFF) != 0 && (flags1 & 0x10) == 0)
        //{
        //    var keyframeType = stream.ReadUint8();
        //    var keyframeSize = stream.ReadUint8();
        //    _logger.LogInformation($"something1={keyframeType}, something2={keyframeSize}");
        //}

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
        sprite.Rotation = stream.ReadInt32("rotation", keys) / 100f;
        sprite.Skew = stream.ReadInt32("skew", keys) / 100f;
        var skip = 48 - (stream.Pos - startPos);
        stream.Skip(skip); // We need to read exactly 48 bytes

        _logger.LogInformation($"{sprite.LocH}x{sprite.LocV}:Ink={sprite.Ink}:Blend={sprite.Blend}:Skew={sprite.Skew}:Rot={sprite.Rotation}:PropOffset={sprite.SpritePropertiesOffset}:Member={sprite.MemberCastLib},{sprite.MemberNum}:Tween={ sprite.TweenFlags}");
        return sprite;
    }


    public RayTweenFlags ParseTweenFlags(byte value)
    {
        return new RayTweenFlags
        {
            TweeningEnabled = (value & 0x01) != 0,
            Path = (value & 0x02) != 0,
            Size = (value & 0x04) != 0,
            Rotation = (value & 0x08) != 0,
            Skew = (value & 0x10) != 0,
            Blend = (value & 0x20) != 0,
            ForeColor = (value & 0x40) != 0,
            BackColor = (value & 0x80) != 0
        };
    }


    #region Frame descriptors

    internal RayScoreIntervalDescriptor? ReadFrameIntervalDescriptor(int index, ReadStream stream)
    {
        if (stream.Size < 44)
            return null;

        var desc = new RayScoreIntervalDescriptor();
        var k = new Dictionary<string, int> { ["entry"] = index };
        desc.StartFrame = stream.ReadInt32("startFrame", k);
        desc.EndFrame = stream.ReadInt32("endFrame", k);
        desc.Unknown1 = stream.ReadInt32("unk1", k);
        desc.Unknown2 = stream.ReadInt16("unk2", k);

        // Correctly cast to flag enum and extract bitfield
        var flags = (RaySpriteFlags)stream.ReadInt16("spriteFlagsBitfield", k);

        desc.FlipH = flags.HasFlag(RaySpriteFlags.FlipH);
        desc.FlipV = flags.HasFlag(RaySpriteFlags.FlipV);
        desc.Editable = flags.HasFlag(RaySpriteFlags.Editable);
        desc.Moveable = flags.HasFlag(RaySpriteFlags.Moveable);
        desc.Trails = flags.HasFlag(RaySpriteFlags.Trails);
        desc.IsLocked = flags.HasFlag(RaySpriteFlags.Locked);


        desc.Channel = stream.ReadInt32("channel", k);
        desc.UnknownAlwaysOne = stream.ReadInt16("const1", k);
        desc.UnkownA = stream.ReadInt16("unkownA", k);
        desc.UnkownB = stream.ReadInt16("unkownB", k);
        desc.UnknownE1 = stream.ReadUint8("e1", k);
        desc.UnknownFD = stream.ReadUint8("fd", k);
        desc.Unknown7 = stream.ReadInt16("unk7", k);
        desc.Unknown8 = stream.ReadInt32("unk8", k);
        while (stream.Pos + 4 <= stream.Size)
            desc.ExtraValues.Add(stream.ReadInt32("extra", k));
        _logger.LogInformation($"Item Desc. {index}: Start={desc.StartFrame}, End={desc.EndFrame}, Channel={desc.Channel}, U1={desc.Unknown1}, Flip={desc.FlipH},{desc.FlipV},🔒={desc.IsLocked},Trails={desc.Trails},Editable={desc.Editable},Moveable={desc.Moveable} , U3={desc.UnknownAlwaysOne}, U4A={desc.UnkownA}, U4B={desc.UnkownB}, U5={desc.UnknownE1}, U6={desc.UnknownFD}");
        return desc;
    }
    internal void ReadFrameDescriptors(RayScoreParseContext ctx)
    {
        ctx.ResetFrameDescriptors();

        int ind = 0;
        foreach (var frameIntervalDescriptor in ctx.FrameIntervalDescriptorBuffers)
        {
            var ps = new ReadStream(frameIntervalDescriptor, Endianness.BigEndian,
                annotator: Annotator);
            var descriptor = ReadFrameIntervalDescriptor(ind, ps);
            if (descriptor != null)
                ctx.AddFrameDescriptor(descriptor);


            ind++;
        }
    }
    #endregion



    #region Behaviors
    internal void ReadBehaviors(RayScoreParseContext ctx)
    {
        int ind = 0;
        foreach (var frameIntervalDescriptor in ctx.BehaviorScriptBuffers)
        {
            var ps = new ReadStream(frameIntervalDescriptor, Endianness.BigEndian,
                annotator: Annotator);
            var behaviourRefs = ReadBehaviors(ind, ps);
            if (ind < ctx.Descriptors.Count)
                ctx.Descriptors[ind].Behaviors.AddRange(behaviourRefs);
            ctx.AddFrameScript(behaviourRefs);

            ind++;
        }
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

    internal void ApplyKnownTag(object currentSprite, ScoreTagV2 value, byte[] data, RayScoreParseContext ctx)
    {
        throw new NotImplementedException();
    }

    #endregion



}

