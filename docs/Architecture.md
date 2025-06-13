# Architecture Overview

LingoEngine is split into several layers:

1. **Core** – The language runtime implementing the Lingo VM.
2. **Framework adapters** – Front‑ends for different rendering frameworks. Currently Godot and SDL2 are provided.
3. **Director** – Optional higher level APIs mirroring Macromedia Director behaviours.
4. **Demo projects** – Sample games demonstrating how to use the engine with each framework.

Each adapter exposes common interfaces so the core can run unchanged on multiple platforms.

## Interfaces and Implementations

The `src/LingoEngine` project defines interfaces for engine concepts such as
`ILingoFrameworkSprite`, `ILingoFrameworkMovie`, and `ILingoFrameworkStage`.
Rendering adapters like **LingoEngine.LGodot** and **LingoEngine.SDL2** provide
concrete classes that implement these interfaces. The core engine interacts only
with the interfaces, allowing the same game logic to run on any framework.

### Factory Pattern

A central `ILingoFrameworkFactory` creates the platform specific objects. Each
adapter implements this factory to produce stages, sprites, members, and input
handlers. The core asks the factory for objects without knowing the underlying
framework.

```csharp
ILingoFrameworkFactory factory = new GodotFactory(serviceProvider, root);
var stage = factory.CreateStage(new LingoPlayer());
var movie = factory.AddMovie(stage, new LingoMovie("Demo"));
```

Adapters may register their factory with a dependency injection container so the
game can resolve the correct implementation at runtime.
