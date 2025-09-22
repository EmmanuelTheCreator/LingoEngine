using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using BlingoEngine.Net.RNetClient.Common;
using BlingoEngine.Net.RNetContracts;

namespace BlingoEngine.Net.RNetPipeClient;

/// <summary>
/// Pipe-based implementation of <see cref="IBlingoRNetClient"/>.
/// </summary>
public interface IBlingoRNetPipeClient : IBlingoRNetClient { }

/// <inheritdoc />
public sealed class RNetPipeClient : IBlingoRNetPipeClient
{
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private readonly Dictionary<string, Func<string, bool>> _inboundHandlers;
    private readonly Dictionary<string, Type> _commandTypes;
    private readonly object _connectionSync = new();

    private Channel<StageFrameDto>? _frames;
    private Channel<SpriteDeltaDto>? _deltas;
    private Channel<KeyframeDto>? _keyframes;
    private Channel<TempoDto>? _tempos;
    private Channel<ColorPaletteDto>? _palettes;
    private Channel<FrameScriptDto>? _frameScripts;
    private Channel<TransitionDto>? _transitions;
    private Channel<RNetMemberPropertyDto>? _memberProperties;
    private Channel<RNetMoviePropertyDto>? _movieProperties;
    private Channel<RNetStagePropertyDto>? _stageProperties;
    private Channel<RNetSpriteCollectionEventDto>? _spriteEvents;
    private Channel<TextStyleDto>? _textStyles;
    private Channel<FilmLoopDto>? _filmLoops;
    private Channel<SoundEventDto>? _sounds;

    private Stream? _stream;
    private CancellationTokenSource? _readerCts;
    private Task? _readLoop;
    private TaskCompletionSource<MovieStateDto>? _pendingMovieSnapshot;
    private TaskCompletionSource<BlingoProjectJsonDto>? _pendingProject;
    private TaskCompletionSource<bool>? _pendingHeartbeat;

    private BlingoNetConnectionState _state = BlingoNetConnectionState.Disconnected;

    public RNetPipeClient()
    {
        _inboundHandlers = new Dictionary<string, Func<string, bool>>(StringComparer.Ordinal)
        {
            ["StageFrame"] = json => TryWrite(_frames, json),
            ["SpriteDelta"] = json => TryWrite(_deltas, json),
            ["Keyframe"] = json => TryWrite(_keyframes, json),
            ["Tempo"] = json => TryWrite(_tempos, json),
            ["ColorPalette"] = json => TryWrite(_palettes, json),
            ["FrameScript"] = json => TryWrite(_frameScripts, json),
            ["Transition"] = json => TryWrite(_transitions, json),
            ["MemberProperty"] = json => TryWrite(_memberProperties, json),
            ["MovieProperty"] = json => TryWrite(_movieProperties, json),
            ["StageProperty"] = json => TryWrite(_stageProperties, json),
            ["SpriteCollectionEvent"] = json => TryWrite(_spriteEvents, json),
            ["TextStyle"] = json => TryWrite(_textStyles, json),
            ["FilmLoop"] = json => TryWrite(_filmLoops, json),
            ["SoundEvent"] = json => TryWrite(_sounds, json),
            ["MovieSnapshot"] = HandleMovieSnapshot,
            ["CurrentProject"] = HandleProject,
            ["HeartbeatAck"] = HandleHeartbeat,
            ["Command"] = HandleCommand
        };

        _commandTypes = new Dictionary<string, Type>(StringComparer.Ordinal)
        {
            [nameof(SetSpritePropCmd)] = typeof(SetSpritePropCmd),
            [nameof(SetMemberPropCmd)] = typeof(SetMemberPropCmd),
            [nameof(SetCastPropCmd)] = typeof(SetCastPropCmd),
            [nameof(GoToFrameCmd)] = typeof(GoToFrameCmd),
            [nameof(PauseCmd)] = typeof(PauseCmd),
            [nameof(ResumeCmd)] = typeof(ResumeCmd)
        };
    }

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
        if (hubUrl is null)
        {
            throw new ArgumentNullException(nameof(hubUrl));
        }

        lock (_connectionSync)
        {
            if (_stream is not null)
            {
                return;
            }

            ResetChannels();
            ConnectionState = BlingoNetConnectionState.Connecting;
        }

        Stream stream;
        if (OperatingSystem.IsWindows())
        {
            var pipeName = ResolvePipeName(hubUrl);
            var client = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            await client.ConnectAsync(ct).ConfigureAwait(false);
            stream = client;
        }
        else
        {
            var socketPath = ResolveSocketPath(hubUrl);
            var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            var endpoint = new UnixDomainSocketEndPoint(socketPath);
            await socket.ConnectAsync(endpoint, ct).ConfigureAwait(false);
            stream = new NetworkStream(socket, ownsSocket: true);
        }

        var readerCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

        lock (_connectionSync)
        {
            _stream = stream;
            _readerCts = readerCts;
            _readLoop = Task.Run(() => ReadLoopAsync(readerCts.Token), readerCts.Token);
        }

        await SendAsync("SessionHello", hello, ct).ConfigureAwait(false);
        ConnectionState = BlingoNetConnectionState.Connected;
    }

    /// <inheritdoc />
    public async Task DisconnectAsync()
    {
        Stream? stream;
        CancellationTokenSource? readerCts;
        Task? readLoop;

        lock (_connectionSync)
        {
            stream = _stream;
            readerCts = _readerCts;
            readLoop = _readLoop;
            _stream = null;
            _readerCts = null;
            _readLoop = null;
        }

        if (stream is null)
        {
            return;
        }

        try
        {
            readerCts?.Cancel();
            if (readLoop is not null)
            {
                try
                {
                    await readLoop.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // Ignore cancellation during shutdown.
                }
            }
        }
        finally
        {
            readerCts?.Dispose();
            stream.Dispose();
            ConnectionState = BlingoNetConnectionState.Disconnected;
            FailPendingRequests(new InvalidOperationException("Connection closed."));
            CompleteAllChannels();
        }
    }

    /// <inheritdoc />
    public IAsyncEnumerable<StageFrameDto> StreamFramesAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return ReadChannelAsync(_frames!, ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<SpriteDeltaDto> StreamDeltasAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return ReadChannelAsync(_deltas!, ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<KeyframeDto> StreamKeyframesAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return ReadChannelAsync(_keyframes!, ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<TempoDto> StreamTemposAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return ReadChannelAsync(_tempos!, ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<ColorPaletteDto> StreamColorPalettesAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return ReadChannelAsync(_palettes!, ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<FrameScriptDto> StreamFrameScriptsAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return ReadChannelAsync(_frameScripts!, ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<TransitionDto> StreamTransitionsAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return ReadChannelAsync(_transitions!, ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<RNetMemberPropertyDto> StreamMemberPropertiesAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return ReadChannelAsync(_memberProperties!, ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<RNetMoviePropertyDto> StreamMoviePropertiesAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return ReadChannelAsync(_movieProperties!, ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<RNetStagePropertyDto> StreamStagePropertiesAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return ReadChannelAsync(_stageProperties!, ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<RNetSpriteCollectionEventDto> StreamSpriteCollectionEventsAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return ReadChannelAsync(_spriteEvents!, ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<TextStyleDto> StreamTextStylesAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return ReadChannelAsync(_textStyles!, ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<FilmLoopDto> StreamFilmLoopsAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return ReadChannelAsync(_filmLoops!, ct);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<SoundEventDto> StreamSoundsAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        return ReadChannelAsync(_sounds!, ct);
    }

    /// <inheritdoc />
    public async Task<MovieStateDto> GetMovieSnapshotAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        var tcs = CreatePending(ref _pendingMovieSnapshot, ct);
        await SendRawAsync("GetMovieSnapshot", "null", ct).ConfigureAwait(false);
        try
        {
            return await tcs.Task.WaitAsync(ct).ConfigureAwait(false);
        }
        finally
        {
            Interlocked.CompareExchange(ref _pendingMovieSnapshot, null, tcs);
        }
    }

    /// <inheritdoc />
    public async Task<BlingoProjectJsonDto> GetCurrentProjectAsync(CancellationToken ct = default)
    {
        EnsureConnected();
        var tcs = CreatePending(ref _pendingProject, ct);
        await SendRawAsync("GetCurrentProject", "null", ct).ConfigureAwait(false);
        try
        {
            return await tcs.Task.WaitAsync(ct).ConfigureAwait(false);
        }
        finally
        {
            Interlocked.CompareExchange(ref _pendingProject, null, tcs);
        }
    }

    /// <inheritdoc />
    public Task SendCommandAsync(RNetCommand cmd, CancellationToken ct = default)
    {
        EnsureConnected();
        if (cmd is null)
        {
            throw new ArgumentNullException(nameof(cmd));
        }

        return SendAsync(cmd.GetType().Name, cmd, ct);
    }

    /// <inheritdoc />
    public async Task SendHeartbeatAsync(TimeSpan? timeout = null, CancellationToken ct = default)
    {
        EnsureConnected();
        var tcs = CreatePending(ref _pendingHeartbeat, ct);
        await SendRawAsync("Heartbeat", "null", ct).ConfigureAwait(false);

        try
        {
            if (timeout.HasValue)
            {
                await tcs.Task.WaitAsync(timeout.Value, ct).ConfigureAwait(false);
            }
            else
            {
                await tcs.Task.WaitAsync(ct).ConfigureAwait(false);
            }
        }
        finally
        {
            Interlocked.CompareExchange(ref _pendingHeartbeat, null, tcs);
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync().ConfigureAwait(false);
        _sendLock.Dispose();
    }

    private void EnsureConnected()
    {
        if (_stream is null || !IsConnected)
        {
            throw new InvalidOperationException("Not connected.");
        }
    }

    private static Channel<T> CreateChannel<T>()
        => Channel.CreateUnbounded<T>(new UnboundedChannelOptions
        {
            SingleReader = false,
            SingleWriter = true
        });

    private void ResetChannels()
    {
        _frames = CreateChannel<StageFrameDto>();
        _deltas = CreateChannel<SpriteDeltaDto>();
        _keyframes = CreateChannel<KeyframeDto>();
        _tempos = CreateChannel<TempoDto>();
        _palettes = CreateChannel<ColorPaletteDto>();
        _frameScripts = CreateChannel<FrameScriptDto>();
        _transitions = CreateChannel<TransitionDto>();
        _memberProperties = CreateChannel<RNetMemberPropertyDto>();
        _movieProperties = CreateChannel<RNetMoviePropertyDto>();
        _stageProperties = CreateChannel<RNetStagePropertyDto>();
        _spriteEvents = CreateChannel<RNetSpriteCollectionEventDto>();
        _textStyles = CreateChannel<TextStyleDto>();
        _filmLoops = CreateChannel<FilmLoopDto>();
        _sounds = CreateChannel<SoundEventDto>();
    }

    private void CompleteAllChannels()
    {
        CompleteChannel(ref _frames);
        CompleteChannel(ref _deltas);
        CompleteChannel(ref _keyframes);
        CompleteChannel(ref _tempos);
        CompleteChannel(ref _palettes);
        CompleteChannel(ref _frameScripts);
        CompleteChannel(ref _transitions);
        CompleteChannel(ref _memberProperties);
        CompleteChannel(ref _movieProperties);
        CompleteChannel(ref _stageProperties);
        CompleteChannel(ref _spriteEvents);
        CompleteChannel(ref _textStyles);
        CompleteChannel(ref _filmLoops);
        CompleteChannel(ref _sounds);
    }

    private static void CompleteChannel<T>(ref Channel<T>? channel)
    {
        channel?.Writer.TryComplete();
        channel = null;
    }

    private async Task ReadLoopAsync(CancellationToken ct)
    {
        try
        {
            if (_stream is null)
            {
                return;
            }

            using var reader = new StreamReader(_stream, Encoding.UTF8, leaveOpen: true);

            while (!ct.IsCancellationRequested)
            {
                var header = await reader.ReadLineAsync(ct).ConfigureAwait(false);
                if (header is null)
                {
                    break;
                }

                var parts = header.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2 || !int.TryParse(parts[1], out var length))
                {
                    continue;
                }

                var buffer = new char[length];
                var read = 0;
                while (read < length)
                {
                    var r = await reader.ReadAsync(buffer.AsMemory(read, length - read), ct).ConfigureAwait(false);
                    if (r == 0)
                    {
                        break;
                    }

                    read += r;
                }

                if (read != length)
                {
                    break;
                }

                var json = new string(buffer);
                HandleInbound(parts[0], json);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Expected during shutdown.
        }
        catch (Exception ex)
        {
            FailPendingRequests(ex);
        }
        finally
        {
            ConnectionState = BlingoNetConnectionState.Disconnected;
            FailPendingRequests(new InvalidOperationException("Connection closed."));
            CompleteAllChannels();
        }
    }

    private void HandleInbound(string command, string json)
    {
        if (_inboundHandlers.TryGetValue(command, out var handler))
        {
            if (!handler(json))
            {
                FailPendingRequests(new InvalidOperationException($"Failed to process '{command}' message."));
            }
        }
    }

    private bool TryWrite<T>(Channel<T>? channel, string json)
    {
        if (channel is null)
        {
            return false;
        }

        try
        {
            if (JsonSerializer.Deserialize<T>(json, _jsonOptions) is { } value)
            {
                return channel.Writer.TryWrite(value);
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private bool HandleMovieSnapshot(string json)
    {
        try
        {
            var dto = JsonSerializer.Deserialize<MovieStateDto>(json, _jsonOptions);
            if (dto is not null)
            {
                var tcs = Interlocked.Exchange(ref _pendingMovieSnapshot, null);
                tcs?.TrySetResult(dto);
                return true;
            }
        }
        catch
        {
            // Ignore malformed responses.
        }

        return false;
    }

    private bool HandleProject(string json)
    {
        try
        {
            var dto = JsonSerializer.Deserialize<BlingoProjectJsonDto>(json, _jsonOptions);
            if (dto is not null)
            {
                var tcs = Interlocked.Exchange(ref _pendingProject, null);
                tcs?.TrySetResult(dto);
                return true;
            }
        }
        catch
        {
            // Ignore malformed responses.
        }

        return false;
    }

    private bool HandleHeartbeat(string json)
    {
        var tcs = Interlocked.Exchange(ref _pendingHeartbeat, null);
        tcs?.TrySetResult(true);
        return true;
    }

    private bool HandleCommand(string json)
    {
        try
        {
            var envelope = JsonSerializer.Deserialize<CommandEnvelope>(json, _jsonOptions);
            if (envelope is null || string.IsNullOrEmpty(envelope.Type) || envelope.Payload is null)
            {
                return false;
            }

            if (!_commandTypes.TryGetValue(envelope.Type, out var type))
            {
                return false;
            }

            var cmd = (IRNetCommand?)JsonSerializer.Deserialize(envelope.Payload, type, _jsonOptions);
            if (cmd is not null)
            {
                NetCommandReceived?.Invoke(cmd);
                return true;
            }
        }
        catch
        {
            // Ignore deserialization failures.
        }

        return false;
    }

    private record CommandEnvelope(string Type, string Payload);

    private Task SendAsync<T>(string command, T payload, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(payload, _jsonOptions);
        return SendRawAsync(command, json, ct);
    }

    private async Task SendRawAsync(string command, string json, CancellationToken ct)
    {
        Stream? stream;
        lock (_connectionSync)
        {
            stream = _stream;
        }

        if (stream is null)
        {
            throw new InvalidOperationException("Not connected.");
        }

        await _sendLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var header = $"{command} {Encoding.UTF8.GetByteCount(json)}\n";
            var headerBytes = Encoding.UTF8.GetBytes(header);
            await stream.WriteAsync(headerBytes, ct).ConfigureAwait(false);
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            await stream.WriteAsync(jsonBytes, ct).ConfigureAwait(false);
            await stream.FlushAsync(ct).ConfigureAwait(false);
        }
        finally
        {
            _sendLock.Release();
        }
    }

    private static async IAsyncEnumerable<T> ReadChannelAsync<T>(Channel<T> channel, [EnumeratorCancellation] CancellationToken ct)
    {
        var reader = channel.Reader;
        while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            while (reader.TryRead(out var item))
            {
                yield return item;
            }
        }
    }

    private static string ResolvePipeName(Uri uri)
    {
        if (!string.Equals(uri.Scheme, "pipe", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Pipe URIs must use the pipe:// scheme.", nameof(uri));
        }

        var path = uri.AbsolutePath.Trim('/');
        if (!string.IsNullOrWhiteSpace(path))
        {
            return path.Replace('/', '.');
        }

        if (uri.Port > 0)
        {
            return $"lingo-rnet-{uri.Port}";
        }

        throw new ArgumentException("Pipe URI must include a path or port.", nameof(uri));
    }

    private static string ResolveSocketPath(Uri uri)
    {
        var path = uri.AbsolutePath;
        if (!string.IsNullOrWhiteSpace(path) && path != "/")
        {
            return path;
        }

        if (uri.Port > 0)
        {
            return Path.Combine(Path.GetTempPath(), $"lingo-rnet-{uri.Port}.sock");
        }

        throw new ArgumentException("Pipe URI must include a path or port.", nameof(uri));
    }

    private static TaskCompletionSource<T> CreatePending<T>(ref TaskCompletionSource<T>? field, CancellationToken ct)
    {
        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        if (Interlocked.CompareExchange(ref field, tcs, null) is not null)
        {
            throw new InvalidOperationException("Another request is already pending.");
        }

        if (ct.CanBeCanceled)
        {
            ct.Register(state => ((TaskCompletionSource<T>)state!).TrySetCanceled(ct), tcs);
        }

        return tcs;
    }

    private void FailPendingRequests(Exception error)
    {
        var snapshot = Interlocked.Exchange(ref _pendingMovieSnapshot, null);
        snapshot?.TrySetException(error);
        var project = Interlocked.Exchange(ref _pendingProject, null);
        project?.TrySetException(error);
        var heartbeat = Interlocked.Exchange(ref _pendingHeartbeat, null);
        heartbeat?.TrySetException(error);
    }
}

