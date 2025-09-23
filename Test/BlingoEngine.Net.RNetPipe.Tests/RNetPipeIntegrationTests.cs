using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using BlingoEngine.Core;
using BlingoEngine.Net.RNetContracts;
using BlingoEngine.Net.RNetHost.Common;
using BlingoEngine.Net.RNetPipeServer;
using BlingoEngine.Net.RNetPipe.Tests.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using PipeClient = BlingoEngine.Net.RNetPipeClient.RNetPipeClient;
using PipeServer = BlingoEngine.Net.RNetPipeServer.RNetPipeServer;

namespace BlingoEngine.Net.RNetPipe.Tests;

public class RNetPipeIntegrationTests
{
    private static int _portSeed = 62000;

    [Fact]
    public Task PipeClientAndServer_ExchangeFramesAndCommands()
        => WithPipeServerAndClientAsync(async (scenario, token) =>
        {
            var expectedFrame = new StageFrameDto(
                4,
                4,
                42,
                new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                new byte[] { 1, 2, 3, 4 });

            await AssertBusForwardingAsync(
                scenario,
                token,
                expectedFrame,
                (client, ct) => client.StreamFramesAsync(ct),
                (bus, frame, ct) => bus.Frames.Writer.WriteAsync(frame, ct),
                (expected, actual) =>
                {
                    Assert.Equal(expected.Width, actual.Width);
                    Assert.Equal(expected.Height, actual.Height);
                    Assert.Equal(expected.FrameId, actual.FrameId);
                    Assert.Equal(expected.TimestampUtc, actual.TimestampUtc);
                    Assert.Equal(expected.Argb32, actual.Argb32);
                });

            await scenario.Client.SendCommandAsync(new PauseCmd(), token);
            var receivedCommand = await scenario.Bus.Commands.Reader.ReadAsync(token);
            Assert.IsType<PauseCmd>(receivedCommand);
        });

    [Fact]
    public Task PipeClientAndServer_ForwardAllMessagesFromBus()
        => WithPipeServerAndClientAsync(async (scenario, token) =>
        {
            var frame = new StageFrameDto(
                8,
                6,
                7,
                new DateTime(2024, 2, 2, 0, 0, 0, DateTimeKind.Utc),
                new byte[] { 5, 6, 7, 8 });

            await AssertBusForwardingAsync(
                scenario,
                token,
                frame,
                (client, ct) => client.StreamFramesAsync(ct),
                (bus, payload, ct) => bus.Frames.Writer.WriteAsync(payload, ct),
                (expected, actual) =>
                {
                    Assert.Equal(expected.Width, actual.Width);
                    Assert.Equal(expected.Height, actual.Height);
                    Assert.Equal(expected.FrameId, actual.FrameId);
                    Assert.Equal(expected.TimestampUtc, actual.TimestampUtc);
                    Assert.Equal(expected.Argb32, actual.Argb32);
                });

            var delta = new SpriteDeltaDto(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14);
            await AssertBusForwardingAsync(
                scenario,
                token,
                delta,
                (client, ct) => client.StreamDeltasAsync(ct),
                (bus, payload, ct) => bus.Deltas.Writer.WriteAsync(payload, ct));

            var keyframe = new KeyframeDto(5, 2, 3, "Blend", "50");
            await AssertBusForwardingAsync(
                scenario,
                token,
                keyframe,
                (client, ct) => client.StreamKeyframesAsync(ct),
                (bus, payload, ct) => bus.Keyframes.Writer.WriteAsync(payload, ct));

            var filmLoop = new FilmLoopDto(2, 4, 10, 20, true);
            await AssertBusForwardingAsync(
                scenario,
                token,
                filmLoop,
                (client, ct) => client.StreamFilmLoopsAsync(ct),
                (bus, payload, ct) => bus.FilmLoops.Writer.WriteAsync(payload, ct));

            var sound = new SoundEventDto(12, 7, 9, true);
            await AssertBusForwardingAsync(
                scenario,
                token,
                sound,
                (client, ct) => client.StreamSoundsAsync(ct),
                (bus, payload, ct) => bus.Sounds.Writer.WriteAsync(payload, ct));

            var tempo = new TempoDto(15, 120);
            await AssertBusForwardingAsync(
                scenario,
                token,
                tempo,
                (client, ct) => client.StreamTemposAsync(ct),
                (bus, payload, ct) => bus.Tempos.Writer.WriteAsync(payload, ct));

            var palette = new ColorPaletteDto(18, new byte[] { 9, 10, 11, 12 });
            await AssertBusForwardingAsync(
                scenario,
                token,
                palette,
                (client, ct) => client.StreamColorPalettesAsync(ct),
                (bus, payload, ct) => bus.ColorPalettes.Writer.WriteAsync(payload, ct),
                (expected, actual) =>
                {
                    Assert.Equal(expected.Frame, actual.Frame);
                    Assert.Equal(expected.Argb32, actual.Argb32);
                });

            var script = new FrameScriptDto(21, "go to frame 5");
            await AssertBusForwardingAsync(
                scenario,
                token,
                script,
                (client, ct) => client.StreamFrameScriptsAsync(ct),
                (bus, payload, ct) => bus.FrameScripts.Writer.WriteAsync(payload, ct));

            var transition = new TransitionDto(24, "crossfade", 6);
            await AssertBusForwardingAsync(
                scenario,
                token,
                transition,
                (client, ct) => client.StreamTransitionsAsync(ct),
                (bus, payload, ct) => bus.Transitions.Writer.WriteAsync(payload, ct));

            var memberProperty = new RNetMemberPropertyDto(3, 8, "Name", "Hero");
            await AssertBusForwardingAsync(
                scenario,
                token,
                memberProperty,
                (client, ct) => client.StreamMemberPropertiesAsync(ct),
                (bus, payload, ct) => bus.MemberProperties.Writer.WriteAsync(payload, ct));

            var textStyle = new TextStyleDto(3, 8, 0, 5, "FontWeight", "Bold");
            await AssertBusForwardingAsync(
                scenario,
                token,
                textStyle,
                (client, ct) => client.StreamTextStylesAsync(ct),
                (bus, payload, ct) => bus.TextStyles.Writer.WriteAsync(payload, ct));

            var movieProperty = new RNetMoviePropertyDto("Duration", "90");
            await AssertBusForwardingAsync(
                scenario,
                token,
                movieProperty,
                (client, ct) => client.StreamMoviePropertiesAsync(ct),
                (bus, payload, ct) => bus.MovieProperties.Writer.WriteAsync(payload, ct));

            var stageProperty = new RNetStagePropertyDto("BackColor", "#112233");
            await AssertBusForwardingAsync(
                scenario,
                token,
                stageProperty,
                (client, ct) => client.StreamStagePropertiesAsync(ct),
                (bus, payload, ct) => bus.StageProperties.Writer.WriteAsync(payload, ct));

            var spriteEvent = new RNetSpriteCollectionEventDto("Sprite2D", RNetSpriteCollectionEventType.Added, 5, 1, null);
            await AssertBusForwardingAsync(
                scenario,
                token,
                spriteEvent,
                (client, ct) => client.StreamSpriteCollectionEventsAsync(ct),
                (bus, payload, ct) => bus.SpriteCollectionEvents.Writer.WriteAsync(payload, ct));
        });

    [Theory]
    [MemberData(nameof(CommandData))]
    public Task PipeClientAndServer_RelayAllCommandTypes(RNetCommand command)
        => WithPipeServerAndClientAsync(async (scenario, token) =>
        {
            await scenario.Client.SendCommandAsync(command, token);
            var received = await scenario.Bus.Commands.Reader.ReadAsync(token);
            var actual = Assert.IsAssignableFrom<RNetCommand>(received);
            Assert.IsType(command.GetType(), actual);
            Assert.Equal(command, actual);
        });

    public static IEnumerable<object[]> CommandData()
    {
        yield return new object[] { new SetSpritePropCmd(3, 1, RNetSpriteTypeDto.Sprite2D, "LocH", "123") };
        yield return new object[] { new SetMemberPropCmd(1, 5, RNetMemberTypeDto.Bitmap, "Name", "MyBitmap") };
        yield return new object[] { new SetCastPropCmd(2, "Title", "Cast A") };
        yield return new object[] { new GoToFrameCmd(42) };
        yield return new object[] { new RewindCmd() };
        yield return new object[] { new PauseCmd() };
        yield return new object[] { new ResumeCmd() };
    }

    private sealed record PipeScenario(PipeClient Client, TestPipeBus Bus);

    private static async Task WithPipeServerAndClientAsync(Func<PipeScenario, CancellationToken, Task> testBody)
    {
        var services = new ServiceCollection();
        var bus = new TestPipeBus();
        services.AddSingleton<IRNetPipeBus>(bus);
        services.AddSingleton<IBlingoPlayer>(new FakeBlingoPlayer());
        services.AddSingleton<IRNetPublisherEngineBridge>(new FakePublisher());
        using var provider = services.BuildServiceProvider();

        var port = Interlocked.Increment(ref _portSeed);
        var server = new PipeServer(new TestConfig(port), NullLogger<PipeServer>.Instance, provider);
        await server.StartAsync();

        var client = new PipeClient();
        var hello = new HelloDto("test-project", "client-id", "1.0", "PipeTest");
        var uri = new Uri($"pipe://localhost:{port}");

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        try
        {
            await ConnectWithRetryAsync(client, uri, hello, cts.Token);
            var scenario = new PipeScenario(client, bus);
            await testBody(scenario, cts.Token);
        }
        finally
        {
            await client.DisposeAsync();
            await server.StopAsync();
        }
    }

    private static async Task AssertBusForwardingAsync<T>(
        PipeScenario scenario,
        CancellationToken ct,
        T payload,
        Func<PipeClient, CancellationToken, IAsyncEnumerable<T>> streamFactory,
        Func<TestPipeBus, T, CancellationToken, ValueTask> writeAsync,
        Action<T, T>? assert = null)
    {
        var readTask = ReadFirstAsync(streamFactory(scenario.Client, ct), ct);
        await writeAsync(scenario.Bus, payload, ct);
        var actual = await readTask;
        if (assert is null)
        {
            Assert.Equal(payload, actual);
        }
        else
        {
            assert(payload, actual);
        }
    }

    private static async Task<T> ReadFirstAsync<T>(IAsyncEnumerable<T> source, CancellationToken ct)
    {
        await foreach (var item in source.WithCancellation(ct))
        {
            return item;
        }

        throw new InvalidOperationException("Sequence completed without emitting a value.");
    }

    private static async Task ConnectWithRetryAsync(PipeClient client, Uri uri, HelloDto hello, CancellationToken ct)
    {
        const int maxAttempts = 5;
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            try
            {
                await client.ConnectAsync(uri, hello, ct);
                return;
            }
            catch (SocketException ex) when (attempt < maxAttempts - 1 &&
                (ex.SocketErrorCode == SocketError.AddressNotAvailable || ex.SocketErrorCode == SocketError.ConnectionRefused))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50), ct);
            }
        }
    }
}
