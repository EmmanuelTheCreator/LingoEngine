using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using BlingoEngine.Core;
using BlingoEngine.IO;
using BlingoEngine.Movies;
using BlingoEngine.Net.RNetContracts;
using BlingoEngine.Net.RNetHost.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BlingoEngine.Net.RNetPipeServer;

/// <summary>
/// Pipe-based implementation of the RNet host.
/// </summary>
/// <inheritdoc />
public sealed class RNetPipeServer : IRNetPipeServer
{
    private readonly IRNetConfiguration _config;
    private readonly ILogger<RNetPipeServer> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };
    private readonly ConcurrentDictionary<string, Type> _commandTypes;
    private CancellationTokenSource? _cts;
    private Task? _listenerTask;
    private IRNetPipeBus? _bus;
    private IBlingoPlayer? _player;
    private IRNetPublisherEngineBridge? _publisher;
    private BlingoNetConnectionState _state = BlingoNetConnectionState.Disconnected;

    public RNetPipeServer(IRNetConfiguration config, ILogger<RNetPipeServer> logger, IServiceProvider serviceProvider)
    {
        _config = config;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _commandTypes = new ConcurrentDictionary<string, Type>(StringComparer.Ordinal)
        {
            [nameof(SetSpritePropCmd)] = typeof(SetSpritePropCmd),
            [nameof(SetMemberPropCmd)] = typeof(SetMemberPropCmd),
            [nameof(SetCastPropCmd)] = typeof(SetCastPropCmd),
            [nameof(GoToFrameCmd)] = typeof(GoToFrameCmd),
            [nameof(RewindCmd)] = typeof(RewindCmd),
            [nameof(PauseCmd)] = typeof(PauseCmd),
            [nameof(ResumeCmd)] = typeof(ResumeCmd)
        };
    }

    public RNetPipeServer(ILogger<RNetPipeServer> logger, IServiceProvider serviceProvider)
        : this(new DefaultConfig(), logger, serviceProvider) { }

    private sealed class DefaultConfig : IRNetConfiguration
    {
        public int Port { get; set; } = 61699;
        public bool AutoStartRNetHostOnStartup { get; set; }
        public string ClientName { get; set; } = "PipeHost";
        public RNetClientType ClientType { get; set; } = RNetClientType.Pipe;
        public RNetRemoteRole RemoteRole { get; set; } = RNetRemoteRole.Host;
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
            _logger.LogInformation("RNet pipe host state: {State}", _state);
        }
    }

    /// <inheritdoc />
    public bool IsEnabled => ConnectionState == BlingoNetConnectionState.Connected;

    /// <inheritdoc />
    public IRNetPublisherEngineBridge Publisher
        => _publisher ??= _serviceProvider.GetRequiredService<IRNetPublisherEngineBridge>();

    /// <inheritdoc />
    public Task StartAsync(CancellationToken ct = default)
    {
        if (_cts is not null)
        {
            return Task.CompletedTask;
        }

        _bus = _serviceProvider.GetRequiredService<IRNetPipeBus>();
        _player = _serviceProvider.GetRequiredService<IBlingoPlayer>();
        _publisher = _serviceProvider.GetRequiredService<IRNetPublisherEngineBridge>();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _listenerTask = Task.Run(() => RunAsync(_cts.Token), _cts.Token);
        ConnectionState = BlingoNetConnectionState.Connected;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task StopAsync()
    {
        if (_cts is null)
        {
            return;
        }

        try
        {
            _cts.Cancel();
            if (_listenerTask is not null)
            {
                try
                {
                    await _listenerTask.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // Ignore cancellation.
                }
            }
        }
        finally
        {
            _cts.Dispose();
            _cts = null;
            _listenerTask = null;
            _bus = null;
            ConnectionState = BlingoNetConnectionState.Disconnected;
        }
    }

    private async Task RunAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await using var connection = await AcceptConnectionAsync(ct).ConfigureAwait(false);
                if (connection is null)
                {
                    return;
                }

                _logger.LogInformation("RNet pipe client connected.");
                await HandleConnectionAsync(connection, ct).ConfigureAwait(false);
                _logger.LogInformation("RNet pipe client disconnected.");
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Pipe server error");
                await Task.Delay(TimeSpan.FromSeconds(1), CancellationToken.None).ConfigureAwait(false);
            }
        }
    }

    private async Task HandleConnectionAsync(PipeConnection connection, CancellationToken ct)
    {
        if (_bus is null)
        {
            return;
        }

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var token = linkedCts.Token;
        var sendTasks = StartSendLoops(connection, _bus, token);
        var readTask = ReadLoopAsync(connection, token);

        try
        {
            await readTask.ConfigureAwait(false);
        }
        finally
        {
            linkedCts.Cancel();
            try
            {
                await Task.WhenAll(sendTasks).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Ignore cancellation
            }
        }
    }

    private Task[] StartSendLoops(PipeConnection connection, IRNetPipeBus bus, CancellationToken ct)
        => new[]
        {
            PumpAsync(connection, bus.Frames.Reader, "StageFrame", ct),
            PumpAsync(connection, bus.Deltas.Reader, "SpriteDelta", ct),
            PumpAsync(connection, bus.Keyframes.Reader, "Keyframe", ct),
            PumpAsync(connection, bus.FilmLoops.Reader, "FilmLoop", ct),
            PumpAsync(connection, bus.Sounds.Reader, "SoundEvent", ct),
            PumpAsync(connection, bus.Tempos.Reader, "Tempo", ct),
            PumpAsync(connection, bus.ColorPalettes.Reader, "ColorPalette", ct),
            PumpAsync(connection, bus.FrameScripts.Reader, "FrameScript", ct),
            PumpAsync(connection, bus.Transitions.Reader, "Transition", ct),
            PumpAsync(connection, bus.MemberProperties.Reader, "MemberProperty", ct),
            PumpAsync(connection, bus.TextStyles.Reader, "TextStyle", ct),
            PumpAsync(connection, bus.MovieProperties.Reader, "MovieProperty", ct),
            PumpAsync(connection, bus.StageProperties.Reader, "StageProperty", ct),
            PumpAsync(connection, bus.SpriteCollectionEvents.Reader, "SpriteCollectionEvent", ct)
        };

    private async Task PumpAsync<T>(PipeConnection connection, ChannelReader<T> reader, string command, CancellationToken ct)
    {
        try
        {
            while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
            {
                while (reader.TryRead(out var item))
                {
                    try
                    {
                        var json = JsonSerializer.Serialize(item, _jsonOptions);
                        await connection.SendRawAsync(command, json, ct).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) when (ct.IsCancellationRequested)
                    {
                        return;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogDebug(ex, "Failed to send {Command} message", command);
                    }
                }
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Expected during shutdown.
        }
    }

    private async Task ReadLoopAsync(PipeConnection connection, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var message = await connection.ReadAsync(ct).ConfigureAwait(false);
            if (message is null)
            {
                break;
            }

            var (command, json) = message.Value;
            try
            {
                if (!await HandleInboundAsync(connection, command, json, ct).ConfigureAwait(false))
                {
                    _logger.LogDebug("Unknown pipe message '{Command}'", command);
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to process pipe message '{Command}'", command);
            }
        }
    }

    private async Task<bool> HandleInboundAsync(PipeConnection connection, string command, string json, CancellationToken ct)
    {
        switch (command)
        {
            case "SessionHello":
                if (JsonSerializer.Deserialize<HelloDto>(json, _jsonOptions) is { } hello)
                {
                    _logger.LogInformation("Client connected: {Client}", hello.ClientName);
                }
                return true;
            case "Heartbeat":
                await connection.SendRawAsync("HeartbeatAck", "null", ct).ConfigureAwait(false);
                return true;
            case "GetMovieSnapshot":
                await SendMovieSnapshotAsync(connection, ct).ConfigureAwait(false);
                return true;
            case "GetCurrentProject":
                await SendCurrentProjectAsync(connection, ct).ConfigureAwait(false);
                return true;
            default:
                if (_commandTypes.TryGetValue(command, out var type) &&
                    JsonSerializer.Deserialize(json, type, _jsonOptions) is IRNetCommand cmd)
                {
                    _bus?.Commands.Writer.TryWrite(cmd);
                    NetCommandReceived?.Invoke(cmd);
                    return true;
                }
                return false;
        }
    }

    private async Task SendMovieSnapshotAsync(PipeConnection connection, CancellationToken ct)
    {
        MovieStateDto dto;
        if (_player?.ActiveMovie is IBlingoMovie movie)
        {
            dto = new MovieStateDto(movie.CurrentFrame, movie.Tempo, movie.IsPlaying);
        }
        else
        {
            dto = new MovieStateDto(0, 0, false);
        }

        var json = JsonSerializer.Serialize(dto, _jsonOptions);
        await connection.SendRawAsync("MovieSnapshot", json, ct).ConfigureAwait(false);
    }

    private async Task SendCurrentProjectAsync(PipeConnection connection, CancellationToken ct)
    {
        if (_player is null)
        {
            await connection.SendRawAsync("CurrentProject", JsonSerializer.Serialize(new BlingoProjectJsonDto(string.Empty), _jsonOptions), ct).ConfigureAwait(false);
            return;
        }

        try
        {
            var json = await _player.RunOnUIThreadAsync<string>(() =>
            {
                var repo = new JsonStateRepository();
                return repo.SerializeProject((BlingoPlayer)_player, new JsonStateRepository.MovieStoreOptions());
            }, ct).ConfigureAwait(false);
            var dto = new BlingoProjectJsonDto(json ?? string.Empty);
            await connection.SendRawAsync("CurrentProject", JsonSerializer.Serialize(dto, _jsonOptions), ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serializing movie");
            await connection.SendRawAsync("CurrentProject", JsonSerializer.Serialize(new BlingoProjectJsonDto(string.Empty), _jsonOptions), ct).ConfigureAwait(false);
        }
    }

    private async Task<PipeConnection?> AcceptConnectionAsync(CancellationToken ct)
    {
        if (OperatingSystem.IsWindows())
        {
            var name = BuildPipeName();
            var server = new NamedPipeServerStream(name, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            try
            {
                await server.WaitForConnectionAsync(ct).ConfigureAwait(false);
                return new PipeConnection(server, null);
            }
            catch
            {
                server.Dispose();
                throw;
            }
        }
        else
        {
            var path = BuildSocketPath();
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            var listener = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            try
            {
                var endpoint = new UnixDomainSocketEndPoint(path);
                listener.Bind(endpoint);
                listener.Listen(1);
                var socket = await listener.AcceptAsync(ct).ConfigureAwait(false);
                return new PipeConnection(new NetworkStream(socket, ownsSocket: true), path);
            }
            finally
            {
                listener.Dispose();
                if (ct.IsCancellationRequested && File.Exists(path))
                {
                    try
                    {
                        File.Delete(path);
                    }
                    catch
                    {
                        // Ignore cleanup failures.
                    }
                }
            }
        }
    }

    private string BuildPipeName() => $"lingo-rnet-{_config.Port}";

    private string BuildSocketPath() => Path.Combine(Path.GetTempPath(), $"lingo-rnet-{_config.Port}.sock");

    private sealed class PipeConnection : IAsyncDisposable
    {
        private readonly Stream _stream;
        private readonly StreamReader _reader;
        private readonly SemaphoreSlim _writeLock = new(1, 1);
        private readonly string? _socketPath;

        public PipeConnection(Stream stream, string? socketPath)
        {
            _stream = stream;
            _reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
            _socketPath = socketPath;
        }

        public async Task<(string Command, string Json)?> ReadAsync(CancellationToken ct)
        {
            var header = await _reader.ReadLineAsync(ct).ConfigureAwait(false);
            if (header is null)
            {
                return null;
            }

            var parts = header.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2 || !int.TryParse(parts[1], out var length))
            {
                return null;
            }

            var buffer = new char[length];
            var read = 0;
            while (read < length)
            {
                var r = await _reader.ReadAsync(buffer.AsMemory(read, length - read), ct).ConfigureAwait(false);
                if (r == 0)
                {
                    return null;
                }

                read += r;
            }

            return (parts[0], new string(buffer));
        }

        public async Task SendRawAsync(string command, string json, CancellationToken ct)
        {
            await _writeLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                var header = $"{command} {Encoding.UTF8.GetByteCount(json)}\n";
                var headerBytes = Encoding.UTF8.GetBytes(header);
                await _stream.WriteAsync(headerBytes, ct).ConfigureAwait(false);
                var jsonBytes = Encoding.UTF8.GetBytes(json);
                await _stream.WriteAsync(jsonBytes, ct).ConfigureAwait(false);
                await _stream.FlushAsync(ct).ConfigureAwait(false);
            }
            finally
            {
                _writeLock.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            _writeLock.Dispose();
            _reader.Dispose();
            await _stream.DisposeAsync().ConfigureAwait(false);
            if (_socketPath is not null && File.Exists(_socketPath))
            {
                try
                {
                    File.Delete(_socketPath);
                }
                catch
                {
                    // Ignore cleanup errors.
                }
            }
        }
    }
}

