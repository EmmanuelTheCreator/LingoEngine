using System;
using System.Text;
using BlingoEngine.Legacy.Lingo.Analysis;

namespace BlingoEngine.Legacy.Lingo.CodeGen;

/// <summary>
/// Provides helper routines for composing valid C# identifiers from Lingo script names.
/// </summary>
internal static class BlCSharpName
{
    /// <summary>
    /// Produces a sanitized script name without the common "_ls" suffix.
    /// </summary>
    public static string NormalizeScriptName(string name)
    {
        var sanitized = SanitizeIdentifier(name);
        return sanitized.EndsWith("_ls", StringComparison.OrdinalIgnoreCase)
            ? sanitized[..^3]
            : sanitized;
    }

    /// <summary>
    /// Composes the final class name using the supplied script name and detected script kind.
    /// </summary>
    public static string ComposeClassName(string name, BlLingoScriptKind kind, BlLegacyClassGeneratorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var baseName = NormalizeScriptName(name);
        baseName = RemoveLeadingUnderscores(baseName);

        var suffix = kind switch
        {
            BlLingoScriptKind.Movie => options.MovieScriptSuffix,
            BlLingoScriptKind.Parent => options.ParentSuffix,
            BlLingoScriptKind.Behavior => options.BehaviorSuffix,
            _ => options.ScriptSuffix,
        };
        return string.IsNullOrEmpty(suffix) ? baseName : baseName + suffix;
    }

    internal static string SanitizeIdentifier(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Script";
        }

        var builder = new StringBuilder(name.Length);
        foreach (var ch in name)
        {
            builder.Append(char.IsLetterOrDigit(ch) || ch == '_' ? ch : '_');
        }

        var index = 0;
        while (index < builder.Length && !(char.IsLetter(builder[index]) || builder[index] == '_'))
        {
            index++;
        }

        if (index > 0)
        {
            builder.Remove(0, index);
        }

        if (builder.Length == 0)
        {
            return "Script";
        }

        if (char.IsDigit(builder[0]))
        {
            builder.Insert(0, 'S');
        }

        return builder.ToString();
    }

    private static string RemoveLeadingUnderscores(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "Script";
        }

        var trimmed = value.TrimStart('_');
        if (trimmed.Length == 0)
        {
            return "Script";
        }

        if (char.IsDigit(trimmed[0]))
        {
            trimmed = "S" + trimmed;
        }

        return trimmed;
    }
}
