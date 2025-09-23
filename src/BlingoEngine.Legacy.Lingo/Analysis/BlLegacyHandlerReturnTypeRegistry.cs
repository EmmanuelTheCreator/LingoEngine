using System;
using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.CodeGen;

namespace BlingoEngine.Legacy.Lingo.Analysis;

internal static class BlLegacyHandlerReturnTypeRegistry
{
    private static readonly object s_lock = new();
    private static readonly Dictionary<string, string> s_byScript = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, string> s_global = new(StringComparer.Ordinal);

    public static void Register(BlLingoScriptKind scriptKind, string? scriptName, string handlerName, string resultType)
    {
        if (string.IsNullOrWhiteSpace(handlerName) || string.IsNullOrWhiteSpace(resultType))
        {
            return;
        }

        var normalizedHandler = BlCSharpName.SanitizeIdentifier(handlerName);
        if (string.IsNullOrEmpty(normalizedHandler))
        {
            return;
        }

        var normalizedScript = scriptName?.Trim() ?? string.Empty;

        lock (s_lock)
        {
            RegisterKey(s_global, ComposeHandlerKey(normalizedHandler), resultType);

            if (normalizedScript.Length > 0)
            {
                RegisterKey(s_byScript, ComposeScriptKey(normalizedScript, normalizedHandler), resultType);
            }

            if (scriptKind == BlLingoScriptKind.Movie)
            {
                RegisterKey(s_byScript, ComposeScriptKey(BlLingoClassSymbolTable.MovieScriptName, normalizedHandler), resultType);
            }
        }
    }

    public static string? Resolve(BlLingoScriptKind scriptKind, string? scriptName, string handlerName)
    {
        if (string.IsNullOrWhiteSpace(handlerName))
        {
            return null;
        }

        var normalizedHandler = BlCSharpName.SanitizeIdentifier(handlerName);
        if (string.IsNullOrEmpty(normalizedHandler))
        {
            return null;
        }

        var normalizedScript = scriptName?.Trim() ?? string.Empty;

        lock (s_lock)
        {
            if (normalizedScript.Length > 0)
            {
                var scriptKey = ComposeScriptKey(normalizedScript, normalizedHandler);
                if (s_byScript.TryGetValue(scriptKey, out var type))
                {
                    return type;
                }
            }

            if (scriptKind == BlLingoScriptKind.Movie)
            {
                var movieKey = ComposeScriptKey(BlLingoClassSymbolTable.MovieScriptName, normalizedHandler);
                if (s_byScript.TryGetValue(movieKey, out var movieType))
                {
                    return movieType;
                }
            }

            var handlerKey = ComposeHandlerKey(normalizedHandler);
            if (s_global.TryGetValue(handlerKey, out var fallbackType))
            {
                return fallbackType;
            }
        }

        return null;
    }

    private static void RegisterKey(Dictionary<string, string> table, string key, string candidate)
    {
        if (table.TryGetValue(key, out var existing))
        {
            table[key] = MergeTypes(existing, candidate);
            return;
        }

        table[key] = candidate;
    }

    private static string MergeTypes(string existing, string candidate)
    {
        if (string.Equals(existing, candidate, StringComparison.Ordinal))
        {
            return existing;
        }

        if (string.Equals(existing, "object?", StringComparison.Ordinal) ||
            string.Equals(candidate, "object?", StringComparison.Ordinal))
        {
            return "object?";
        }

        if ((string.Equals(existing, "int", StringComparison.Ordinal) && string.Equals(candidate, "double", StringComparison.Ordinal)) ||
            (string.Equals(existing, "double", StringComparison.Ordinal) && string.Equals(candidate, "int", StringComparison.Ordinal)))
        {
            return "double";
        }

        return "object?";
    }

    private static string ComposeScriptKey(string scriptName, string handlerName)
    {
        return $"{scriptName.ToUpperInvariant()}|{handlerName.ToUpperInvariant()}";
    }

    private static string ComposeHandlerKey(string handlerName)
    {
        return handlerName.ToUpperInvariant();
    }
}
