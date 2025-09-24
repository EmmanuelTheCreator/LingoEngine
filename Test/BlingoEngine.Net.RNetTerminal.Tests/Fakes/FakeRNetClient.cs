using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using BlingoEngine.IO.Data.DTO;
using BlingoEngine.Net.RNetClient.Common;
using BlingoEngine.Net.RNetContracts;

namespace BlingoEngine.Net.RNetTerminal.Tests.Fakes;

internal sealed class FakeRNetClient : IBlingoRNetClient
{
    private readonly Channel<SpriteDeltaDto> _deltaChannel;
    private readonly TaskCompletionSource<bool> _deltaEnumerationStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly string _projectJson;
    private readonly MovieStateDto _movieState;

    public FakeRNetClient(BlingoProjectDTO project, MovieStateDto movieState)
    {
        if (project is null)
        {
            throw new ArgumentNullException(nameof(project));
        }

        _projectJson = JsonSerializer.Serialize(project);
        _movieState = movieState;
        _deltaChannel = Channel.CreateUnbounded<SpriteDeltaDto>(new UnboundedChannelOptions
        {
            SingleReader = false,
            SingleWriter = false
        });
    }

    public event Action<BlingoNetConnectionState>? ConnectionStatusChanged;

    public event Action<IRNetCommand>? NetCommandReceived;

    public BlingoNetConnectionState ConnectionState { get; private set; } = BlingoNetConnectionState.Disconnected;

    public bool IsConnected => ConnectionState == BlingoNetConnectionState.Connected;

    public Task ConnectAsync(Uri hubUrl, HelloDto hello, CancellationToken ct = default)
    {
        ConnectionState = BlingoNetConnectionState.Connected;
        ConnectionStatusChanged?.Invoke(ConnectionState);
        return Task.CompletedTask;
    }

    public Task DisconnectAsync()
    {
        ConnectionState = BlingoNetConnectionState.Disconnected;
        ConnectionStatusChanged?.Invoke(ConnectionState);
        _deltaChannel.Writer.TryComplete();
        return Task.CompletedTask;
    }

    public IAsyncEnumerable<StageFrameDto> StreamFramesAsync(CancellationToken ct = default)
        => EmptyAsync<StageFrameDto>(ct);

    public IAsyncEnumerable<SpriteDeltaDto> StreamDeltasAsync(CancellationToken ct = default)
        => ReadChannelAsync(_deltaChannel.Reader, ct, _deltaEnumerationStarted);

    public IAsyncEnumerable<KeyframeDto> StreamKeyframesAsync(CancellationToken ct = default)
        => EmptyAsync<KeyframeDto>(ct);

    public IAsyncEnumerable<TempoDto> StreamTemposAsync(CancellationToken ct = default)
        => EmptyAsync<TempoDto>(ct);

    public IAsyncEnumerable<ColorPaletteDto> StreamColorPalettesAsync(CancellationToken ct = default)
        => EmptyAsync<ColorPaletteDto>(ct);

    public IAsyncEnumerable<FrameScriptDto> StreamFrameScriptsAsync(CancellationToken ct = default)
        => EmptyAsync<FrameScriptDto>(ct);

    public IAsyncEnumerable<TransitionDto> StreamTransitionsAsync(CancellationToken ct = default)
        => EmptyAsync<TransitionDto>(ct);

    public IAsyncEnumerable<RNetMemberPropertyDto> StreamMemberPropertiesAsync(CancellationToken ct = default)
        => EmptyAsync<RNetMemberPropertyDto>(ct);

    public IAsyncEnumerable<RNetMoviePropertyDto> StreamMoviePropertiesAsync(CancellationToken ct = default)
        => EmptyAsync<RNetMoviePropertyDto>(ct);

    public IAsyncEnumerable<RNetStagePropertyDto> StreamStagePropertiesAsync(CancellationToken ct = default)
        => EmptyAsync<RNetStagePropertyDto>(ct);

    public IAsyncEnumerable<RNetSpriteCollectionEventDto> StreamSpriteCollectionEventsAsync(CancellationToken ct = default)
        => EmptyAsync<RNetSpriteCollectionEventDto>(ct);

    public IAsyncEnumerable<TextStyleDto> StreamTextStylesAsync(CancellationToken ct = default)
        => EmptyAsync<TextStyleDto>(ct);

    public IAsyncEnumerable<FilmLoopDto> StreamFilmLoopsAsync(CancellationToken ct = default)
        => EmptyAsync<FilmLoopDto>(ct);

    public IAsyncEnumerable<SoundEventDto> StreamSoundsAsync(CancellationToken ct = default)
        => EmptyAsync<SoundEventDto>(ct);

    public Task<MovieStateDto> GetMovieSnapshotAsync(CancellationToken ct = default)
        => Task.FromResult(_movieState);

    public Task<BlingoProjectJsonDto> GetCurrentProjectAsync(CancellationToken ct = default)
        => Task.FromResult(new BlingoProjectJsonDto(_projectJson));

    public Task SendCommandAsync(RNetCommand cmd, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task SendHeartbeatAsync(TimeSpan? timeout = null, CancellationToken ct = default)
        => Task.CompletedTask;

    public ValueTask DisposeAsync()
        => new(DisconnectAsync());

    public void PublishDelta(SpriteDeltaDto delta)
    {
        if (!_deltaChannel.Writer.TryWrite(delta))
        {
            throw new InvalidOperationException("Failed to publish delta to fake client channel.");
        }
    }

    public Task WaitForDeltaEnumerationAsync(CancellationToken ct = default)
        => _deltaEnumerationStarted.Task.WaitAsync(ct);

    private static async IAsyncEnumerable<T> EmptyAsync<T>([EnumeratorCancellation] CancellationToken ct)
    {
        await Task.CompletedTask;
        yield break;
    }

    private static async IAsyncEnumerable<T> ReadChannelAsync<T>(
        ChannelReader<T> reader,
        [EnumeratorCancellation] CancellationToken ct,
        TaskCompletionSource<bool>? started = null)
    {
        started?.TrySetResult(true);

        while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            while (reader.TryRead(out var item))
            {
                yield return item;
            }
        }
    }
}
