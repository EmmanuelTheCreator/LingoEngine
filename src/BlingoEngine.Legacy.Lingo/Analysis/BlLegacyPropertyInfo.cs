using System;

namespace BlingoEngine.Legacy.Lingo.Analysis;

/// <summary>
/// Describes a property declared within a legacy Lingo class, including its inferred type and trailing comment.
/// </summary>
public sealed class BlLegacyPropertyInfo
{
    public BlLegacyPropertyInfo(string name, string type, string? comment)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(type);

        Name = name;
        Type = type;
        Comment = comment;
    }

    /// <summary>
    /// Gets the property name as it should appear in generated C# code.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the inferred property type.
    /// </summary>
    public string Type { get; }

    /// <summary>
    /// Gets the trailing comment associated with the property declaration, if any.
    /// </summary>
    public string? Comment { get; }
}
