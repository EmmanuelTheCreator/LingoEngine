# LingoEngine

**LingoEngine** is a C# runtime for games originally written in Macromedia Director's Lingo language. It provides a modern, modular architecture so existing Lingo projects can run on top of different rendering frameworks.

## Key Features

- Executes original Macromedia Director scripts using a modern C# runtime.
- Pluggable rendering backends with adapters for **Godot** and **SDL2**.
- Optional Director API layer offering higher level compatibility.
- Cross-platform execution anywhere the .NET SDK is available.

## Projects

| Folder | Description |
|-------|------------|
| `src/LingoEngine` | Core Lingo runtime and abstractions |
| `src/LingoEngine.LGodot` | Adapter for the [Godot](https://godotengine.org/) engine |
| `src/LingoEngine.SDL2` | Adapter for SDL2 based applications |
| `src/Director` | Experimental Director API re‑implementations |
| `Demo/TetriGrounds` | Sample game showing how to integrate with Godot and SDL2 |

See the [Architecture overview](docs/Architecture.md) for details on how these pieces fit together.

## Getting Started

Clone the repository and open `LingoEngine.sln` in your preferred C# IDE. To build a demo project, choose the corresponding solution inside `Demo/TetriGrounds`.

Detailed setup instructions are available for:

- [Godot](docs/GodotSetup.md)
- [SDL2](docs/SDLSetup.md)

## Running the Demo

Both front‑ends share the same core setup. Register the engine with a
`ServiceCollection` and then build the provider:

```csharp
var services = new ServiceCollection();
services.AddTetriGrounds(cfg => cfg.WithLingoSdlEngine("TetriGrounds", 640, 480));
var provider = services.BuildServiceProvider();
provider.GetRequiredService<TetriGroundsGame>().Play();
provider.GetRequiredService<SdlRootContext>().Run();
```

The Godot version follows the same pattern but uses `WithLingoGodotEngine`. See
the dedicated guides for full instructions.

## Running Tests

This repository uses the **.NET SDK**. Make sure `dotnet` is available in your
`PATH` before running the test suite:

```bash
dotnet test
```

Refer to the [official installation guide](https://learn.microsoft.com/dotnet/core/install/) if the `dotnet` command is missing.

You can automatically install the SDK by executing the helper script:

```bash
./scripts/install-dotnet.sh
```

## Additional Documentation

The repository contains further guides and reference material:

- [Lingo vs C# Differences](Lingo_vs_CSharp.md)
- [Architecture Overview](docs/Architecture.md)
- [Godot Setup](docs/GodotSetup.md)
- [SDL2 Setup](docs/SDLSetup.md)
- [XMED File Comparisons](docs/XMED_FileComparisons.md)
- [XMED Offsets](docs/XMED_Offsets.md)
- [Text Styling Example](docs/Text_Multi_Line_Multi_Style.md)
- [Core Runtime Readme](src/LingoEngine/README.md)
- [Godot Adapter Readme](src/LingoEngine.LGodot/ReadMe.md)
- [IO Library Readme](src/LingoEngine.IO/ReadMe.md)
- [Director Core Readme](src/Director/LingoEngine.Director.Core/ReadMe.md)
- [Director Godot Adapter Readme](src/Director/LingoEngine.Director.LGodot/ReadMe.md)
- [SDL2 Adapter Readme](src/LingoEngine.SDL2/ReadMe.md)
- [Conversion Text For AI](Demo/TetriGrounds/ConversionTextForAI.md)

## License

This project is licensed under the terms of the [MIT License](LICENSE).
