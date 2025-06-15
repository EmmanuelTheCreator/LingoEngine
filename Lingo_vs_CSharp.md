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

Additional notes:

- Lingo lists and collections are 1‑based, whereas C# arrays and lists are
  0‑based.
- Lingo requires `then` and `end if` around conditionals; C# uses curly braces.

