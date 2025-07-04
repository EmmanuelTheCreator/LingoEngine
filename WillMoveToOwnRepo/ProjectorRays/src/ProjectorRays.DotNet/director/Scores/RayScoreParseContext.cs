using ProjectorRays.Common;
using ProjectorRays.director.Scores.Data;
using System.IO;
using static ProjectorRays.director.Scores.RaysScoreChunk;

namespace ProjectorRays.director.Scores;

/// <summary>
/// Holds the current parsing state when decoding a Director score.
/// </summary>
internal class RayScoreParseContext
{

    public RayStreamAnnotatorDecorator Annotator { get; }
    public Dictionary<int, RaySprite> SpriteMap { get; } = new();
    public List<RaySprite> Sprites { get; } = new();
    public Microsoft.Extensions.Logging.ILogger Logger { get; }

    public BufferView? FrameDataBufferView { get; private set; }
    public List<BufferView> FrameIntervalDescriptorBuffers { get; } = new();
    public List<BufferView> BehaviorScriptBuffers { get; } = new();
    public List<RayScoreIntervalDescriptor> Descriptors { get; } = new();
    public Dictionary<int, RayScoreIntervalDescriptor> ChannelToDescriptor { get; } = new();
    public List<List<RaysBehaviourRef>> FrameScripts { get; } = new();
    //public List<int> IntervalOrder { get; } = new();

    public int CurrentFrame { get; private set; }
    public int CurrentSprite { get; private set; }
    public int BlockDepth { get; set; }
    public int UpcomingBlockSize { get; set; }


    public RayScoreParseContext(RayStreamAnnotatorDecorator annotator,Microsoft.Extensions.Logging.ILogger logger)
    {
        Annotator = annotator;
        Logger = logger;
    }


    internal void SetFrameDataBufferView(byte[] data, int absoluteStart, int size) => FrameDataBufferView = new BufferView(data, absoluteStart, size);

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

    public Dictionary<string, int> GetAnnotationKeys() => new()
    {
        ["frame"] = CurrentFrame,
        ["sprite"] = CurrentSprite
    };

    

    internal void AdvanceFrame(int framesToAdvance)
    {
         CurrentFrame += framesToAdvance;
    }

    internal void ResetFrameDescriptors()
    {
        ChannelToDescriptor.Clear();
        Descriptors.Clear();
    }

    internal void AddFrameDescriptor(RayScoreIntervalDescriptor descriptor)
    {
        Descriptors.Add(descriptor);
        ChannelToDescriptor[descriptor.Channel] = descriptor;
    }

    internal void ResetFrameDescriptorBuffers()
    {
        FrameIntervalDescriptorBuffers.Clear();
        BehaviorScriptBuffers.Clear();
    }

    internal void AddFrameDescriptorBuffer(BufferView bufferView)
    {
        FrameIntervalDescriptorBuffers.Add(bufferView);
    }

    internal void AddBehaviorScriptBuffer(BufferView secView)
    {
        BehaviorScriptBuffers.Add(secView);
    }

    internal void AddFrameScript(List<RaysBehaviourRef> behaviourRefs)
    {
        FrameScripts.Add(behaviourRefs);
    }

 
}
