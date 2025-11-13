# AGENTS

These instructions apply to the entire repository.

## Pre-Start
- **Do not work on `main`.**  
  If the current branch is `main`, stop immediately and instruct the user to switch to another branch (e.g. `develop`).

## Environment
- The project requires **.NET 8 (LTS)** for Godot targets and **.NET 9** for Blazor targets.
- If the `dotnet` CLI is missing, run `./scripts/install-packages-linux.sh` **and allow it to finish** (it can take 2–3 minutes).
  This script will:
  - Install required system packages (SDL2, X11/GL, etc.).
  - Install .NET SDKs (8 and 9) into `$HOME/.dotnet`.
  - Install the `dotnet-format` tool.
  - Download and link the Godot Mono editor/runtime.
- Ensure `$HOME/.dotnet`, `$HOME/.dotnet/tools`, and `$HOME/.local/bin` are on your `PATH`:
  ```bash
  export PATH="$HOME/.dotnet:$HOME/.dotnet/tools:$HOME/.local/bin:$PATH"
  ```
- The repository specifies SDK requirements through `global.json`; keep it committed and do not bypass it with older SDKs.

## Testing
- Run tests only for the projects affected by your changes; do not run the entire solution.
- For changes in core engine code (e.g., under `src` or `Test/BlingoEngine.Lingo.Core.Tests`), run `dotnet test Test/BlingoEngine.Lingo.Core.Tests/BlingoEngine.Lingo.Core.Tests.csproj`.
- For changes in the ProjectorRays area (`WillMoveToOwnRepo/ProjectorRays`), run `dotnet test WillMoveToOwnRepo/ProjectorRays/Test/ProjectorRays.DotNet.Test/ProjectorRays.DotNet.Test.csproj`.
- Apply the same approach for other components: run `dotnet test <path-to-test-project>` for each modified project.
- Project `BlingoEngine.SDL2.GfxVisualTest.csproj` is a console application to test the UI visually, not with tests in it.
 - Other visual test projects such as `WillMoveToOwnRepo/AbstUI/Test/AbstUI.GfxVisualTest.*` are also manual console apps; build or run them with `dotnet build`/`dotnet run` rather than `dotnet test`.
- Core Lingo tests take roughly 10 seconds with ~126 tests (one skip is expected); other focused test projects generally complete in <5 seconds.
- Unity-related tests are flaky by design; investigate only when specifically working on Unity integration.

## Build and Validation
- Prefer building individual projects over the full solution to save time:
  ```bash
  dotnet build Demo/TetriGrounds/BlingoEngine.Demo.TetriGrounds.SDL2/BlingoEngine.Demo.TetriGrounds.SDL2.csproj
  ```
- Build the entire solution (`dotnet build BlingoEngine.sln`) only when absolutely necessary; it can take 60+ seconds and may surface expected Unity integration errors.
- Manually validate gameplay or UI changes by running the relevant demos, e.g.:
  ```bash
  dotnet run --project Demo/TetriGrounds/BlingoEngine.Demo.TetriGrounds.SDL2/BlingoEngine.Demo.TetriGrounds.SDL2.csproj
  dotnet run --project Demo/TetriGrounds/BlingoEngine.Demo.TetriGrounds.Blazor/BlingoEngine.Demo.TetriGrounds.Blazor.csproj
  ```
- Headless environments may fail due to missing fonts or audio devices; reaching that failure point usually confirms the build succeeded.
- Always run `dotnet format <path/to/project.csproj> --include <relative/path>` before committing if style issues appear.

## Important Files
- `BlingoEngine.sln` – top-level solution (large; avoid unless necessary).
- `global.json` – pins the .NET SDK versions to 8 and 9.
- `AGENTS.md` – canonical contributor guidelines (this file).
- `scripts/install-packages-linux.sh` – installs SDKs, SDL2, Godot, and tooling.

## Common Tasks
1. Create or use a feature branch (never work on `main`).
2. Follow existing architecture patterns (the TetriGrounds demos are good references).
3. Update or add tests in the project you modify.
4. Build and, when possible, run the affected demo or sample to validate behavior.
5. Use `rg` (ripgrep) for searches and `dotnet format` for styling.

## Code Style
- Format only the project that owns your changes:
    `dotnet format <path/to/project.csproj> --include <relative/path/to/file.cs> -v diagnostic`
- Do not remove existing comments from code.
- When writing new classes, place members in the order: fields, then properties, then constructors.
- I repeat: Always put first the fields, then the properties, then the constructor and then the methods.
- Avoid adding business logic or default implementations inside interfaces to preserve .NET Framework 4.8 compatibility.
- When introducing new reusable test utilities or fakes, place their implementations under the test project's `/Fakes/` directory so future tests can consume them.
- When adding reusable fakes or reflection helpers for tests, place deterministic sources under the owning test project's `Fakes/` directory and shared helpers under its `TestUtilities/` directory so future tests can reuse them rather than redefining copies.
- I repeat again: Always put first the fields, then the properties, then the constructor and then the methods.


# VERY IMPORTANT
- If there is only one code line after an `if`, `for`, or similar statement, you may omit the braces `{}`.
This is VERY IMPORTANT.


NEVER DO THIS:
```csharp
if (condition) 
{
      DoSomething();
}
  ```
  ALWAYS DO THIS
```csharp
if (condition) 
      DoSomething();
  ```

## Troubleshooting & Known Issues
- "Framework not found" errors usually mean the setup script has not been run.
- Missing Godot references can be resolved by re-running the setup script or configuring the `GODOT_URL` environment variable.
- Unity integration is incomplete; build or test failures in that area are typically expected.
- Demo runs that fail due to missing fonts or audio still indicate the engine initialized correctly.

## Project Structure

- ### Core Runtime
| Path | Description |
| --- | --- |
| src/BlingoEngine/BlingoEngine.csproj | Core engine functionality and dependency injection setup |
| src/BlingoEngine.Lingo.Core/BlingoEngine.Lingo.Core.csproj | Runtime implementation of the Lingo scripting language |
| src/BlingoEngine.Legacy.Lingo/BlingoEngine.Legacy.Lingo.csproj | Compatibility shims for legacy Lingo behaviors |
| src/BlingoEngine.IO/BlingoEngine.IO.csproj | File and resource I/O built on BlingoEngine |
| src/BlingoEngine.IO.Data/BlingoEngine.IO.Data.csproj | Shared data structures for the I/O layer |
| src/BlingoEngine.IO.Legacy/BlingoEngine.IO.Legacy.csproj | Legacy I/O compatibility helpers |
| src/BlingoEngine.SDL2/BlingoEngine.SDL2.csproj | SDL2 bindings and rendering support |
| src/BlingoEngine.LGodot/BlingoEngine.LGodot.csproj | Godot engine integration layer |
| src/BlingoEngine.Unity/BlingoEngine.Unity.csproj | Unity engine integration layer |
| src/BlingoEngine.Blazor/BlingoEngine.Blazor.csproj | Blazor integration layer |
| src/BlingoEngine.3D.Core/BlingoEngine.3D.Core.csproj | Core components for 3D features |
| src/BlingoEngine.VerboseLanguage/BlingoEngine.VerboseLanguage.csproj | Verbose fluent API for the Lingo language |

- ### Director Tooling
| Path | Description |
| --- | --- |
| src/Director/BlingoEngine.Director.Core/BlingoEngine.Director.Core.csproj | Editor tooling reminiscent of Macromedia Director |
| src/Director/BlingoEngine.Director.SDL2/BlingoEngine.Director.SDL2.csproj | SDL2 integration for Director tooling |
| src/Director/BlingoEngine.Director.LGodot/BlingoEngine.Director.LGodot.csproj | Godot integration for Director tooling |
| src/Director/BlingoEngine.Director.Runner.SDL2/BlingoEngine.Director.Runner.SDL2.csproj | Standalone SDL2 runner for Director tooling |
| src/Director/BlingoEngine.Director.Runner.LGodot/BlingoEngine.Director.Runner.LGodot.csproj | Standalone Godot runner for Director tooling |

- ### Networking (RNet)
| Path | Description |
| --- | --- |
| src/Net/BlingoEngine.Net.RNetContracts/BlingoEngine.Net.RNetContracts.csproj | Shared contracts for RNet tooling |
| src/Net/BlingoEngine.Net.RNetClient.Common/BlingoEngine.Net.RNetClient.Common.csproj | Shared client helpers |
| src/Net/BlingoEngine.Net.RNetClient/BlingoEngine.Net.RNetClient.csproj | Client library for RNet tooling |
| src/Net/BlingoEngine.Net.RNetClientPlayer/BlingoEngine.Net.RNetClientPlayer.csproj | Player-facing client implementation |
| src/Net/BlingoEngine.Net.RNetPipeClient/BlingoEngine.Net.RNetPipeClient.csproj | Named pipe client transport |
| src/Net/BlingoEngine.Net.RNetPipeServer/BlingoEngine.Net.RNetPipeServer.csproj | Named pipe server transport |
| src/Net/BlingoEngine.Net.RNetProjectClient/BlingoEngine.Net.RNetProjectClient.csproj | Project interaction client |
| src/Net/BlingoEngine.Net.RNetProjectHost/BlingoEngine.Net.RNetProjectHost.csproj | Project host implementation |
| src/Net/BlingoEngine.Net.RNetHost/BlingoEngine.Net.RNetHost.csproj | SignalR host for RNet tooling |
| src/Net/BlingoEngine.Net.RNetServer/BlingoEngine.Net.RNetServer.csproj | Shared RNet server hosting |
| src/Net/BlingoEngine.Net.RNetTerminal/BlingoEngine.Net.RNetTerminal.csproj | Console app for RNet client debugging |
| src/Net/BlingoEngine.Net.RNetHost.Common/BlingoEngine.Net.RNetHost.Common.csproj | Shared helpers for hosting RNet transports |

- ### Demos
| Path | Description |
| --- | --- |
| Demo/TetriGrounds/BlingoEngine.Demo.TetriGrounds.Core/BlingoEngine.Demo.TetriGrounds.Core.csproj | Shared code for the TetriGrounds demo |
| Demo/TetriGrounds/BlingoEngine.Demo.TetriGrounds.SDL2/BlingoEngine.Demo.TetriGrounds.SDL2.csproj | SDL2-based TetriGrounds demo |
| Demo/TetriGrounds/BlingoEngine.Demo.TetriGrounds.Blazor/BlingoEngine.Demo.TetriGrounds.Blazor.csproj | Blazor-based TetriGrounds demo |
| Demo/TetriGrounds/BlingoEngine.Demo.TetriGrounds.Godot/BlingoEngine.Demo.TetriGrounds.Godot.csproj | Godot-based TetriGrounds demo |
| Demo/LPacMan/Blingo.PacMan.Core/Blingo.PacMan.Core.csproj | Core logic for the LPacMan demo |
| Demo/LPacMan/Blingo.PacMan.SDL2/Blingo.PacMan.SDL2.csproj | SDL2 frontend for the LPacMan demo |
| Demo/LPacMan/Blingo.PacMan.Godot/Blingo.PacMan.Godot.csproj | Godot frontend for the LPacMan demo |

- ### Samples
| Path | Description |
| --- | --- |
| Samples/SetupWays/BlingoEngineMinimalSDL/BlingoEngineMinimalSDL.csproj | Minimal SDL bootstrap sample that renders a centered text sprite |
| Samples/SetupWays/BlingoEngineMinimalGodot/BlingoEngineMinimalGodot.csproj | Minimal Godot sample that launches the engine from a Godot scene |
| Samples/SetupWays/BlingoEngineWithDirectorInDebugSDL/BlingoEngineWithDirectorInDebugSDL.csproj | SDL sample that enables Director tooling in debug builds |
| Samples/SetupWays/BlingoEngineWithDirectorInDebugGodot/BlingoEngineWithDirectorInDebugGodot.csproj | Godot sample that enables Director tooling in debug builds |

- ### Tests
| Path | Description |
| --- | --- |
| Test/BlingoEngine.Lingo.Core.Tests/BlingoEngine.Lingo.Core.Tests.csproj | Tests for the Lingo scripting runtime |
| Test/BlingoEngine.Lingo.Tests/BlingoEngine.Lingo.Tests.csproj | Tests for BlingoEngine core features |
| Test/BlingoEngine.Tests/BlingoEngine.Tests.csproj | Tests for additional BlingoEngine features |
| Test/BlingoEngine.Blingo.Tests/BlingoEngine.Blingo.Tests.csproj | Tests for higher-level Blingo abstractions |
| Test/BlingoEngine.Blingo.SDL2.Tests/BlingoEngine.Blingo.SDL2.Tests.csproj | Tests for SDL2-specific functionality |
| Test/BlingoEngine.Director.Core.Tests/BlingoEngine.Director.Core.Tests.csproj | Tests for Director tooling |
| Test/BlingoEngine.IO.Legacy.Tests/BlingoEngine.IO.Legacy.Tests.csproj | Tests for the legacy I/O layer |
| Test/BlingoEngine.Legacy.Lingo.Tests/BlingoEngine.Legacy.Lingo.Tests.csproj | Tests for the legacy Lingo compatibility layer |
| Test/BlingoEngine.Net.RNetPipe.Tests/BlingoEngine.Net.RNetPipe.Tests.csproj | Tests for pipe transports |
| Test/BlingoEngine.Net.RNetProjectHost.Tests/BlingoEngine.Net.RNetProjectHost.Tests.csproj | Tests for project host functionality |
| Test/BlingoEngine.Net.RNetTerminal.Tests/BlingoEngine.Net.RNetTerminal.Tests.csproj | Tests for the RNet terminal |
| Test/BlingoEngine.SDL2.GfxVisualTest/BlingoEngine.SDL2.GfxVisualTest.csproj | Console app for manual SDL2 graphics checks |
| Test/Blingo.PacMan.Tests/Blingo.PacMan.Tests.csproj | Tests for the LPacMan demo |

- ### WillMoveToOwnRepo / AbstUI
| Path | Description |
| --- | --- |
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
| WillMoveToOwnRepo/AbstUI/Test/AbstUI.GfxVisualTest.ImGui/AbstUI.GfxVisualTest.ImGui.csproj | ImGui visual test application for AbstUI |
| WillMoveToOwnRepo/AbstUI/Test/AbstUI.GfxVisualTest.LGodot/AbstUI.GfxVisualTest.LGodot.csproj | Godot visual test application for AbstUI |
| WillMoveToOwnRepo/AbstUI/Test/AbstUI.GfxVisualTest.LUnity/AbstUI.GfxVisualTest.LUnity.csproj | Unity visual test application for AbstUI |
| WillMoveToOwnRepo/AbstUI/Test/AbstUI.GfxVisualTest.SDL2/AbstUI.GfxVisualTest.SDL2.csproj | SDL2 visual test application for AbstUI |
| WillMoveToOwnRepo/AbstUI/Test/AbstUI.Tests.Common/AbstUI.Tests.Common.csproj | Shared test utilities for AbstUI |
| WillMoveToOwnRepo/AbstUI/Test/AbstUI.Tests/AbstUI.Tests.csproj | Tests for core AbstUI components |
| WillMoveToOwnRepo/AbstUI/Test/AbstUI.SDLTest/AbstUI.SDLTest.csproj | Tests for the AbstUI SDL2 backend |
| WillMoveToOwnRepo/AbstUI/Test/AbstUI.LGodotTest/AbstUI.LGodotTest.csproj | Tests for the AbstUI Godot backend |

- ### WillMoveToOwnRepo / ProjectorRays
Will be DELETED

## Notes for Agents
- The solution file is `BlingoEngine.sln`. But avoid using it, its big.
- Keep cross-platform compatibility in mind when making changes.

## Generated Code
- Do **not** modify files under any `Generated/` directory. These files are produced for UI verification and should remain unchanged.

