using System;
using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.Analysis;
using BlingoEngine.Legacy.Lingo.CodeGen;
using BlingoEngine.Scripts;

namespace BlingoEngine.Legacy.Lingo;

/// <summary>
/// Bridges the Director tooling with the legacy Lingo conversion pipeline.
/// </summary>
public sealed class BlingoToCSharpConverter
{
    private readonly List<string> _errors = new();

    public IReadOnlyList<string> Errors => _errors;

    public string Convert(string? lingoSource)
    {
        _errors.Clear();
        var script = new BlingoScriptFile("ConvertedScript", lingoSource ?? string.Empty, BlingoScriptType.Behavior);
        return ConvertScript(script, respectDeclaredKind: false);
    }

    public BlingoBatchResult Convert(IEnumerable<BlingoScriptFile> scripts)
    {
        _errors.Clear();
        var result = new BlingoBatchResult();
        if (scripts is null)
        {
            return result;
        }

        foreach (var script in scripts)
        {
            if (script is null)
            {
                continue;
            }

            var code = ConvertScript(script, respectDeclaredKind: true);
            if (!string.IsNullOrEmpty(code))
            {
                var key = string.IsNullOrWhiteSpace(script.Name) ? "ConvertedScript" : script.Name;
                result.ConvertedScripts[key] = code;
            }
        }

        return result;
    }

    public string GetCurrentErrorsAndFlush()
    {
        if (_errors.Count == 0)
        {
            return string.Empty;
        }

        var message = string.Join(Environment.NewLine, _errors);
        _errors.Clear();
        return message;
    }

    private string ConvertScript(BlingoScriptFile script, bool respectDeclaredKind)
    {
        var scriptName = string.IsNullOrWhiteSpace(script.Name) ? "ConvertedScript" : script.Name;
        var source = script.Source ?? string.Empty;
        var kind = respectDeclaredKind ? MapScriptType(script.Type) : BlLingoScriptKind.Unknown;

        try
        {
            var generator = new BlLegacyClassGenerator();
            var code = generator.GenerateClass(scriptName, source, kind);
            script.CSharp = code;
            return code;
        }
        catch (Exception ex)
        {
            var message = $"{scriptName}: {ex.Message}";
            _errors.Add(message);
            script.CSharp = string.Empty;
            script.Errors = string.IsNullOrEmpty(script.Errors)
                ? message
                : string.Concat(script.Errors, Environment.NewLine, message);
            return string.Empty;
        }
    }

    private static BlLingoScriptKind MapScriptType(BlingoScriptType type)
    {
        return type switch
        {
            BlingoScriptType.Movie => BlLingoScriptKind.Movie,
            BlingoScriptType.Parent => BlLingoScriptKind.Parent,
            BlingoScriptType.Behavior => BlLingoScriptKind.Behavior,
            _ => BlLingoScriptKind.Unknown,
        };
    }
}

public sealed class BlingoBatchResult
{
    public Dictionary<string, string> ConvertedScripts { get; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class BlingoScriptFile
{
    public BlingoScriptFile(string name, string source, BlingoScriptType type)
    {
        Name = name ?? string.Empty;
        Source = source ?? string.Empty;
        Type = type;
    }

    public string Name { get; }

    public string Source { get; }

    public BlingoScriptType Type { get; set; }

    public string CSharp { get; set; } = string.Empty;

    public string Errors { get; set; } = string.Empty;
}
