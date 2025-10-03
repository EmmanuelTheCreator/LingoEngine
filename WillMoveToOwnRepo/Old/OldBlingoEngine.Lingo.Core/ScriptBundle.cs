using System.Collections.Generic;

namespace OldBlingoEngine.Lingo.Core;

/// <summary>
/// Represents a single Lingo script file with its type.
/// </summary>
public class BlingoScriptFile
{
    /// <summary>
    /// Filename of the script file.
    /// </summary>
    public string Name { get; init; }
    /// <summary>
    /// Source code contained in the script file.
    /// </summary>
    public string Source { get; init; }
    /// <summary>
    /// Directory of the script relative to the conversion root.
    /// Used to build namespaces from folder names.
    /// </summary>
    public string? RelativeDirectory { get; init; }
    public string CSharp { get; set; } = "";
    /// <summary>
    /// Requested script type (Auto, Parent, Behavior, Movie).
    /// </summary>
    public ScriptDetectionType Detection { get; init; } = ScriptDetectionType.Auto;
    /// <summary>
    /// Detected script type used during conversion.
    /// </summary>
    public BlingoScriptType Type { get; set; } = BlingoScriptType.Behavior;
    public string Errors { get; internal set; } = "";

    public BlingoScriptFile() : this(string.Empty, string.Empty) { }

    public BlingoScriptFile(string name, string source, ScriptDetectionType detection = ScriptDetectionType.Auto)
    {
        Name = name;
        Source = source;
        Detection = detection;
    }

    public BlingoScriptFile(string name, string source, BlingoScriptType type)
        : this(name, source, (ScriptDetectionType)type)
    {
        Type = type;
    }
}

/// <summary>
/// Specifies the kind of script a file contains.
/// </summary>
public enum BlingoScriptType
{
    Parent,
    Behavior,
    Movie
}

/// <summary>
/// Specifies how a script's type should be determined.
/// </summary>
public enum ScriptDetectionType
{
    Auto,
    Parent,
    Behavior,
    Movie
}

/// <summary>
/// Result of a batch conversion.
/// </summary>
public class BlingoBatchResult
{
    public Dictionary<string, string> ConvertedScripts { get; } = new();
    public HashSet<string> CustomMethods { get; } = new();
    public Dictionary<string, List<MethodSignature>> Methods { get; } = new();
    public Dictionary<string, List<PropertyInfo>> Properties { get; } = new();
}

/// <summary>
/// Represents a converted method signature.
/// </summary>
public record MethodSignature(string Name, List<ParameterInfo> Parameters);

/// <summary>
/// Represents a parameter detected during conversion.
/// </summary>
public record ParameterInfo(string Name, string Type);

/// <summary>
/// Represents a property detected during conversion.
/// </summary>
public record PropertyInfo(string Name, string Type);

