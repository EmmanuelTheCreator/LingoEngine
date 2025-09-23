using System;
using BlingoEngine.Core;
using BlingoEngine.Net.RNetContracts;
using BlingoEngine.Net.RNetHost.Common;

namespace BlingoEngine.Net.RNetPipe.Tests.Fakes;

internal sealed class FakePublisher : IRNetPublisherEngineBridge
{
    public void Enable(IBlingoPlayer player)
    {
    }

    public void Disable()
    {
    }

    public void TryPublishFrame(StageFrameDto frame)
    {
    }

    public void TryPublishDelta(SpriteDeltaDto delta)
    {
    }

    public void TryPublishKeyframe(KeyframeDto keyframe)
    {
    }

    public void TryPublishFilmLoop(FilmLoopDto filmLoop)
    {
    }

    public void TryPublishSound(SoundEventDto sound)
    {
    }

    public void TryPublishTempo(TempoDto tempo)
    {
    }

    public void TryPublishColorPalette(ColorPaletteDto palette)
    {
    }

    public void TryPublishFrameScript(FrameScriptDto script)
    {
    }

    public void TryPublishTransition(TransitionDto transition)
    {
    }

    public void TryPublishMemberProperty(RNetMemberPropertyDto property)
    {
    }

    public void TryPublishMovieProperty(RNetMoviePropertyDto property)
    {
    }

    public void TryPublishStageProperty(RNetStagePropertyDto property)
    {
    }

    public void TryPublishTextStyle(TextStyleDto style)
    {
    }

    public void TryPublishSpriteCollectionEvent(RNetSpriteCollectionEventDto evt)
    {
    }

    public void FlushQueuedProperties()
    {
    }

    public bool TryDrainCommands(Action<IRNetCommand> apply) => false;
}
