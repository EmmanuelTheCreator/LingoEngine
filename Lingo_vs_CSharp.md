# Lingo vs C# Differences

The table below shows how common Lingo constructs map to their C# equivalents.
Examples were derived from the original TetriGrounds scripts and their
translated C# versions.

| Lingo example | C# equivalent |
|---------------|---------------|
| `-- comment` | `// comment` |
| `put 1 into x` | `x = 1;` |
| `on handler a, b` … `end` | `void Handler(type a, type b) { … }` |
| `global gVar` | `static` field or property |
| `property myValue` | class field/property |
| `repeat with i = 1 to n` | `for (int i = 1; i <= n; i++)` |
| `repeat while cond` | `while (cond)` |
| `repeat until cond` | `do { … } while (!cond)` |
| `repeat forever` | `while (true)` |
| `exit repeat` | `break;` |
| `next repeat` | `continue;` |
| `if a > b then … end if` | `if (a > b) { … }` |
| `case v of … end case` | `switch (v) { … }` |
| `exit` | `return;` |
| `me` | `this` |
| `sprite(n).locH` | `Sprite(n).LocH` |
| `sprite(n).member = member("Name")` | `Sprite(n).SetMember("Name");` |
| `member("Name").text` | `Member<LingoMemberText>("Name").Text` |
| `the mouseH` | `_Mouse.MouseH` |
| `the actorList.append(me)` | `_Movie.ActorList.Add(this)` |
| `script("Class").new(args)` | `new Class(args)` |
| `[1,2,3]` | `new[] { 1, 2, 3 }` |
| `"A" & "B"` | `"A" + "B"` |
| `<>` (not equal) | `!=` |
| `voidp(x)` | `x == null` |
| `sendSprite 2, #doIt` | `SendSprite<B2>(2, b2 => b2.doIt());` |
| `myMovieHandler` | `CallMovieScript<M1>(m1 => m1.myMovieHandler());` |

Additional notes:

- Lingo lists and collections are 1‑based, whereas C# arrays and lists are
  0‑based.
- Lingo requires `then` and `end if` around conditionals; C# uses curly braces.
- To access text members, use the generic `Member<T>` helper, e.g.
  `member("Name").text` becomes `Member<LingoMemberText>("Name").Text`.

## Constructors and IoC

Lingo scripts are instantiated at runtime using the `script("Class").new(args)`
syntax. When converted to C#, each script becomes a class whose constructor can
accept dependencies. The engine resolves these dependencies through the
dependency injection container when the script is created.

```csharp
// Example parent script
public class BlockParentScript : LingoParentScript
{
    private readonly GlobalVars _global;

    // ILingoMovieEnvironment is provided by the runtime; GlobalVars comes
    // from the service container
    public BlockParentScript(ILingoMovieEnvironment env, GlobalVars global)
        : base(env)
    {
        _global = global;
    }
}
```

Scripts are registered with the container using helpers such as
`AddScriptsFromAssembly()` or `AddMovieScript<T>()`. When Lingo code executes
`new(script "BlockParentScript")`, LingoEngine retrieves the constructor from the
container and supplies any required services automatically.


## Creating Cast Members

In Lingo you create new members using the `newMember` command. The type symbol indicates what kind of member to create.

```lingo
-- create a new bitmap cast member
newBitmap = _movie.newMember(#bitmap)
newBitmap.name = "Background"
```

In C#, use the `New` factory on `ILingoMovie` to create typed members:

```csharp
// equivalent to the Lingo example above
var newBitmap = _movie.New.Picture(name: "Background");
```

The factory exposes helper methods for each member type, such as `Picture()`, `Sound()`, `FilmLoop()` and `Text()`. Optional arguments let you specify the cast slot or member name.

