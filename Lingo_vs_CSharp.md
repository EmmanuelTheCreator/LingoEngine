# Lingo vs C# Differences

The tables below summarize how LingoEngine converts Lingo syntax into C#.

## Lingo‑specific constructs

| Lingo example | C# equivalent |
|---------------|---------------|
| `-- comment` | `// comment` |
| `on handler a, b` … `end` | `void Handler(type a, type b) { … }` |
| `global gVar` | `GlobalVars` singleton injected via DI |
| `property myValue` | class field/property |
| `me` | `this` |
| `sprite(n).locH` | `Sprite(n).LocH` |
| `sprite(n).member = member("Name")` | `Sprite(n).SetMember("Name");` |
| `member("Name").text` | `Member<LingoMemberText>("Name").Text` |
| `the mouseH` | `_Mouse.MouseH` |
| `the actorList.append(me)` | `_Movie.ActorList.Add(this)` |
| `script("Class").new(args)` | `new Class(args)` |
| `sendSprite 2, #doIt` | `SendSprite<B2>(2, b2 => b2.doIt());` |
| `myMovieHandler` | `CallMovieScript<M1>(m1 => m1.myMovieHandler());` |

## Classic control flow

| Lingo example | C# equivalent |
|---------------|---------------|
| `repeat with i = 1 to n` | `for (int i = 1; i <= n; i++)` |
| `repeat with item in list` | `foreach (var item in list)` |
| `repeat while cond` | `while (cond)` |
| `repeat until cond` | `do { … } while (!cond)` |
| `repeat forever` | `while (true)` |
| `exit repeat` | `break;` |
| `exit repeat if cond` | `if (cond) break;` |
| `next repeat` | `continue;` |
| `next repeat if cond` | `if (cond) continue;` |
| `if a > b then … end if` | `if (a > b) { … }` |
| `case v of … end case` | `switch (v) { … }` |
| `exit` | `return;` |
| `[1,2,3]` | `new[] { 1, 2, 3 }` |
| `"A" & "B"` | `"A" + "B"` |
| `<>` (not equal) | `!=` |
| `voidp(x)` | `x == null` |

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
var newBitmap = _movie.New.Bitmap(name: "Background");
```

The factory exposes helper methods for each member type, such as `Bitmap()`, `Sound()`, `FilmLoop()` and `Text()`. Optional arguments let you specify the cast slot or member name.


## 🔁 `put ... into ...` Handling in C#

Lingo's `put` syntax is used for assignment across a wide range of targets:

```lingo
put "Hello" into field "Status"
put 100 into sprite(3).locH
put 42 into myList[2]
```

In Lingo, **if the target does not exist (e.g., a missing field), it fails silently**.  
To match that behavior in C#, LingoEngine introduces *safe assignment helpers*.

### ✅ C# Equivalents

| Lingo | C# |
|-------|----|
| `put 100 into x` | `x = 100;` |
| `put "Hi" into field "Greeting"` | `PutTextIntoField("Greeting", "Hi");` |
| `put 100 into sprite(3).locH` | `Sprite(3).LocH = 100;` |
| `put 42 into myList[2]` | `myList.SetAt(2, 42);` |

---

## ✳️ Safe Field Assignment: `PutTextIntoField()`

```csharp
PutTextIntoField("Greeting", "Hello");
```

This method attempts to locate a field member and assign the text, but **does nothing if the field is missing**, just like Lingo.

### Method:
```csharp
protected void PutTextIntoField(string name, string text)
{
    TryMember<ILingoMemberField>(name, field => field.Text = text);
}
```

---

## ⚠️ Avoiding Exceptions with `TryMember<T>()`

To safely access any member (e.g., bitmap, field, text), use the `Action<T>` overload:

```csharp
TryMember<ILingoMemberText>("Title", m => m.Text = "Welcome");
```

This form avoids explicit null checks and keeps the call concise.

### `TryMember` overloads available:
```csharp
TryMember<T>(string name, int? castLib = null, Action<T>? action = null)
```

This method:
- Returns `null` if the member is not found
- Executes the action if the member exists

---

## 🧱 Building Custom Safe `put` Functions

You can create safe helpers for other member types, e.g.:

```csharp
protected void PutSoundInto(string name, ILingoSoundData sound)
{
    TryMember<ILingoMemberSound>(name, m => m.Sound = sound);
}
```

---

## 🔍 Summary

| Pattern | Use |
|--------|-----|
| Direct assignment | For local variables or known safe references |
| `PutTextIntoField(name, value)` | For field text, silent on failure |
| `TryMember<T>(..., action)` | For concise safe access to members |
| `SafePut<T>()` (optional) | For generalized logic via delegates |

This ensures **Lingo compatibility** without throwing exceptions and keeps behavior **fail-safe**, just like in Director.
