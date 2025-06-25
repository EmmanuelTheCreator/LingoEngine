# LingoEngine

**LingoEngine** is a modern, cross-platform C# runtime designed to emulate Macromedia Director's **Lingo** scripting language. It enables playback of original Lingo code and behaviors on top of modern rendering backends like **Godot** and **SDL2**, allowing legacy projects to be revived or reimagined with full flexibility.

---

## ✨ Key Features

- ✅ **Lingo Script Execution** – Runs legacy Macromedia Director scripts directly in C#.
- 🔌 **Pluggable Rendering Backends** – Clean architecture supporting:
  - [Godot Engine](https://godotengine.org/)
  - SDL2
- 🧠 **Experimental Director API Layer** – Provides high-level compatibility with Director's original movie and cast behavior.
- 🧩 **Modular Runtime Architecture** – Clear separation of concerns: input, rendering, audio, system services, and script execution.
- ⚙️ **Service-Oriented Initialization** – Uses dependency injection and service collections for clean setup.
- 🌍 **Cross-Platform Compatibility** – Works anywhere the .NET SDK is available.

---

## 📁 Project Structure

| Folder | Description |
|--------|-------------|
| `src/LingoEngine` | Core Lingo runtime and engine abstractions |
| `src/LingoEngine.LGodot` | Adapter for [Godot](https://godotengine.org/) |
| `src/LingoEngine.SDL2` | Adapter for SDL2 |
| `src/Director` | Experimental Director API re‑implementations |
| `Demo/TetriGrounds` | Sample game showing usage with both backends |

🔎 For a detailed technical overview, see the [Architecture guide](docs/Architecture.md).

---

## 🚀 Getting Started

1. **Clone the repository**:

   ```bash
   git clone https://github.com/EmmanuelTheCreator/LingoEngine.git
   cd LingoEngine
   ```

2. **Open the solution**  
   Open `LingoEngine.sln` in your preferred C# IDE (Visual Studio / Rider).

3. **Build a demo**  
   Navigate to `Demo/TetriGrounds` and run one of the included platform integrations.

👉 Use the dedicated guides for full setup instructions:

- [Godot Setup](docs/GodotSetup.md)
- [SDL2 Setup](docs/SDLSetup.md)

### VS Code Setup

1. Install the [.NET SDK](https://learn.microsoft.com/dotnet/core/install/) and [Godot 4](https://godotengine.org/) with C# support.
2. Open the repository folder in VS Code and accept the recommended extensions.
3. Press <kbd>Ctrl</kbd>+<kbd>Shift</kbd>+<kbd>B</kbd> to build the solution.
4. From the Run and Debug panel choose **Launch Demo SDL2** or **Launch Demo Godot**.


---

## 🎮 Running the Demo

Both the SDL2 and Godot frontends share the same backend logic. Here's an example of how to bootstrap the SDL2 engine:

```csharp
var services = new ServiceCollection();
services.AddTetriGrounds(cfg => cfg.WithLingoSdlEngine("TetriGrounds", 640, 480));
var provider = services.BuildServiceProvider();

provider.GetRequiredService<TetriGroundsGame>().Play();
provider.GetRequiredService<SdlRootContext>().Run();
```

Swap to the Godot backend by using `.WithLingoGodotEngine(...)`.

📄 See [Godot Setup](docs/GodotSetup.md) and [SDL2 Setup](docs/SDLSetup.md) for exact details.

---

## 🧪 Running Tests

This project uses the .NET SDK. You can run all unit tests with:

```bash
dotnet test
```

Need to install the SDK?

- Follow the [official install guide](https://learn.microsoft.com/dotnet/core/install/)
- Or run the helper script:

```bash
./scripts/install-dotnet.sh
```

---

## 📚 Documentation

### Guides

- [Lingo vs C# Differences](Lingo_vs_CSharp.md)
- [Architecture Overview](docs/Architecture.md)
- [Godot Setup](docs/GodotSetup.md)
- [SDL2 Setup](docs/SDLSetup.md)
- [XMED File Comparisons](docs/XMED_FileComparisons.md)
- [XMED Offsets](docs/XMED_Offsets.md)
- [Text Styling Example](docs/Text_Multi_Line_Multi_Style.md)

### API Reference

Documentation generated from the source code is available using [DocFX](https://github.com/dotnet/docfx). Run `scripts/build-docs.sh` to produce the site in `docs/docfx/_site`. The pages include "View Source" links back to the repository.

### Component READMEs

- [Core Runtime Readme](src/LingoEngine/README.md)
- [Godot Adapter Readme](src/LingoEngine.LGodot/ReadMe.md)
- [SDL2 Adapter Readme](src/LingoEngine.SDL2/ReadMe.md)
- [IO Library Readme](src/LingoEngine.IO/ReadMe.md)
- [Director Core Readme](src/Director/LingoEngine.Director.Core/ReadMe.md)
- [Director Godot Adapter Readme](src/Director/LingoEngine.Director.LGodot/ReadMe.md)
- [AI Conversion Notes](Demo/TetriGrounds/ConversionTextForAI.md)

---

## 🧭 Roadmap

| Feature                                | Status       |
|----------------------------------------|--------------|
| Full Lingo language support            | In Progress  |
| Director-compatible APIs               | Partial      |
| Backends: Godot, SDL2                  | ✅ Implemented |
| Documentation & learning materials     | In Progress  |
| Unity integration                      | Planned      |
| Lingo bytecode (dcode) interpreter     | Experimental |

---

## 🤝 Contributing

We welcome contributions from the community!

To get started:

1. Fork this repository
2. Create a feature branch
3. Write your code and tests
4. Submit a pull request

Please include examples or documentation when appropriate.

---

## ⭐ Why Use LingoEngine?

- 🚀 Port legacy Director projects to modern engines
- 🔁 Reuse existing assets, scripts, and logic
- 🛠️ Build hybrid projects that combine old logic with new rendering
- 🕹️ Explore the inner workings of Director games using readable C# code
- 💾 Preserve interactive media history with a modern toolset

---

## 📄 License

Licensed under the [MIT License](LICENSE).

---

## 🙋‍♂️ Questions or Feedback?

Feel free to [open an issue](https://github.com/EmmanuelTheCreator/LingoEngine/issues) or start a discussion. We're happy to help, and open to ideas!

