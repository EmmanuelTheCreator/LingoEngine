using System;
using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.CodeGen;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.Analysis.Blocks;

internal static class HandlerBlockClassifierUtilities
{
    public static List<string> ParseArgumentExpressions(IReadOnlyList<BlSyntaxToken> tokens)
    {
        var arguments = new List<string>();
        if (tokens.Count == 0)
        {
            return arguments;
        }

        var segments = BlLegacyHandlerTokenUtilities.SplitByComma(tokens);
        foreach (var segment in segments)
        {
            var expression = BlLegacyExpressionConverter.Convert(segment);
            if (expression.Length > 0)
            {
                arguments.Add(expression);
            }
        }

        return arguments;
    }

    public static string? FindBehaviorForHandler(BlLingoSymbolTable symbols, string handlerName)
    {
        if (string.IsNullOrEmpty(handlerName))
        {
            return null;
        }

        foreach (var scope in symbols.ClassScopes.Values)
        {
            if (scope is null || scope.IsMovieScript)
            {
                continue;
            }

            if (scope.Handlers.ContainsKey(handlerName))
            {
                return scope.Symbol.Name;
            }
        }

        return null;
    }

    public static string? FindMovieHandlerScope(BlLingoSymbolTable symbols, string handlerName)
    {
        if (string.IsNullOrEmpty(handlerName))
        {
            return null;
        }

        if (symbols.MovieScript.Handlers.ContainsKey(handlerName))
        {
            return symbols.MovieScript.Symbol.Name;
        }

        foreach (var scope in symbols.ClassScopes.Values)
        {
            if (!scope.IsMovieScript)
            {
                continue;
            }

            if (scope.Handlers.ContainsKey(handlerName))
            {
                return scope.Symbol.Name;
            }
        }

        return null;
    }
}
