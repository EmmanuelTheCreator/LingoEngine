using System;

namespace OldBlingoEngine.Lingo.Core;

/// <summary>
/// Configuration for Lingo to C# class name generation.
/// </summary>
public class BlingoToCSharpConverterSettings
{
    public string BehaviorSuffix { get; set; } = "Behavior";
    public string ParentSuffix { get; set; } = "Parent";
    public string MovieScriptSuffix { get; set; } = "MovieScript";
}

