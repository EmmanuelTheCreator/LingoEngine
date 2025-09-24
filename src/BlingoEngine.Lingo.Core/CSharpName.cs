using System;
using System.Text;

namespace BlingoEngine.Lingo.Core;

public static class CSharpName
{
    public static string SanitizeIdentifier(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "L";
        var sb = new StringBuilder();
        foreach (var c in name)
            sb.Append(char.IsLetterOrDigit(c) || c == '_' ? c : '_');

        void TrimLeadingUnderscoresAndDigits()
        {
            var trim = 0;
            while (trim < sb.Length && (char.IsDigit(sb[trim]) || sb[trim] == '_'))
                trim++;
            if (trim > 0)
                sb.Remove(0, trim);
        }

        TrimLeadingUnderscoresAndDigits();

        if (sb.Length > 2 && sb[1] == '_' && char.IsLetter(sb[0]))
        {
            sb.Remove(0, 2);
            TrimLeadingUnderscoresAndDigits();
        }

        if (sb.Length == 0 || !char.IsLetter(sb[0]))
            sb.Insert(0, 'L');
        return sb.ToString();
    }

    public static string NormalizeScriptName(string name)
    {
        var safe = SanitizeIdentifier(name);
        return safe.EndsWith("_ls", StringComparison.OrdinalIgnoreCase) ? safe[..^3] : safe;
    }

    public static string ComposeName(string name, BlingoScriptType type, BlingoToCSharpConverterSettings settings)
    {
        var baseName = NormalizeScriptName(name);
        var suffix = type switch
        {
            BlingoScriptType.Movie => settings.MovieScriptSuffix,
            BlingoScriptType.Parent => settings.ParentSuffix,
            BlingoScriptType.Behavior => settings.BehaviorSuffix,
            _ => "Script"
        };
        return baseName + suffix;
    }
}


