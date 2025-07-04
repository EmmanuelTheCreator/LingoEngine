using ProjectorRays.Common;
using System.Collections.Generic;
using static ProjectorRays.director.Scores.RaysScoreChunk;

namespace ProjectorRays.director.Scores;

/// <summary>
/// Holds the current parsing state when decoding a Director score.
/// </summary>
internal class RayScoreParseContext
{
    public RayScoreParseContext(RayStreamAnnotatorDecorator annotator,
        Dictionary<int, RaySprite> spriteMap, List<RaySprite> sprites,
        Microsoft.Extensions.Logging.ILogger logger)
    {
        Annotator = annotator;
        SpriteMap = spriteMap;
        Sprites = sprites;
        Logger = logger;
    }

    public RayStreamAnnotatorDecorator Annotator { get; }
    public Dictionary<int, RaySprite> SpriteMap { get; }
    public List<RaySprite> Sprites { get; }
    public Microsoft.Extensions.Logging.ILogger Logger { get; }

    public BufferView FrameDataBufferView { get; private set; }
    public List<BufferView> FrameIntervalDescriptorBuffers { get; } = new();
    public List<BufferView> BehaviorScriptBuffers { get; } = new();
    public List<RaysScoreReader.IntervalDescriptor> Descriptors { get; } = new();
    public Dictionary<int, RaysScoreReader.IntervalDescriptor> ChannelToDescriptor { get; } = new();
    public List<List<RaysBehaviourRef>> FrameScripts { get; } = new();
    public List<int> IntervalOrder { get; } = new();

    public int CurrentFrame { get; private set; }
    public int CurrentSprite { get; private set; }
    public int BlockDepth { get; set; }
    public int UpcomingBlockSize { get; set; }

    public void SetCurrentFrame(int frame) => CurrentFrame = frame;

    public void SetCurrentSprite(int channel)
    {
        CurrentSprite = channel - 6;
        if (SpriteMap.TryGetValue(channel, out var sp))
            CurrentFrame = sp.StartFrame;
    }

    public RaySprite GetOrCreateSprite(int channel)
    {
        if (!SpriteMap.TryGetValue(channel, out var sprite))
        {
            sprite = new RaySprite
            {
                SpriteNumber = channel,
                StartFrame = CurrentFrame,
                EndFrame = CurrentFrame
            };
            sprite.Keyframes.Add(RaySpriteFactory.CreateKeyFrame(sprite, CurrentFrame));
            SpriteMap[channel] = sprite;
            Sprites.Add(sprite);
        }
        return sprite;
    }

    public Dictionary<string, int> ToDict() => new()
    {
        ["frame"] = CurrentFrame,
        ["sprite"] = CurrentSprite
    };

    internal void ReadAllIntervals(int entryCount, ReadStream stream)
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
        FrameDataBufferView = new BufferView(stream.Data, absoluteStart, size);

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
                    IntervalOrder.Add(os.ReadInt32("order", new() { ["index"] = i }));
            }
        }

        var entryIndices = IntervalOrder.Count > 0 ? IntervalOrder : null;
        if (entryIndices == null)
        {
            entryIndices = new List<int>();
            for (int i = 3; i + 2 < offsets.Length; i += 3)
                entryIndices.Add(i);
        }

        FrameIntervalDescriptorBuffers.Clear();
        BehaviorScriptBuffers.Clear();
        foreach (int primaryIdx in entryIndices)
        {
            if (primaryIdx + 2 >= offsets.Length)
                continue;

            size = offsets[primaryIdx + 1] - offsets[primaryIdx];
            int absoluteStart2 = stream.Offset + entriesStart + offsets[primaryIdx];
            FrameIntervalDescriptorBuffers.Add(new BufferView(stream.Data, absoluteStart2, size));

            var secSize = offsets[primaryIdx + 2] - offsets[primaryIdx + 1];
            if (secSize > 0)
            {
                int absoluteStart3 = stream.Offset + entriesStart + offsets[primaryIdx + 1];
                var secView = new BufferView(stream.Data, absoluteStart3, secSize);
                BehaviorScriptBuffers.Add(secView);
            }
        }
    }

    internal void ReadFrameDescriptors()
    {
        ChannelToDescriptor.Clear();
        Descriptors.Clear();
        int ind = 0;
        foreach (var frameIntervalDescriptor in FrameIntervalDescriptorBuffers)
        {
            var ps = new ReadStream(frameIntervalDescriptor, Endianness.BigEndian,
                annotator: Annotator);
            var descriptor = RaysScoreReader.ReadFrameIntervalDescriptor(ind, ps);
            if (descriptor != null)
            {
                Descriptors.Add(descriptor);
                ChannelToDescriptor[descriptor.Channel] = descriptor;
            }
            ind++;
        }
    }

    internal void ReadBehaviors()
    {
        int ind = 0;
        foreach (var frameIntervalDescriptor in BehaviorScriptBuffers)
        {
            var ps = new ReadStream(frameIntervalDescriptor, Endianness.BigEndian,
                annotator: Annotator);
            var behaviourRefs = RaysScoreReader.ReadBehaviors(ind, ps);
            if (ind < Descriptors.Count)
                Descriptors[ind].Behaviors.AddRange(behaviourRefs);
            FrameScripts.Add(behaviourRefs);
            ind++;
        }
    }

    internal void AdvanceFrame(int framesToAdvance)
    {
         CurrentFrame += framesToAdvance;
    }
}
