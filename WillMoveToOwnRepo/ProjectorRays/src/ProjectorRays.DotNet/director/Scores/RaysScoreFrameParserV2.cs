using Microsoft.Extensions.Logging;
using ProjectorRays.Common;
using ProjectorRays.director.Scores.Data;
using System;
using System.Buffers.Binary;
using System.ComponentModel.DataAnnotations;
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
    private readonly RaysScoreReader _reader;

    public RayStreamAnnotatorDecorator Annotator => _annotator;




    public RaysScoreFrameParserV2(ILogger logger, RayStreamAnnotatorDecorator annotator)
    {
        _logger = logger;
        _annotator = annotator;
        _reader = new RaysScoreReader(logger);
    }

    public List<RaySprite> ParseScore(ReadStream stream)
    {
        stream.Endianness = Endianness.BigEndian;
        var ctx = new RayScoreParseContext(_annotator, _logger);

        // read headers with intervals
        var header = _reader.ReadMainHeader(stream);
        _reader.ReadAllIntervals(header.EntryCount, stream, ctx);
        if (ctx.FrameDataBufferView == null)
            return new();

        // Create new reader
        var newReader = new ReadStream(ctx.FrameDataBufferView, Endianness.BigEndian, annotator: Annotator);
        //LogFullScoreBytes(newReader);
       // return new List<RaySprite>();
        _reader.ReadHeader(newReader, header);

        _reader.ReadFrameDescriptors(ctx);
        _reader.ReadBehaviors(ctx);

        _logger.LogDebug($"Score header: size={header.ActualSize} frames={header.HighestFrame} channels={header.ChannelCount}");

        //ParseBlock(newReader, header.ActualSize - 20, header, ctx);
        ParseBlock(newReader, newReader.Size, header, ctx);
        LinkKeyFrames(ctx);

        return ctx.Sprites;
    }

  
    private void LogFullScoreBytes(ReadStream readerSource)
    {
        var frameBytes1 = readerSource.ReadBytes(readerSource.BytesLeft);
        var frameData1 = new ReadStream(frameBytes1, frameBytes1.Length, readerSource.Endianness,
            annotator: Annotator);
        var rawata1 = frameData1.LogHex(frameData1.Size);
        _logger.LogInformation("FrameData:" + rawata1);
        return;
    }

  
    private void ParseBlock(ReadStream stream, int length, RayScoreHeader header, RayScoreParseContext ctx)
    {
        long end = stream.Position + length;
        ctx.BlockDepth++;
        while (stream.Position < end && stream.BytesLeft >= 2)
        {
            ushort prefix = stream.ReadUint16("prefix", ctx.GetAnnotationKeys());
            if (prefix == 0) continue;
            if (prefix == RayScoreTagsV2.BlockEnd)
            {
                if (ctx.IsInAdvanceFrameMode)
                {
                    ctx.ClearAdvanceFrame();
                    continue;
                }
                ctx.BlockDepth--;
                return;
            }

            if (prefix == header.SpriteSize)
            {
                var sp = ParseSpriteBlock(stream, header, ctx, header.SpriteSize);
                ctx.SetCurrentSprite(sp);
                continue;
            }

            if (prefix >= header.SpriteSize)
            {
                var tagMain = _reader.ReadMainTag(prefix, stream,ctx);
                if (tagMain == null)
                {
                    if (prefix > 1000)
                    {
                        _logger.LogError($"Wrong byte reading in block at {stream.Pos - 1}: X0{prefix:X2}({prefix})");
                        return;
                    }
                    ParseBlock(stream, prefix, header, ctx);
                }
                else
                {
                    if (tagMain == RayScoreTagsV2.ScoreTagMain.ThisIsASpriteBlock)
                    {
                        var sp = ParseSpriteBlock(stream, header, ctx, header.SpriteSize);
                        ctx.SetCurrentSprite(sp);
                        continue;
                    }
                }

                continue;
            }

            ushort tag = stream.ReadUint16("tag", ctx.GetAnnotationKeys());
            byte[] data = prefix > 0 ? stream.ReadBytes(prefix, "tagData", ctx.GetAnnotationKeys()) : Array.Empty<byte>();
            HandleTag(tag, data, ctx);
        }
        ctx.BlockDepth--;
    }
    //void ValidateNextTag(ReadStream s)
    //{
    //    long pos = s.Position;
    //    ushort tag = s.PeekUint16();

    //    if (tag < 0x0120 || tag > 0x01F6)
    //    {
    //        _logger.LogInformation($"[FrameTag] Unexpected tag {tag:X} at 0x{pos:X} — possible misalignment");
    //    }
    //}


    private RaySprite ParseSpriteBlock(ReadStream stream, RayScoreHeader header, RayScoreParseContext ctx, int length = -1)
    {
        //RayKeyframeBlock block = ReadSprite(stream, header, ctx, length);
        var sprite = _reader.CreateChannelSprite(stream, ctx);

        int channel = ctx.CurrentSpriteNum + 6;
        RayScoreIntervalDescriptor desc;
        if (ctx.ChannelToDescriptor.TryGetValue(channel, out var known))
            desc = known;
        else
        {
            _logger.LogError($"No descriptor found for channel {channel} at frame {ctx.CurrentFrame}. Creating default descriptor.");
            desc = new RayScoreIntervalDescriptor
            {
                StartFrame = ctx.CurrentFrame,
                EndFrame = ctx.CurrentFrame,
                Channel = channel
            };
        }

        sprite.Behaviors.AddRange(desc.Behaviors);
        sprite.ExtraValues.AddRange(desc.ExtraValues);
        sprite.SpriteNumber = channel;
        ctx.AddSprite(sprite);  
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
            var sprite = ctx.GetSprite(channel.Value, ctx.CurrentFrame);
            if (data.Length >= 2)
            {
                ushort flags = BinaryPrimitives.ReadUInt16BigEndian(data);

                const ushort CreateKeyframeBit = 0x8000;
                const ushort AdvanceFrameMask = 0x7F00;

                bool createKeyframe = (flags & CreateKeyframeBit) != 0;
                
                int framesToAdvance = (flags & AdvanceFrameMask) >> 8;

                if (framesToAdvance == 0)
                    framesToAdvance = 1;
                
                ctx.AdvanceFrame(framesToAdvance);

                if (createKeyframe && sprite != null)
                {
                    var keyFrameNew = RaySpriteFactory.CreateKeyFrame(sprite, ctx.CurrentSpriteNum, ctx.CurrentFrame);
                    ctx.AddKeyframe(keyFrameNew);
                    ctx.SetActiveKeyframe(keyFrameNew);
                    _logger.LogInformation($"New KeyFrame:SpriteNum={ctx.CurrentSpriteNum}:Frame={ctx.CurrentFrame}");
                }
            }
            return;
        }
        var known = RayScoreTagsV2.TryParseTag(tag);
        // todo is it for spite or keyframe?
        if (known != null && ctx.CurrentSprite != null)
            _reader.ApplyKnownTag(ctx.CurrentSprite, known.Value, data, ctx);

        // todo : this is wrong
        //var target = ctx.CurrentSprite;
        //var keyFrame = target != null? target : RaySpriteFactory.CreateKeyFrame(target,ctx.CurrentSpriteNum, ctx.CurrentFrame);
        //if (target == null)
        //    ctx.AddKeyframe(keyFrame);

        if (ctx.CurrentKeyframe != null)
        {
            if (known != null)
                _reader.ApplyKnownTag(ctx.CurrentKeyframe, known.Value, data, ctx);
            else
            {
                ctx.CurrentKeyframe.UnknownTags.Add(new UnknownTag(tag, data));
                _logger.LogInformation($"Unkown Spritenum={ctx.CurrentSpriteNum} at frame={ctx.CurrentFrame} : 0x{tag:X2}({tag})");
            }
        }
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

    private void LinkKeyFrames(RayScoreParseContext ctx)
    {
        foreach (var keyFrame in ctx.Keyframes)
        {
            var sprite = ctx.GetSprite(keyFrame.SpriteNum, keyFrame.FrameNum);
            if (sprite != null)
                sprite.Keyframes.Add(keyFrame);
            else
            {
                _logger.LogWarning($"Keyframe for sprite {keyFrame.SpriteNum} at frame {keyFrame.FrameNum} not found in context. " +
                    "This may indicate a parsing error or missing sprite data.");
            }
        }
    }




}
