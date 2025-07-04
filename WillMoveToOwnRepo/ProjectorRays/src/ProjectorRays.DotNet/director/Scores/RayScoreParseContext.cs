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
    public int CurrentSpriteNum { get; private set; }
    public int BlockDepth { get; set; }
    public int UpcomingBlockSize { get; set; }
    public bool IsInAdvanceFrameMode { get; private set; }

    public RayScoreParseContext(RayStreamAnnotatorDecorator annotator,Microsoft.Extensions.Logging.ILogger logger)
    {
        Annotator = annotator;
        Logger = logger;
    }


    internal void SetFrameDataBufferView(byte[] data, int absoluteStart, int size) => FrameDataBufferView = new BufferView(data, absoluteStart, size);

    public void SetCurrentFrame(int frame) => CurrentFrame = frame;

    public void SetCurrentSprite(int channel)
    {
        CurrentSpriteNum = channel - 6;
        var sprite = Sprites.FirstOrDefault(x => x.SpriteNumber == channel);
        if (sprite != null)
            CurrentFrame = sprite.StartFrame;
      
    }
    public void AddSprite(RaySprite sprite)
    {
          Sprites.Add(sprite);
    }
    public RaySprite GetOrCreateSprite(int channel)
    {
        // todo : smarter system based on begin sprite and end sprite
        var sprite = Sprites.FirstOrDefault(x => x.SpriteNumber == channel);
        if (sprite != null)
        {
            CurrentFrame = sprite.StartFrame;
            return sprite;
        }

        
        sprite = new RaySprite
        {
            SpriteNumber = channel,
            StartFrame = CurrentFrame,
            EndFrame = CurrentFrame
        };
        sprite.Keyframes.Add(RaySpriteFactory.CreateKeyFrame(sprite, CurrentFrame));
        Sprites.Add(sprite);
        return sprite;
    }

    public Dictionary<string, int> GetAnnotationKeys() => new()
    {
        ["frame"] = CurrentFrame,
        ["sprite"] = CurrentSpriteNum
    };



    internal void ClearAdvanceFrame()
        => IsInAdvanceFrameMode = false;
    internal void AdvanceFrame(int framesToAdvance)
    {
        IsInAdvanceFrameMode = true;
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
