using Microsoft.Extensions.Logging;
using ProjectorRays.Common;
using System;
using System.Buffers.Binary;
using static ProjectorRays.director.Scores.RaysScoreChunk;


namespace ProjectorRays.director.Scores;

/// <summary>
/// Simplified parser for Director score (VWSC) chunks.
/// It parses the main header and reads consecutive keyframe blocks.
/// The result is a list of sprites each with a single keyframe.
/// </summary>
internal class RaysScoreFrameParserV2
{
    private readonly ILogger _logger;
    private readonly RayStreamAnnotatorDecorator _annotator;
    public RayStreamAnnotatorDecorator Annotator => _annotator;




    public RaysScoreFrameParserV2(ILogger logger, RayStreamAnnotatorDecorator annotator)
    {
        _logger = logger;
        _annotator = annotator;
    }

    public List<RaySprite> ParseScore(ReadStream stream, int entryCount)
    {
        stream.Endianness = Endianness.BigEndian;

        var sprites = new List<RaySprite>();
        var spriteMap = new Dictionary<int, RaySprite>();
        var ctx = new RayScoreParseContext(_annotator, spriteMap, sprites, _logger);

        ctx.ReadAllIntervals(entryCount, stream);
        ctx.ReadFrameDescriptors();
        ctx.ReadBehaviors();

        RaysScoreReader.ScoreHeader header = RaysScoreReader.ReadHeader(stream);
        _logger.LogDebug($"Score header: size={header.ActualSize} frames={header.HighestFrame} channels={header.ChannelCount}");

        ParseBlock(stream, header.ActualSize - 20, header, ctx);

        return sprites;
    }

    private void ParseBlock(ReadStream stream, int length, RaysScoreReader.ScoreHeader header, RayScoreParseContext ctx)
    {
        long end = stream.Position + length;
        ctx.BlockDepth++;
        while (stream.Position < end && stream.BytesLeft >= 2)
        {
            ushort prefix = stream.ReadUint16("prefix", ctx.ToDict());
            if (prefix == RayScoreTagsV2.BlockEnd)
            {
                ctx.BlockDepth--;
                return;
            }

            if (prefix == header.SpriteSize)
            {
                var sp = ParseSpriteBlock(stream, header, ctx, header.SpriteSize);
                ctx.SetCurrentSprite(sp.SpriteNumber);
                continue;
            }

            if (prefix >= header.SpriteSize)
            {
                ParseBlock(stream, prefix, header, ctx);
                continue;
            }

            ushort tag = stream.ReadUint16("tag", ctx.ToDict());
            byte[] data = prefix > 0 ? stream.ReadBytes(prefix, "tagData", ctx.ToDict()) : Array.Empty<byte>();
            HandleTag(tag, data, ctx);
        }
        ctx.BlockDepth--;
    }



    private RaySprite ParseSpriteBlock(ReadStream stream, RaysScoreReader.ScoreHeader header, RayScoreParseContext ctx, int length = -1)
    {
        int size = length > 0 ? length : header.SpriteSize;
        var view = stream.ReadByteView(size, "spriteBlock", ctx.ToDict());
        var rs = new ReadStream(view, Endianness.BigEndian, annotator: ctx.Annotator);
        RaySprite sprite = RaysScoreReader.ReadChannelSprite(rs, _logger, RayKeyframeEnabled.None, ctx.ToDict());
        int channel = ctx.CurrentSprite + 6;
        if (ctx.ChannelToDescriptor.TryGetValue(channel, out var desc))
        {
            sprite.StartFrame = desc.StartFrame;
            sprite.EndFrame = desc.EndFrame;
            sprite.SpriteNumber = desc.Channel;
            sprite.Behaviors.AddRange(desc.Behaviors);
            sprite.ExtraValues.AddRange(desc.ExtraValues);
        }
        else
        {
            sprite.StartFrame = ctx.CurrentFrame;
            sprite.EndFrame = ctx.CurrentFrame;
            sprite.SpriteNumber = channel;
        }
        sprite.Keyframes.Add(RaySpriteFactory.CreateKeyFrame(sprite, sprite.StartFrame));
        return sprite;
    }


    private void HandleTag(ushort tag, byte[] data, RayScoreParseContext ctx)
    {
        if (tag == 0x0180)
        {
            if (data.Length >= 2)
                ctx.UpcomingBlockSize = BinaryPrimitives.ReadUInt16BigEndian(data);
            return;
        }

        if (tag == 0x0120)
        {
            if (data.Length > 0)
                ctx.SetCurrentSprite(data[0]);
            return;
        }

        int? channel = TryDecodeChannel(tag);
        if (channel.HasValue)
        {
            ctx.SetCurrentSprite(channel.Value);
            var sprite = ctx.GetOrCreateSprite(channel.Value);
            if (data.Length >= 2)
            {
                ushort flags = BinaryPrimitives.ReadUInt16BigEndian(data);
                if ((flags & RayScoreTagsV2.KeyframeCreateFlag) == RayScoreTagsV2.KeyframeCreateFlag)
                    sprite.Keyframes.Add(RaySpriteFactory.CreateKeyFrame(sprite, ctx.CurrentFrame));
            }
            return;
        }

        var target = ctx.GetOrCreateSprite(ctx.CurrentSprite + 6);
        var kf = target.Keyframes.Count > 0 ? target.Keyframes[^1] : RaySpriteFactory.CreateKeyFrame(target, ctx.CurrentFrame);
        if (target.Keyframes.Count == 0)
            target.Keyframes.Add(kf);

        var known = RayScoreTagsV2.TryParseTag(tag);
        if (known != null)
            RayKeyframeDeltaDecoderV2.ApplyKnownTag(target, kf, known.Value, data, ctx);
        else
            kf.UnknownTags.Add(new UnknownTag(tag, data));
    }

    /// <summary>
    /// Tags from <c>0x0136</c> upward encode the sprite channel. Every 0x30 step
    /// selects the next channel starting from 6.
    /// </summary>
    private static int? TryDecodeChannel(ushort tag)
    {
        if (tag >= (ushort)RayScoreTagsV2.ScoreTagV2.AdvanceFrame && (tag - 0x0136) % 0x30 == 0)
            return ((tag - 0x0136) / 0x30) + 6;
        return null;
    }





}
