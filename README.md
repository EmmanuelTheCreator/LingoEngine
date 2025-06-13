# LingoEngine

**LingoEngine** is a C# runtime for games originally written in Macromedia Director's Lingo language. It provides a modern, modular architecture so existing Lingo projects can run on top of different rendering frameworks.

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

## License

This project is licensed under the terms of the [MIT License](LICENSE).
