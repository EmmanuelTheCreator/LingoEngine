using System;

namespace OldBlingoEngine.Lingo.Core;

/// <summary>
/// Options that control how Lingo scripts are converted to C#.
/// </summary>
public class ConversionOptions
{
    /// <summary>
    /// Access modifier used for generated methods.
    /// </summary>
    public string MethodAccessModifier { get; set; } = "public";

    /// <summary>
    /// Base namespace for generated files. Subdirectories are appended using PascalCase.
    /// </summary>
    public string? Namespace { get; set; } = "Generated";
}

