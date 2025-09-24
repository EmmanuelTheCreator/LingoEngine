using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using BlingoEngine.Core;
using BlingoEngine.Net.RNetContracts;
using BlingoEngine.IO;
using BlingoEngine.Movies;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace BlingoEngine.Net.RNetProjectHost;

/// <summary>
/// SignalR hub exposing the debugging stream.
/// </summary>
public sealed class BlingoRNetProjectHub : Hub
{
    private readonly IRNetProjectBus _bus;
    private readonly IBlingoPlayer _player;
    private readonly ILogger<BlingoRNetProjectHub> _logger;
    private static readonly ConcurrentDictionary<string, DateTime> _heartbeats = new();

    private static readonly JsonSerializerOptions _commandSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly IReadOnlyDictionary<string, Type> _commandTypes = new Dictionary<string, Type>(StringComparer.Ordinal)
    {
        [nameof(SetSpritePropCmd)] = typeof(SetSpritePropCmd),
        [nameof(SetMemberPropCmd)] = typeof(SetMemberPropCmd),
        [nameof(SetCastPropCmd)] = typeof(SetCastPropCmd),
        [nameof(GoToFrameCmd)] = typeof(GoToFrameCmd),
        [nameof(RewindCmd)] = typeof(RewindCmd),
        [nameof(PauseCmd)] = typeof(PauseCmd),
        [nameof(ResumeCmd)] = typeof(ResumeCmd)
    };

    /// <summary>Initializes a new instance of the <see cref="BlingoRNetProjectHub"/> class.</summary>
    public BlingoRNetProjectHub(IRNetProjectBus bus, IBlingoPlayer player, ILogger<BlingoRNetProjectHub> logger)
    {
        _bus = bus;
        _player = player;
        _logger = logger;
    }


    #region Client->Server
    /// <summary>
    /// Initializes a new session.
    /// </summary>
    public Task SessionHello(HelloDto dto)
    {
        _logger.LogInformation("Client connected: {Client}", dto.ClientName);
        _heartbeats[Context.ConnectionId] = DateTime.UtcNow;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Receives a command from a client.
    /// </summary>
    public Task SendCommand(JsonElement payload)
    {
        if (!payload.TryGetProperty("type", out var typeProperty) || typeProperty.ValueKind != JsonValueKind.String)
        {
            throw new HubException("Command payload missing type discriminator.");
        }

        var typeName = typeProperty.GetString();
        if (string.IsNullOrWhiteSpace(typeName) || !_commandTypes.TryGetValue(typeName, out var commandType))
        {
            throw new HubException($"Unknown command type '{typeName}'.");
        }

        if (!payload.TryGetProperty("payload", out var payloadProperty))
        {
            throw new HubException("Command payload missing payload body.");
        }

        try
        {
            if (payloadProperty.Deserialize(commandType, _commandSerializerOptions) is not IRNetCommand cmd)
            {
                throw new HubException($"Command payload could not be deserialized as '{typeName}'.");
            }

            _bus.Commands.Writer.TryWrite(cmd);
        }
        catch (HubException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize command payload for type {Type}", typeName);
            throw new HubException($"Command payload could not be deserialized as '{typeName}'.", ex);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Receives a heartbeat message to keep the session alive.
    /// </summary>
    public Task Heartbeat()
    {
        _heartbeats[Context.ConnectionId] = DateTime.UtcNow;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns a snapshot of the current movie state.
    /// </summary>
    public Task<MovieStateDto> GetMovieSnapshot()
    {
        if (_player.ActiveMovie is not null)
        {
            return Task.FromResult(new MovieStateDto(
                Frame: _player.ActiveMovie.CurrentFrame,
                Tempo: _player.ActiveMovie.Tempo,
                IsPlaying: _player.ActiveMovie.IsPlaying));
        }
        return Task.FromResult(new MovieStateDto(0, 0, false));
    }
    public async Task<BlingoProjectJsonDto> GetCurrentProject()
    {
        try
        {
            if (_player.ActiveMovie == null)
                return new BlingoProjectJsonDto("");
            var result1 = await _player.RunOnUIThreadAsync<string>(() =>
            {
                var json = new JsonStateRepository().SerializeProject((BlingoPlayer)_player, new JsonStateRepository.MovieStoreOptions { });
                return json;
            }, CancellationToken.None);
            return new BlingoProjectJsonDto(result1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error serializing movie");
            return new BlingoProjectJsonDto("");
        }

    }

    #endregion


    #region Server->Client (streams)

    /// <summary>
    /// Streams stage frame snapshots to the connected client.
    /// </summary>
    public async IAsyncEnumerable<StageFrameDto> StreamFrames([EnumeratorCancellation] CancellationToken ct)
    {
        var reader = _bus.Frames.Reader;
        while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            while (reader.TryRead(out var frame))
            {
                yield return frame;
            }
        }
    }

    /// <summary>
    /// Streams sprite deltas to the connected client.
    /// </summary>
    public async IAsyncEnumerable<SpriteDeltaDto> StreamDeltas([EnumeratorCancellation] CancellationToken ct)
    {
        var reader = _bus.Deltas.Reader;
        while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            while (reader.TryRead(out var delta))
            {
                yield return delta;
            }
        }
    }

    /// <summary>
    /// Streams keyframe information to the connected client.
    /// </summary>
    public async IAsyncEnumerable<KeyframeDto> StreamKeyframes([EnumeratorCancellation] CancellationToken ct)
    {
        var reader = _bus.Keyframes.Reader;
        while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            while (reader.TryRead(out var keyframe))
            {
                yield return keyframe;
            }
        }
    }

    /// <summary>
    /// Streams film loop state changes to the connected client.
    /// </summary>
    public async IAsyncEnumerable<FilmLoopDto> StreamFilmLoops([EnumeratorCancellation] CancellationToken ct)
    {
        var reader = _bus.FilmLoops.Reader;
        while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            while (reader.TryRead(out var loop))
            {
                yield return loop;
            }
        }
    }

    /// <summary>
    /// Streams sound events to the connected client.
    /// </summary>
    public async IAsyncEnumerable<SoundEventDto> StreamSounds([EnumeratorCancellation] CancellationToken ct)
    {
        var reader = _bus.Sounds.Reader;
        while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            while (reader.TryRead(out var snd))
            {
                yield return snd;
            }
        }
    }

    /// <summary>
    /// Streams tempo changes to the connected client.
    /// </summary>
    public async IAsyncEnumerable<TempoDto> StreamTempos([EnumeratorCancellation] CancellationToken ct)
    {
        var reader = _bus.Tempos.Reader;
        while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            while (reader.TryRead(out var tempo))
            {
                yield return tempo;
            }
        }
    }

    /// <summary>
    /// Streams color palette updates to the connected client.
    /// </summary>
    public async IAsyncEnumerable<ColorPaletteDto> StreamColorPalettes([EnumeratorCancellation] CancellationToken ct)
    {
        var reader = _bus.ColorPalettes.Reader;
        while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            while (reader.TryRead(out var palette))
            {
                yield return palette;
            }
        }
    }

    /// <summary>
    /// Streams frame scripts to the connected client.
    /// </summary>
    public async IAsyncEnumerable<FrameScriptDto> StreamFrameScripts([EnumeratorCancellation] CancellationToken ct)
    {
        var reader = _bus.FrameScripts.Reader;
        while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            while (reader.TryRead(out var script))
            {
                yield return script;
            }
        }
    }

    /// <summary>
    /// Streams transitions to the connected client.
    /// </summary>
    public async IAsyncEnumerable<TransitionDto> StreamTransitions([EnumeratorCancellation] CancellationToken ct)
    {
        var reader = _bus.Transitions.Reader;
        while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            while (reader.TryRead(out var transition))
            {
                yield return transition;
            }
        }
    }

    /// <summary>
    /// Streams member property updates to the connected client.
    /// </summary>
    public async IAsyncEnumerable<RNetMemberPropertyDto> StreamMemberProperties([EnumeratorCancellation] CancellationToken ct)
    {
        var reader = _bus.MemberProperties.Reader;
        while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            while (reader.TryRead(out var prop))
            {
                yield return prop;
            }
        }
    }

    /// <summary>
    /// Streams movie property updates to the connected client.
    /// </summary>
    public async IAsyncEnumerable<RNetMoviePropertyDto> StreamMovieProperties([EnumeratorCancellation] CancellationToken ct)
    {
        var reader = _bus.MovieProperties.Reader;
        while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            while (reader.TryRead(out var prop))
            {
                yield return prop;
            }
        }
    }

    /// <summary>
    /// Streams stage property updates to the connected client.
    /// </summary>
    public async IAsyncEnumerable<RNetStagePropertyDto> StreamStageProperties([EnumeratorCancellation] CancellationToken ct)
    {
        var reader = _bus.StageProperties.Reader;
        while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            while (reader.TryRead(out var prop))
            {
                yield return prop;
            }
        }
    }

    /// <summary>
    /// Streams sprite collection change events to the connected client.
    /// </summary>
    public async IAsyncEnumerable<RNetSpriteCollectionEventDto> StreamSpriteCollectionEvents([EnumeratorCancellation] CancellationToken ct)
    {
        var reader = _bus.SpriteCollectionEvents.Reader;
        while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            while (reader.TryRead(out var evt))
            {
                yield return evt;
            }
        }
    }

    /// <summary>
    /// Streams text style updates to the connected client.
    /// </summary>
    public async IAsyncEnumerable<TextStyleDto> StreamTextStyles([EnumeratorCancellation] CancellationToken ct)
    {
        var reader = _bus.TextStyles.Reader;
        while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            while (reader.TryRead(out var style))
            {
                yield return style;
            }
        }
    }
    #endregion


}

