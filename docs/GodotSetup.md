# Setting Up the Godot Runtime

This guide explains how to build and run the Godot front‑end for LingoEngine.

1. Install **Godot 4** from the [official website](https://godotengine.org/).
2. Open `LingoEngine.Demo.TetriGrounds.Godot.sln` with your C# IDE or open the `project.godot` file directly in Godot.
3. Ensure the Godot C# tools are configured. In the Godot editor, open **Project → Tools → C#** and set the path to `dotnet` if required.
4. Build and run the project from within Godot. The demo project demonstrates how LingoEngine integrates with Godot scenes and nodes.

```csharp
// RootNodeTetriGrounds.cs excerpt
TetriGroundsSetup.AddTetriGrounds(_services, c =>
    c.WithLingoGodotEngine(this));
var provider = _services.BuildServiceProvider();
TetriGroundsSetup.StartGame(provider);
```
