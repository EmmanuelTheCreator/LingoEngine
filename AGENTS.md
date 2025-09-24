# AGENTS

These instructions apply to the entire repository.

## Pre-Start
- **Do not work on `main`.**  
  If the current branch is `main`, stop immediately and instruct the user to switch to another branch (e.g. `develop`).

## Environment
- The project requires **.NET 8 (LTS)** for Godot and **.NET 9** for Blazor.
- If the `dotnet` CLI isn't available, run `./scripts/install-packages-linux.sh`.  
  This script will:
  - Install required system packages (SDL2, X11/GL, etc.).
  - Install .NET SDKs (8 and 9) into `$HOME/.dotnet`.
  - Install the `dotnet-format` tool.
  - Download and link the Godot Mono editor/runtime.
- Ensure `$HOME/.dotnet` and `$HOME/.dotnet/tools` and `$HOME/.local/bin` are on your `PATH`.

## Testing
- Run tests only for the projects affected by your changes; do not run the entire solution.
- For changes in core engine code (e.g., under `src` or `Test/BlingoEngine.Lingo.Core.Tests`), run `dotnet test Test/BlingoEngine.Lingo.Core.Tests/BlingoEngine.Lingo.Core.Tests.csproj`.
- For changes in the ProjectorRays area (`WillMoveToOwnRepo/ProjectorRays`), run `dotnet test WillMoveToOwnRepo/ProjectorRays/Test/ProjectorRays.DotNet.Test/ProjectorRays.DotNet.Test.csproj`.
- Apply the same approach for other components: run `dotnet test <path-to-test-project>` for each modified project.
- Project `BlingoEngine.SDL2.GfxVisualTest.csproj` is a console application to test the UI visually, not with tests in it.
 - Other visual test projects such as `WillMoveToOwnRepo/AbstUI/Test/AbstUI.GfxVisualTest.*` are also manual console apps; build or run them with `dotnet build`/`dotnet run` rather than `dotnet test`.

## Code Style
 - Use `dotnet format` to fix style issues when needed.
  - Format only the project that owns your changes:

    `dotnet format <path/to/project.csproj> --include <relative/path/to/file.cs> -v diagnostic`
- Prefer `rg` (ripgrep) over `grep` for searching the codebase.
- Do not remove existing comments from code.
- When writing new classes, place members in the order: fields, then properties, then constructors.
- Avoid adding business logic or default implementations inside interfaces to preserve .NET Framework 4.8 compatibility.
- When introducing new reusable test utilities or fakes, place their implementations under the test project's `/Fakes/` directory so future tests can consume them.

## Project Structure

| Path | Description |
| --- | --- |
| src/BlingoEngine/BlingoEngine.csproj | Core engine functionality and dependency injection setup |
| src/BlingoEngine.Lingo.Core/BlingoEngine.Lingo.Core.csproj | Runtime implementation of the Lingo scripting language |
| src/BlingoEngine.IO/BlingoEngine.IO.csproj | File and resource I/O built on BlingoEngine |
| src/BlingoEngine.IO.Data/BlingoEngine.IO.Data.csproj | Shared data structures for the I/O layer |
| src/BlingoEngine.SDL2/BlingoEngine.SDL2.csproj | SDL2 bindings and rendering support |
| src/BlingoEngine.Unity/BlingoEngine.Unity.csproj | Unity engine integration layer |
| src/BlingoEngine.LGodot/BlingoEngine.LGodot.csproj | Godot engine integration layer |
| src/BlingoEngine.3D.Core/BlingoEngine.3D.Core.csproj | Core components for 3D features |
| src/BlingoEngine.Blazor/BlingoEngine.Blazor.csproj | Blazor integration layer |
| src/BlingoEngine.VerboseLanguage/BlingoEngine.VerboseLanguage.csproj | Verbose fluent API for the Lingo language |
| src/Director/BlingoEngine.Director.Core/BlingoEngine.Director.Core.csproj | Editor tooling reminiscent of Macromedia Director |
| src/Net/BlingoEngine.Net.RNetContracts/BlingoEngine.Net.RNetContracts.csproj | Shared contracts for RNet tooling |
| src/Net/BlingoEngine.Net.RNetClient/BlingoEngine.Net.RNetClient.csproj | Client library for RNet tooling |
| src/Net/BlingoEngine.Net.RNetHost/BlingoEngine.Net.RNetHost.csproj | SignalR host for RNet tooling |
| src/Net/BlingoEngine.Net.RNetHost.Common/BlingoEngine.Net.RNetHost.Common.csproj | Shared helpers for hosting RNet transports |
| src/Net/BlingoEngine.Net.RNetTerminal/BlingoEngine.Net.RNetTerminal.csproj | Console app for RNet client debugging |
| src/Director/BlingoEngine.Director.SDL2/BlingoEngine.Director.SDL2.csproj | SDL2 integration for Director tooling |
| src/Director/BlingoEngine.Director.LGodot/BlingoEngine.Director.LGodot.csproj | Godot integration for Director tooling |
| src/Director/BlingoEngine.Director.Runner.SDL2/BlingoEngine.Director.Runner.SDL2.csproj | Standalone SDL2 runner for Director tooling |
| src/Director/BlingoEngine.Director.Runner.LGodot/BlingoEngine.Director.Runner.LGodot.csproj | Standalone Godot runner for Director tooling |
| Demo/TetriGrounds/BlingoEngine.Demo.TetriGrounds.Core/BlingoEngine.Demo.TetriGrounds.Core.csproj | Shared code for the TetriGrounds demo |
| Demo/TetriGrounds/BlingoEngine.Demo.TetriGrounds.SDL2/BlingoEngine.Demo.TetriGrounds.SDL2.csproj | SDL2-based TetriGrounds demo |
| Demo/TetriGrounds/BlingoEngine.Demo.TetriGrounds.Blazor/BlingoEngine.Demo.TetriGrounds.Blazor.csproj | Blazor-based TetriGrounds demo |
| Demo/TetriGrounds/BlingoEngine.Demo.TetriGrounds.Godot/BlingoEngine.Demo.TetriGrounds.Godot.csproj | Godot-based TetriGrounds demo |
| Test/BlingoEngine.Lingo.Core.Tests/BlingoEngine.Lingo.Core.Tests.csproj | Tests for the Lingo scripting runtime |
| Test/BlingoEngine.Lingo.Tests/BlingoEngine.Lingo.Tests.csproj | Tests for BlingoEngine core features |
| Test/BlingoEngine.Tests/BlingoEngine.Tests.csproj | Tests for additional BlingoEngine features |
| Test/BlingoEngine.SDL2.GfxVisualTest/BlingoEngine.SDL2.GfxVisualTest.csproj | Console app for manual SDL2 graphics checks |
| WillMoveToOwnRepo/AbstUI/src/AbstUI/AbstUI.csproj | Core abstractions for the AbstUI framework |
| WillMoveToOwnRepo/AbstUI/src/AbstUI.SDL2/AbstUI.SDL2.csproj | SDL2 backend for AbstUI |
| WillMoveToOwnRepo/AbstUI/src/AbstUI.LUnity/AbstUI.LUnity.csproj | Unity backend for AbstUI |
| WillMoveToOwnRepo/AbstUI/src/AbstUI.LGodot/AbstUI.LGodot.csproj | Godot backend for AbstUI |
| WillMoveToOwnRepo/AbstUI/src/AbstUI.ImGui/AbstUI.ImGui.csproj | ImGui backend for AbstUI |
| WillMoveToOwnRepo/AbstUI/src/AbstUI.Blazor/AbstUI.Blazor.csproj | Blazor backend for AbstUI |
| WillMoveToOwnRepo/AbstUI/src/AbstUI.SDL2.FFmpeg/AbstUI.SDL2.FFmpeg.csproj | FFmpeg-based media playback for the SDL2 backend |
| WillMoveToOwnRepo/AbstUI/src/AbstUI.SDL2.Vlc/AbstUI.SDL2.Vlc.csproj | LibVLC-based media playback for the SDL2 backend |
| WillMoveToOwnRepo/AbstUI/src/AbstUI.SDL2RmlUi/AbstUI.SDL2RmlUi.csproj | SDL2 backend using RmlUi.NET |
| WillMoveToOwnRepo/AbstUI/Test/AbstUI.GfxVisualTest/AbstUI.GfxVisualTest.csproj | Shared graphics visual test utilities for AbstUI |
| WillMoveToOwnRepo/AbstUI/Test/AbstUI.GfxVisualTest.Blazor/AbstUI.GfxVisualTest.Blazor.csproj | Blazor visual test application for AbstUI |
| WillMoveToOwnRepo/AbstUI/Test/AbstUI.GfxVisualTest.LGodot/AbstUI.GfxVisualTest.LGodot.csproj | Godot visual test application for AbstUI |
| WillMoveToOwnRepo/AbstUI/Test/AbstUI.GfxVisualTest.LUnity/AbstUI.GfxVisualTest.LUnity.csproj | Unity visual test application for AbstUI |
| WillMoveToOwnRepo/AbstUI/Test/AbstUI.GfxVisualTest.ImGui/AbstUI.GfxVisualTest.ImGui.csproj | ImGui visual test application for AbstUI |
| WillMoveToOwnRepo/AbstUI/Test/AbstUI.GfxVisualTest.SDL2/AbstUI.GfxVisualTest.SDL2.csproj | SDL2 visual test application for AbstUI |
| WillMoveToOwnRepo/AbstUI/Test/AbstUI.Tests.Common/AbstUI.Tests.Common.csproj | Shared test utilities for AbstUI |
| WillMoveToOwnRepo/AbstUI/Test/AbstUI.Tests/AbstUI.Tests.csproj | Tests for core AbstUI components |
| WillMoveToOwnRepo/AbstUI/Test/AbstUI.SDLTest/AbstUI.SDLTest.csproj | Tests for the AbstUI SDL2 backend |
| WillMoveToOwnRepo/AbstUI/Test/AbstUI.LGodotTest/AbstUI.LGodotTest.csproj | Tests for the AbstUI Godot backend |
| WillMoveToOwnRepo/ProjectorRays/src/ProjectorRays.DotNet/ProjectorRays.DotNet.csproj | Core ProjectorRays .NET library |
| WillMoveToOwnRepo/ProjectorRays/src/ProjectorRays.Console/ProjectorRays.Console.csproj | Console showcase for ProjectorRays |
| WillMoveToOwnRepo/ProjectorRays/Test/ProjectorRays.DotNet.Test/ProjectorRays.DotNet.Test.csproj | Tests for ProjectorRays library |
| Samples/SetupWays/BlingoEngineMinimalSDL/BlingoEngineMinimalSDL.csproj | Minimal SDL bootstrap sample that renders a centered text sprite |
| Samples/SetupWays/BlingoEngineMinimalGodot/BlingoEngineMinimalGodot.csproj | Minimal Godot sample that launches the engine from a Godot scene |
| Samples/SetupWays/BlingoEngineWithDirectorInDebugSDL/BlingoEngineWithDirectorInDebugSDL.csproj | SDL sample that enables Director tooling in debug builds |
| Samples/SetupWays/BlingoEngineWithDirectorInDebugGodot/BlingoEngineWithDirectorInDebugGodot.csproj | Godot sample that enables Director tooling in debug builds |
## Notes for Agents
- The solution file is `BlingoEngine.sln`. But avoid using it, its big.
- Keep cross-platform compatibility in mind when making changes.

