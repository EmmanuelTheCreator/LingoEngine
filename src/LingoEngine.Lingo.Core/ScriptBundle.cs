using System.Collections.Generic;

namespace LingoEngine.Lingo.Core;

/// <summary>
/// Represents a single Lingo script file with its type.
/// </summary>
public class LingoScriptFile
{
    public required string Name { get; init; }
    public required string Source { get; init; }
    public LingoScriptType Type { get; init; }
}

/// <summary>
/// Specifies the kind of script a file contains.
/// </summary>
public enum LingoScriptType
{
    Parent,
    Behavior,
    Movie
}

/// <summary>
/// Result of a batch conversion.
/// </summary>
public class LingoBatchResult
{
    public Dictionary<string, string> ConvertedScripts { get; } = new();
    public HashSet<string> CustomMethods { get; } = new();
}
