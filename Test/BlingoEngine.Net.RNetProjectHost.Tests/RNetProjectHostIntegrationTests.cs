using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using BlingoEngine.Core;
using BlingoEngine.Net.RNetContracts;
using BlingoEngine.Net.RNetHost.Common;
using BlingoEngine.Net.RNetProjectClient;
using BlingoEngine.Net.RNetProjectHost;
using BlingoEngine.Net.RNetProjectHost.Tests.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace BlingoEngine.Net.RNetProjectHost.Tests;

public class RNetProjectHostIntegrationTests
{
    private static int _portSeed = 63000;

    [Theory]
    [MemberData(nameof(CommandData))]
    public Task HttpClientAndServer_RelayAllCommands(RNetCommand command)
        => WithServerAndClientAsync(async (scenario, token) =>
        {
            var tcs = new TaskCompletionSource<IRNetCommand>(TaskCreationOptions.RunContinuationsAsynchronously);
            void Handler(IRNetCommand cmd) => tcs.TrySetResult(cmd);
            scenario.Server.NetCommandReceived += Handler;

            try
            {
                await scenario.Client.SendCommandAsync(command, token);
                var received = await tcs.Task.WaitAsync(token);
                var typed = Assert.IsAssignableFrom<RNetCommand>(received);
                Assert.IsType(command.GetType(), typed);
                Assert.Equal(command, typed);
            }
            finally
            {
                scenario.Server.NetCommandReceived -= Handler;
            }
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

    private sealed record HttpScenario(BlingoRNetProjectClient Client, TestProjectBus Bus, RNetProjectServer Server);

    private static async Task WithServerAndClientAsync(Func<HttpScenario, CancellationToken, Task> testBody)
    {
        var services = new ServiceCollection();
        var bus = new TestProjectBus();
        services.AddSingleton<IRNetProjectBus>(bus);
        services.AddSingleton<IBlingoPlayer>(new FakeBlingoPlayer());
        services.AddSingleton<IRNetPublisherEngineBridge>(new FakePublisher());
        using var provider = services.BuildServiceProvider();

        var port = Interlocked.Increment(ref _portSeed);
        var server = new RNetProjectServer(new TestConfig(port), NullLogger<RNetProjectServer>.Instance, provider);
        await server.StartAsync();

        var client = new BlingoRNetProjectClient();
        var hello = new HelloDto("test-project", "client-id", "1.0", "HttpTest");
        var uri = new Uri($"http://localhost:{port}/director");

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        try
        {
            await ConnectWithRetryAsync(client, uri, hello, cts.Token);
            var scenario = new HttpScenario(client, bus, server);
            await testBody(scenario, cts.Token);
        }
        finally
        {
            await client.DisposeAsync();
            await server.StopAsync();
        }
    }

    private static async Task ConnectWithRetryAsync(BlingoRNetProjectClient client, Uri uri, HelloDto hello, CancellationToken ct)
    {
        const int maxAttempts = 5;
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            try
            {
                await client.ConnectAsync(uri, hello, ct);
                return;
            }
            catch (HttpRequestException) when (attempt < maxAttempts - 1)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50), ct);
            }
            catch (SocketException) when (attempt < maxAttempts - 1)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50), ct);
            }
        }
    }
}
