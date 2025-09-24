using System;
using System.Collections.Generic;
using System.Text.Json;
using BlingoEngine.Net.RNetClient.Common;
using BlingoEngine.Net.RNetContracts;
using Microsoft.AspNetCore.SignalR.Client;

namespace BlingoEngine.Net.RNetProjectClient;

/// <summary>
/// High-level API for interacting with a running RNet project host.
/// </summary>
public interface IBlingoRNetProjectClient : IBlingoRNetClient { }

/// <summary>
/// Default implementation of <see cref="IBlingoRNetProjectClient"/>.
/// </summary>
public sealed class BlingoRNetProjectClient : IBlingoRNetProjectClient
{
    private HubConnection? _connection;
    private BlingoNetConnectionState _state = BlingoNetConnectionState.Disconnected;
    private readonly IRNetConfiguration _config;
    private static readonly JsonSerializerOptions CommandSerializerOptions = new(JsonSerializerDefaults.Web);

    public BlingoRNetProjectClient(IRNetConfiguration config)
    {
        _config = config;
    }

    public BlingoRNetProjectClient() : this(new DefaultConfig()) { }

    private sealed class DefaultConfig : IRNetConfiguration
    {
        public int Port { get; set; } = 61699;
        public bool AutoStartRNetHostOnStartup { get; set; }
        public string ClientName { get; set; } = "Some client";
        public RNetClientType ClientType { get; set; } = RNetClientType.Project;
        public RNetRemoteRole RemoteRole { get; set; } = RNetRemoteRole.Client;
    }

    private sealed record CommandEnvelope(string Type, JsonElement Payload);

    /// <inheritdoc />
    public event Action<BlingoNetConnectionState>? ConnectionStatusChanged;

    /// <inheritdoc />
    public event Action<IRNetCommand>? NetCommandReceived;

    /// <inheritdoc />
    public BlingoNetConnectionState ConnectionState
    {
        get => _state;
        private set
        {
            if (_state == value)
            {
                return;
            }

            _state = value;
            ConnectionStatusChanged?.Invoke(_state);
        }
    }

    /// <inheritdoc />
    public bool IsConnected => ConnectionState == BlingoNetConnectionState.Connected;

    /// <inheritdoc />
    public async Task ConnectAsync(Uri hubUrl, HelloDto hello, CancellationToken ct = default)
    {
        if (_connection is not null)
        {
            return;
        }

        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        _connection.Reconnecting += _ =>
        {
            ConnectionState = BlingoNetConnectionState.Connecting;
            return Task.CompletedTask;
        };
        _connection.Reconnected += _ =>
        {
            ConnectionState = BlingoNetConnectionState.Connected;
            return Task.CompletedTask;
        };
        _connection.Closed += _ =>
        {
            ConnectionState = BlingoNetConnectionState.Disconnected;
            return Task.CompletedTask;
        };

        _connection.On<RNetCommand>("Command", cmd => NetCommandReceived?.Invoke(cmd));

        ConnectionState = BlingoNetConnectionState.Connecting;
        await _connection.StartAsync(ct).ConfigureAwait(false);
        await _connection.InvokeAsync("SessionHello", hello, ct).ConfigureAwait(false);
        ConnectionState = BlingoNetConnectionState.Connected;
    }

    /// <inheritdoc />
    public async Task DisconnectAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync().ConfigureAwait(false);
            _connection = null;
            ConnectionState = BlingoNetConnectionState.Disconnected;
        }
    }

    /// <inheritdoc />
    public IAsyncEnumerable<StageFrameDto> StreamFramesAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.StreamAsync<StageFrameDto>("StreamFrames", ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<SpriteDeltaDto> StreamDeltasAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.StreamAsync<SpriteDeltaDto>("StreamDeltas", ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<KeyframeDto> StreamKeyframesAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.StreamAsync<KeyframeDto>("StreamKeyframes", ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<FilmLoopDto> StreamFilmLoopsAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.StreamAsync<FilmLoopDto>("StreamFilmLoops", ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<SoundEventDto> StreamSoundsAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.StreamAsync<SoundEventDto>("StreamSounds", ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<TempoDto> StreamTemposAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.StreamAsync<TempoDto>("StreamTempos", ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<ColorPaletteDto> StreamColorPalettesAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.StreamAsync<ColorPaletteDto>("StreamColorPalettes", ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<FrameScriptDto> StreamFrameScriptsAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.StreamAsync<FrameScriptDto>("StreamFrameScripts", ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<TransitionDto> StreamTransitionsAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.StreamAsync<TransitionDto>("StreamTransitions", ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<RNetMemberPropertyDto> StreamMemberPropertiesAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.StreamAsync<RNetMemberPropertyDto>("StreamMemberProperties", ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<RNetMoviePropertyDto> StreamMoviePropertiesAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.StreamAsync<RNetMoviePropertyDto>("StreamMovieProperties", ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<RNetStagePropertyDto> StreamStagePropertiesAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.StreamAsync<RNetStagePropertyDto>("StreamStageProperties", ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<RNetSpriteCollectionEventDto> StreamSpriteCollectionEventsAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.StreamAsync<RNetSpriteCollectionEventDto>("StreamSpriteCollectionEvents", ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<TextStyleDto> StreamTextStylesAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.StreamAsync<TextStyleDto>("StreamTextStyles", ct);
    }

    /// <inheritdoc />
    public Task<MovieStateDto> GetMovieSnapshotAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.InvokeAsync<MovieStateDto>("GetMovieSnapshot", ct);
    }
    /// <inheritdoc />
    public Task<BlingoProjectJsonDto> GetCurrentProjectAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return _connection!.InvokeAsync<BlingoProjectJsonDto>("GetCurrentProject", ct);
    }

    /// <inheritdoc />
    public Task SendCommandAsync(RNetCommand cmd, CancellationToken ct = default)
    {
        EnsureConnected();
        var type = cmd.GetType();
        var payload = JsonSerializer.SerializeToElement(cmd, type, CommandSerializerOptions);
        var envelope = new CommandEnvelope(type.Name, payload);
        return _connection!.InvokeAsync("SendCommand", envelope, ct).WaitAsync(TimeSpan.FromSeconds(5), ct);
    }

    /// <inheritdoc />
    public Task SendHeartbeatAsync(TimeSpan? timeout = null, CancellationToken ct = default)
    {
        EnsureConnected();
        var invoke = _connection!.InvokeAsync("Heartbeat", ct);
        return timeout.HasValue ? invoke.WaitAsync(timeout.Value, ct) : invoke;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync().ConfigureAwait(false);
    }

    private void EnsureConnected()
    {
        if (_connection is null)
        {
            throw new InvalidOperationException("Not connected.");
        }
    }

}

