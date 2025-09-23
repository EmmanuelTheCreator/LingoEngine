# RNet

RNet (short for **Remote Net**) is BlingoEngine's remote control and observation protocol. It lets a tooling process watch frames, sprites, sounds, and property changes coming from a running movie while sending commands such as property edits or frame jumps back to the host. Multiple transports are supported—today SignalR over HTTP and a low-latency pipe transport—so RNet can be embedded into desktop tools, headless renderers, or network services while reusing the same contracts and client logic.

## Architecture at a Glance

The ecosystem is composed of a set of focused projects that you can mix and match:

1. **Contracts** – The common DTOs, commands, and configuration abstractions that describe what flows across the wire.
2. **Publishers** – Helpers that subscribe to a live `IBlingoPlayer`, watch the engine's events, and push DTOs into channels.
3. **Hosts** – Servers that expose those channels over a specific transport (SignalR, pipes, etc.).
4. **Clients** – Implementations of the shared client interface that consume the streams and send commands back.
5. **Runtimes & Tools** – Higher-level helpers and applications that assemble the pieces (e.g., the RNet Terminal, the client player).

The following diagram summarizes the typical flow for the SignalR transport:

```
BlingoEngine runtime ──▶ Publisher (Project/Pipe) ──▶ Bus channels ──▶ Host (Hub/Server)
         ▲                                                           │
         │                                                           ▼
   Command queue ◀────────────────────────────── Clients (Terminal, custom apps)
```

The pipe transport mirrors the same shape but swaps the SignalR hub for a duplex pipe reader/writer. Both transports implement the same `IBlingoRNetClient` interface, so tooling code can stay transport-agnostic.

## Package Reference

### BlingoEngine.Net.RNetContracts

*Defines the shared language spoken by all RNet components.*

- DTOs describing frames, sprite deltas, film loops, transitions, tempo changes, sound events, text styles, and more live under this project. (see [StageFrameDto.cs](../../src/Net/BlingoEngine.Net.RNetContracts/StageFrameDto.cs#L1-L12), [SpriteDeltaDto.cs](../../src/Net/BlingoEngine.Net.RNetContracts/SpriteDeltaDto.cs#L1-L33))
- `RNetCommand` and its derived records (`SetSpritePropCmd`, `SetMemberPropCmd`, `SetCastPropCmd`, `GoToFrameCmd`, `RewindCmd`, `PauseCmd`, `ResumeCmd`) capture the write-side surface area for tooling commands alongside the `RNetMemberTypeDto` and `RNetSpriteTypeDto` enums used for member and sprite mutations. (see [RNetCommand.cs](../../src/Net/BlingoEngine.Net.RNetContracts/RNetCommand.cs#L1-L79))
- `IRNetConfiguration` and `RNetConfiguration` provide a simple options object (port, autostart flag, client name) shared by both HTTP and pipe hosts/clients. (see [IRNetConfiguration.cs](../../src/Net/BlingoEngine.Net.RNetContracts/IRNetConfiguration.cs#L1-L12), [RNetConfiguration.cs](../../src/Net/BlingoEngine.Net.RNetContracts/RNetConfiguration.cs#L1-L13))
- `IRNetPublisher` defines the methods a publisher must expose for the engine to push updates into the transport-agnostic bus. (see [IRNetPublisher.cs](../../src/Net/BlingoEngine.Net.RNetContracts/IRNetPublisher.cs#L1-L59))

### BlingoEngine.Net.RNetHost.Common

*Infrastructure shared by the host implementations.*

- `IRNetPublisherEngineBridge` extends `IRNetPublisher` with `Enable`/`Disable` hooks so a publisher can subscribe to an `IBlingoPlayer` at runtime. (see [IRNetPublisherEngineBridge.cs](../../src/Net/BlingoEngine.Net.RNetHost.Common/IRNetPublisherEngineBridge.cs#L1-L13))
- `RNetPublisherBase` implements the heavy lifting for tracking sprite/member/movie/stage property changes, queueing them, and flushing through the bus channels while also reacting to cast library and movie lifecycle events. (see [RNetPublisherBase.cs](../../src/Net/BlingoEngine.Net.RNetHost.Common/RNetPublisherBase.cs#L23-L155))
- Helpers such as `DtoExtensions` and `IBlingoRNetServer` (not shown) provide glue so the host servers can raise connection state events and expose the active publisher instance. (see [IBlingoRNetServer.cs](../../src/Net/BlingoEngine.Net.RNetHost.Common/IBlingoRNetServer.cs#L1-L27))

### BlingoEngine.Net.RNetProjectHost

*SignalR/HTTP host used by the Director tooling and most remote sessions.*

- `BlingoRNetProjectHostSetup.WithRNetProjectHostServer` wires up the host inside engine registration, registering the bus, publisher, and server and optionally autostarting once the engine is built. (see [RNetProjectHostSetup.cs](../../src/Net/BlingoEngine.Net.RNetProjectHost/RNetProjectHostSetup.cs#L21-L41))
- `RNetProjectServer` self-hosts ASP.NET Core, exposing the hub at `/director`, managing connection state, and piping inbound commands back to the publisher via a bounded channel. (see [RNetProjectServer.cs](../../src/Net/BlingoEngine.Net.RNetProjectHost/RNetProjectServer.cs#L20-L174))
- `RNetProjectBus` is the set of channels linking the publisher to the hub; each DTO type has its own bounded queue tuned for that payload. (see [RNetProjectBus.cs](../../src/Net/BlingoEngine.Net.RNetProjectHost/RNetProjectBus.cs#L6-L117))
- `BlingoRNetProjectHub` is the SignalR hub that streams frames, deltas, and property updates to clients and accepts commands, heartbeats, and snapshot requests in return. (see [RNetProjectHub.cs](../../src/Net/BlingoEngine.Net.RNetProjectHost/RNetProjectHub.cs#L35-L155))
- `RNetProjectPublisher` (not shown) derives from `RNetPublisherBase` to push data into the `IRNetProjectBus` and drain command queues back into the active `IBlingoPlayer`.

### BlingoEngine.Net.RNetPipeServer

*Pipe-based host for tooling scenarios where HTTP is undesirable.*

- `WithRNetPipeHostServer` mirrors the SignalR setup helper but registers `IRNetPipeServer`, `IRNetPipeBus`, and the pipe publisher instead. (see [BlingoRNetPipeHostSetup.cs](../../src/Net/BlingoEngine.Net.RNetPipeServer/BlingoRNetPipeHostSetup.cs#L21-L37))
- `RNetPipeServer` listens on named pipes (Windows) or Unix domain sockets (macOS/Linux), decoding framed JSON messages, multiplexing streams, and raising connection state events just like the SignalR server. (see [RNetPipeServer.cs](../../src/Net/BlingoEngine.Net.RNetPipeServer/RNetPipeServer.cs#L42-L198))
- `RNetPipeBus` defines the same set of bounded channels as the SignalR bus so the publishers can stay transport-agnostic. (see [RNetPipeBus.cs](../../src/Net/BlingoEngine.Net.RNetPipeServer/RNetPipeBus.cs#L6-L61))
- `RNetPipePublisher` (derived from `RNetPublisherBase`) writes DTOs onto those channels and consumes commands coming back from the pipe reader.

### BlingoEngine.Net.RNetServer

*A standalone ASP.NET Core relay used when hosts and clients cannot connect directly.*

- `Program.cs` boots a minimal web application that maps a SignalR hub at `/rnet` and a simple health-check controller at `/` for diagnostics. (see [Program.cs](../../src/Net/BlingoEngine.Net.RNetServer/Program.cs#L1-L11), [HomeController.cs](../../src/Net/BlingoEngine.Net.RNetServer/Controllers/HomeController.cs#L1-L20))
- `ProjectRelayHub` tracks active project hosts and their clients, forwards broadcast events from the host to every registered client, and relays commands back to the host connection ID. (see [ProjectRelayHub.cs](../../src/Net/BlingoEngine.Net.RNetServer/Hubs/ProjectRelayHub.cs#L16-L75))
- `ProjectRegistry` stores the mapping between project names, the active host connection, and the connected client IDs. (see [ProjectRegistry.cs](../../src/Net/BlingoEngine.Net.RNetServer/ProjectRegistry.cs#L1-L12))

This relay is optional; the standard `RNetProjectServer` already exposes `/director`. The relay becomes useful when multiple remote tools need to share a hosted movie through a central message broker.

### BlingoEngine.Net.RNetClient.Common

*Transport-agnostic client contract.*

- `IBlingoRNetClient` expresses everything a client must do: connect with a `HelloDto`, stream the various DTO feeds, request snapshots, send commands, and emit heartbeats. (see [IBlingoRNetClient.cs](../../src/Net/BlingoEngine.Net.RNetClient.Common/IBlingoRNetClient.cs#L13-L85))
- Tooling code written against this interface can swap in either the SignalR or pipe implementation without code changes.

### BlingoEngine.Net.RNetProjectClient

*SignalR client implementation.*

- `BlingoRNetProjectClient` builds a `HubConnection`, subscribes to connection state callbacks, exposes the stream APIs, and forwards commands via `InvokeAsync`. It also implements automatic reconnection so transient network failures surface as state changes instead of exceptions. (see [BlingoRNetProjectClient.cs](../../src/Net/BlingoEngine.Net.RNetProjectClient/BlingoRNetProjectClient.cs#L63-L199))
- Default configuration values (port 61699, sample client name) are provided for convenience, but you can inject your own `IRNetConfiguration` to control these settings. (see [BlingoRNetProjectClient.cs](../../src/Net/BlingoEngine.Net.RNetProjectClient/BlingoRNetProjectClient.cs#L30-L35))

### BlingoEngine.Net.RNetPipeClient

*Named pipe / Unix socket client implementation.*

- `RNetPipeClient` connects to `pipe://` URIs, resolves the platform-specific endpoint (named pipes on Windows, Unix sockets elsewhere), and pumps JSON payloads through asynchronous channels mirroring the SignalR client APIs. (see [RNetPipeClient.cs](../../src/Net/BlingoEngine.Net.RNetPipeClient/RNetPipeClient.cs#L1-L160))
- Each inbound payload type has a dedicated channel writer so back-pressure can be applied independently; commands and snapshots are handled via task completions that resolve when the matching response arrives. (see [RNetPipeClient.cs](../../src/Net/BlingoEngine.Net.RNetPipeClient/RNetPipeClient.cs#L55-L87), [RNetPipeClient.cs](../../src/Net/BlingoEngine.Net.RNetPipeClient/RNetPipeClient.cs#L309-L337))
- Heartbeats and commands share the same framing logic, keeping pipe sessions alive without relying on HTTP infrastructure. (see [RNetPipeClient.cs](../../src/Net/BlingoEngine.Net.RNetPipeClient/RNetPipeClient.cs#L353-L372))

### BlingoEngine.Net.RNetClientPlayer

*Bridges an RNet client with a local `IBlingoPlayer`.*

- `BlingoRNetClientPlayer` accepts any `IBlingoRNetProjectClient`, subscribes to every stream (frames, deltas, film loops, sounds, tempos, transitions, properties, sprite events, text styles), and applies the updates through `RNetClientPlayerApplier`. (see [BlingoRNetClientPlayer.cs](../../src/Net/BlingoEngine.Net.RNetClientPlayer/BlingoRNetClientPlayer.cs#L18-L178))
- This class is ideal for building headless renderers or regression bots that need to stay synchronized with a remote host but run their own playback locally.

### BlingoEngine.Net.RNetTerminal

*Interactive console tool for development and diagnostics.*

- `RNetTerminalConnection` centralizes connection management, background streaming tasks, heartbeat timers, and outgoing command queues. It exposes `QueueGoToFrameCommand`, `QueueSpritePropertyChange`, and `QueueMemberPropertyChange` (accepting `RNetSpriteTypeDto`/`BlingoMemberTypeDTO`) so the UI can stay thin. (see [RNetTerminalConnection.cs](../../src/Net/BlingoEngine.Net.RNetTerminal/RNetTerminalConnection.cs#L21-L146))
- The terminal respects both HTTP and pipe transports through `RNetTerminalTransport` and builds the correct URI automatically, keeping the rest of the UI agnostic to the transport mechanics. (see [RNetTerminalConnection.cs](../../src/Net/BlingoEngine.Net.RNetTerminal/RNetTerminalConnection.cs#L13-L162))
- `TerminalDataStore` (not shown) coordinates sprite/member state, ensuring that in remote mode UI edits are deferred until the host confirms them, while `BlingoRNetTerminal` wires everything into the `Terminal.Gui` front end.

### Native Interop Sample

- The `src/Net/cpp/BlingoEngine.RNetProjectClient` folder contains a minimal C++ client that exercises the same protocol from native code. Use it as a template when integrating RNet with legacy tools or custom engines.

## Working with RNet

### Hosting the SignalR server inside your application

```csharp
using BlingoEngine.Net.RNetProjectHost;
using BlingoEngine.Setup;

var registration = BlingoEngineSetup.Create()
    .WithRNetProjectHostServer(port: 7000, autoStart: true);

using var engine = registration.Build();
// The host starts automatically because of autoStart; otherwise call
// engine.Services.GetRequiredService<IRNetProjectServer>().StartAsync();
```

- The helper registers `IRNetConfiguration`, `IRNetProjectServer`, the publisher, and the bus, so nothing else is required beyond calling `WithRNetProjectHostServer` during setup. (see [RNetProjectHostSetup.cs](../../src/Net/BlingoEngine.Net.RNetProjectHost/RNetProjectHostSetup.cs#L23-L28))
- When `autoStart` (or `IRNetConfiguration.AutoStartRNetHostOnStartup`) is `true`, the post-build action starts the server and enables the publisher as soon as the engine finishes building. (see [RNetProjectHostSetup.cs](../../src/Net/BlingoEngine.Net.RNetProjectHost/RNetProjectHostSetup.cs#L30-L40))
- To stop the host manually, resolve `IRNetProjectServer` and call `StopAsync()`. Connection state changes are surfaced through `ConnectionStatusChanged` so you can update UI or logs accordingly. (see [RNetProjectServer.cs](../../src/Net/BlingoEngine.Net.RNetProjectHost/RNetProjectServer.cs#L37-L78))

### Hosting the pipe transport

```csharp
using BlingoEngine.Net.RNetPipeServer;
using BlingoEngine.Setup;

var registration = BlingoEngineSetup.Create()
    .WithRNetPipeHostServer(port: 9001, autoStart: true);

using var engine = registration.Build();
```

- Pipes are ideal for local-only tooling or platforms where HTTP is too heavyweight. The helper registers `IRNetPipeServer`, `IRNetPipeBus`, and the pipe publisher for you. (see [BlingoRNetPipeHostSetup.cs](../../src/Net/BlingoEngine.Net.RNetPipeServer/BlingoRNetPipeHostSetup.cs#L23-L34))
- Windows builds use named pipes derived from the port value; Unix platforms derive a socket path. From the client side you simply connect to `pipe://localhost:9001/` and the implementation handles the OS-specific plumbing. (see [RNetPipeClient.cs](../../src/Net/BlingoEngine.Net.RNetPipeClient/RNetPipeClient.cs#L63-L103))

### Connecting from tooling

#### Using the SignalR client

```csharp
using BlingoEngine.Net.RNetProjectClient;
using BlingoEngine.Net.RNetContracts;

var client = new BlingoRNetProjectClient();
await client.ConnectAsync(new Uri("http://localhost:7000/director"),
    new HelloDto("sample-project", "custom-tool", "1.0", "My tool"));

await foreach (var frame in client.StreamFramesAsync())
{
    Console.WriteLine($"Frame {frame.FrameNumber} has {frame.Sprites.Length} sprites");
}
```

- Every client starts by sending a `HelloDto` so the host knows who connected. (see [BlingoRNetProjectClient.cs](../../src/Net/BlingoEngine.Net.RNetProjectClient/BlingoRNetProjectClient.cs#L93-L96))
- All stream methods return `IAsyncEnumerable<T>` and accept cancellation tokens, so you can coordinate graceful shutdowns or back-pressure naturally.
- Commands such as `SetSpritePropCmd` are sent with `SendCommandAsync`, and heartbeats are optional but recommended to keep sessions alive behind proxies. (see [IBlingoRNetClient.cs](../../src/Net/BlingoEngine.Net.RNetClient.Common/IBlingoRNetClient.cs#L80-L84))

#### Using the pipe client

```csharp
using BlingoEngine.Net.RNetPipeClient;
using BlingoEngine.Net.RNetContracts;

var client = new RNetPipeClient();
await client.ConnectAsync(new Uri("pipe://localhost:9001/"),
    new HelloDto("sample-project", "pipe-tool", "1.0", "Pipe client"));
```

- The remainder of the API mirrors the SignalR client. Because the transport is message-based, heartbeats (`SendHeartbeatAsync`) are particularly important to detect broken pipe connections. (see [RNetPipeClient.cs](../../src/Net/BlingoEngine.Net.RNetPipeClient/RNetPipeClient.cs#L353-L372))

### Driving a local player from a remote host

```csharp
var player = /* resolve or create IBlingoPlayer */;
var client = new BlingoRNetProjectClient();
await using var remote = new BlingoRNetClientPlayer(client, player);
await remote.ConnectAsync(new Uri("http://localhost:7000/director"),
    new HelloDto("project", "client-player", "1.0", "Sync bot"));
```

- `BlingoRNetClientPlayer` starts a background pump that consumes every stream concurrently and applies the updates through `RNetClientPlayerApplier`. This keeps the local player synchronized with the host's state without manual wiring. (see [BlingoRNetClientPlayer.cs](../../src/Net/BlingoEngine.Net.RNetClientPlayer/BlingoRNetClientPlayer.cs#L48-L178))

### Debugging with the RNet Terminal

- Launch `BlingoEngine.Net.RNetTerminal` from the command line; the startup dialog now offers dedicated buttons for HTTP or pipe connections. Transport choices are persisted via `RNetTerminalSettings` so the next session remembers your preference. (see [RNetTerminalConnection.cs](../../src/Net/BlingoEngine.Net.RNetTerminal/RNetTerminalConnection.cs#L13-L162), [RNetTerminalSettings.cs](../../src/Net/BlingoEngine.Net.RNetTerminal/RNetTerminalSettings.cs#L6-L44))
- Once connected, the terminal streams frames, sprite deltas, and property updates through `RNetTerminalConnection`. UI edits queue commands via `QueueSpritePropertyChange`/`QueueMemberPropertyChange` and wait for the host to echo the change before updating the display, ensuring the UI always reflects authoritative remote state. (see [RNetTerminalConnection.cs](../../src/Net/BlingoEngine.Net.RNetTerminal/RNetTerminalConnection.cs#L120-L140), [RNetTerminalConnection.cs](../../src/Net/BlingoEngine.Net.RNetTerminal/RNetTerminalConnection.cs#L246-L317))
- Clicking in the score sends `GoToFrameCmd` messages so the host moves to the selected frame, keeping both sides in sync. (see [RNetTerminalConnection.cs](../../src/Net/BlingoEngine.Net.RNetTerminal/RNetTerminalConnection.cs#L109-L118))

## Choosing the Right Transport

| Scenario | Recommended Transport | Reason |
| --- | --- | --- |
| Cross-machine debugging, remote QA, or cloud-hosted projects | SignalR (`BlingoEngine.Net.RNetProjectHost` + `BlingoRNetProjectClient`) | Works over HTTP/S, supports automatic reconnection, easy to deploy alongside existing web infrastructure. (see [RNetProjectServer.cs](../../src/Net/BlingoEngine.Net.RNetProjectHost/RNetProjectServer.cs#L52-L98)) |
| Local editor tooling, high-frequency property scrubbing, or air-gapped machines | Pipe (`BlingoEngine.Net.RNetPipeServer` + `RNetPipeClient`) | Avoids HTTP overhead, uses OS-level sockets/pipes for lower latency, no firewall configuration required. (see [RNetPipeServer.cs](../../src/Net/BlingoEngine.Net.RNetPipeServer/RNetPipeServer.cs#L42-L144), [RNetPipeClient.cs](../../src/Net/BlingoEngine.Net.RNetPipeClient/RNetPipeClient.cs#L63-L125)) |
| Multi-tenant relay where hosts/clients discover each other dynamically | `BlingoEngine.Net.RNetServer` | Provides a hub that keeps a registry of named projects and forwards payloads between them. (see [ProjectRelayHub.cs](../../src/Net/BlingoEngine.Net.RNetServer/Hubs/ProjectRelayHub.cs#L16-L75)) |

## Advanced Topics

- **Command processing** – Publishers call `TryDrainCommands` each frame (or on a timer) to apply queued commands back into the engine. Implementations typically pass a delegate that switches on `RNetCommand` types and updates sprites, members, or playback state accordingly. (see [RNetPublisherBase.cs](../../src/Net/BlingoEngine.Net.RNetHost.Common/RNetPublisherBase.cs#L187-L197))
- **Property coalescing** – `RNetPublisherBase` batches sprite/member/movie/stage property notifications so flurries of property changes during a single tick collapse into a single DTO per unique key before being flushed to clients. (see [RNetPublisherBase.cs](../../src/Net/BlingoEngine.Net.RNetHost.Common/RNetPublisherBase.cs#L23-L121))
- **Snapshots and project export** – Clients can call `GetMovieSnapshotAsync` or `GetCurrentProjectAsync` to retrieve the current playhead state or full serialized project. The SignalR hub answers these by interrogating the active `IBlingoMovie` and using `JsonStateRepository` to serialize the project graph. (see [RNetProjectHub.cs](../../src/Net/BlingoEngine.Net.RNetProjectHost/RNetProjectHub.cs#L63-L91))
- **Heartbeats** – Both transports expose `SendHeartbeatAsync` to keep the session alive. The hub tracks the last heartbeat timestamp per connection and can disconnect idle sessions server-side if desired. (see [RNetProjectHub.cs](../../src/Net/BlingoEngine.Net.RNetProjectHost/RNetProjectHub.cs#L51-L58))
- **Extensibility** – To add a new transport, implement `IBlingoRNetServer`/`IRNetPublisherEngineBridge` on the host side and `IBlingoRNetClient` on the client side. Because DTOs and commands are shared, your new transport immediately works with existing tooling like the RNet Terminal or client player.

## Related Resources

- `src/Net/README.md` – High-level overview of the RNet directory structure and a short connection example. (see [README.md](../../src/Net/README.md#L1-L30))
- `docs/design/Architecture.md` – Broader engine architecture context when embedding RNet in larger applications.
- `src/Net/cpp/BlingoEngine.RNetProjectClient/README.md` – Native interop notes for the C++ sample client.


