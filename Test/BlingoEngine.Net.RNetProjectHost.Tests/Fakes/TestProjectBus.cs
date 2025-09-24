using System.Threading.Channels;
using BlingoEngine.Net.RNetContracts;
using BlingoEngine.Net.RNetProjectHost;

namespace BlingoEngine.Net.RNetProjectHost.Tests.Fakes;

internal sealed class TestProjectBus : IRNetProjectBus
{
    public Channel<StageFrameDto> Frames { get; } = CreateChannel<StageFrameDto>();
    public Channel<SpriteDeltaDto> Deltas { get; } = CreateChannel<SpriteDeltaDto>();
    public Channel<KeyframeDto> Keyframes { get; } = CreateChannel<KeyframeDto>();
    public Channel<FilmLoopDto> FilmLoops { get; } = CreateChannel<FilmLoopDto>();
    public Channel<SoundEventDto> Sounds { get; } = CreateChannel<SoundEventDto>();
    public Channel<TempoDto> Tempos { get; } = CreateChannel<TempoDto>();
    public Channel<ColorPaletteDto> ColorPalettes { get; } = CreateChannel<ColorPaletteDto>();
    public Channel<FrameScriptDto> FrameScripts { get; } = CreateChannel<FrameScriptDto>();
    public Channel<TransitionDto> Transitions { get; } = CreateChannel<TransitionDto>();
    public Channel<RNetMemberPropertyDto> MemberProperties { get; } = CreateChannel<RNetMemberPropertyDto>();
    public Channel<TextStyleDto> TextStyles { get; } = CreateChannel<TextStyleDto>();
    public Channel<RNetMoviePropertyDto> MovieProperties { get; } = CreateChannel<RNetMoviePropertyDto>();
    public Channel<RNetStagePropertyDto> StageProperties { get; } = CreateChannel<RNetStagePropertyDto>();
    public Channel<RNetSpriteCollectionEventDto> SpriteCollectionEvents { get; } = CreateChannel<RNetSpriteCollectionEventDto>();
    public Channel<IRNetCommand> Commands { get; } = CreateCommandChannel();

    private static Channel<T> CreateChannel<T>() => Channel.CreateUnbounded<T>(new UnboundedChannelOptions
    {
        SingleReader = false,
        SingleWriter = false
    });

    private static Channel<IRNetCommand> CreateCommandChannel()
        => Channel.CreateUnbounded<IRNetCommand>(new UnboundedChannelOptions
        {
            SingleReader = false,
            SingleWriter = false
        });
}
