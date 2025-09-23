using System;
using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.CodeGen;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.Analysis.Passes;

/// <summary>
/// Classifies handler body statements into high level code blocks so code generation can operate on pre-analysed structures.
/// </summary>
public sealed class BlLingoHandlerCodeBlockPass : BlLingoAnalysisPass
{
    public const string HandlerCodeBlocksKey = "Legacy.HandlerCodeBlocks";
    public const string HandlerReturnTypesKey = "Legacy.HandlerReturnTypes";

    public BlLingoHandlerCodeBlockPass()
        : base("HandlerCodeBlocks")
    {
    }

    public override void Execute(BlLingoAnalysisContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var tokens = context.Tokens;
        var map = new Dictionary<BlLingoHandlerSymbolTable, IReadOnlyList<BlLingoHandlerCodeBlock>>();
        var owners = new Dictionary<BlLingoHandlerSymbolTable, BlLingoClassSymbolTable>();

        foreach (var classScope in EnumerateClasses(context.Symbols))
        {
            foreach (var handler in classScope.Handlers.Values)
            {
                if (handler is null)
                {
                    continue;
                }

                var body = ExtractHandlerBody(tokens, handler);
                if (body.Tokens.Count == 0 && body.EndTrivia.Count == 0)
                {
                    map[handler] = Array.Empty<BlLingoHandlerCodeBlock>();
                    continue;
                }

                var blocks = BlLegacyHandlerCodeBlockClassifier.BuildBlocks(body.Tokens, body.EndTrivia, context.Symbols);
                map[handler] = blocks;
                owners[handler] = classScope;
            }
        }

        var returnTypes = ComputeReturnTypes(map);
        var handlers = new List<BlLingoHandlerSymbolTable>(map.Keys);
        foreach (var handler in handlers)
        {
            var blocks = map[handler];
            var annotated = AnnotateCallResults(blocks, returnTypes, context.Symbols);
            map[handler] = annotated;
        }

        RegisterReturnTypes(returnTypes, owners);

        context.SetData<IReadOnlyDictionary<BlLingoHandlerSymbolTable, IReadOnlyList<BlLingoHandlerCodeBlock>>>(HandlerCodeBlocksKey, map);
        context.SetData<IReadOnlyDictionary<BlLingoHandlerSymbolTable, string?>>(HandlerReturnTypesKey, returnTypes);
    }

    private static IEnumerable<BlLingoClassSymbolTable> EnumerateClasses(BlLingoSymbolTable symbols)
    {
        yield return symbols.MovieScript;
        foreach (var scope in symbols.ClassScopes.Values)
        {
            yield return scope;
        }
    }

    private static HandlerBody ExtractHandlerBody(IReadOnlyList<BlSyntaxToken> tokens, BlLingoHandlerSymbolTable handler)
    {
        if (handler.Symbol.Declarations.Count == 0)
        {
            return HandlerBody.Empty;
        }

        var declaration = handler.Symbol.Declarations[0];
        var declarationIndex = IndexOfToken(tokens, declaration);
        if (declarationIndex < 0)
        {
            return HandlerBody.Empty;
        }

        var bodyStartIndex = declarationIndex + 1;
        while (bodyStartIndex < tokens.Count)
        {
            var token = tokens[bodyStartIndex];
            if (ContainsNewLine(token.LeadingTrivia))
            {
                break;
            }

            bodyStartIndex++;
        }

        if (bodyStartIndex >= tokens.Count)
        {
            return HandlerBody.Empty;
        }

        var endIndex = FindHandlerEndIndex(tokens, bodyStartIndex);
        if (endIndex < 0 || endIndex <= bodyStartIndex)
        {
            return HandlerBody.Empty;
        }

        var bodyTokens = new List<BlSyntaxToken>(endIndex - bodyStartIndex);
        for (var index = bodyStartIndex; index < endIndex; index++)
        {
            bodyTokens.Add(tokens[index]);
        }

        return new HandlerBody(bodyTokens, tokens[endIndex].LeadingTrivia ?? Array.Empty<BlSyntaxTrivia>());
    }

    private static int IndexOfToken(IReadOnlyList<BlSyntaxToken> tokens, BlSyntaxToken target)
    {
        for (var index = 0; index < tokens.Count; index++)
        {
            if (ReferenceEquals(tokens[index], target))
            {
                return index;
            }
        }

        return -1;
    }

    private static int FindHandlerEndIndex(IReadOnlyList<BlSyntaxToken> tokens, int startIndex)
    {
        for (var index = Math.Max(startIndex, 0); index < tokens.Count; index++)
        {
            var token = tokens[index];
            if (token.Kind != BlSyntaxKind.KeywordToken)
            {
                continue;
            }

            if (!string.Equals(token.ValueText, "end", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (IsHandlerTerminator(tokens, index))
            {
                return index;
            }
        }

        return -1;
    }

    private static bool IsHandlerTerminator(IReadOnlyList<BlSyntaxToken> tokens, int index)
    {
        var nextIndex = index + 1;
        if (nextIndex >= tokens.Count)
        {
            return true;
        }

        var next = tokens[nextIndex];
        if (next.Kind == BlSyntaxKind.EndOfFileToken)
        {
            return true;
        }

        return ContainsNewLine(next.LeadingTrivia);
    }

    private static bool ContainsNewLine(IReadOnlyList<BlSyntaxTrivia> trivia)
    {
        if (trivia is null)
        {
            return false;
        }

        for (var index = 0; index < trivia.Count; index++)
        {
            if (trivia[index].Kind == BlSyntaxKind.NewLineTrivia)
            {
                return true;
            }
        }

        return false;
    }


    private static Dictionary<BlLingoHandlerSymbolTable, string?> ComputeReturnTypes(
        IReadOnlyDictionary<BlLingoHandlerSymbolTable, IReadOnlyList<BlLingoHandlerCodeBlock>> map)
    {
        var result = new Dictionary<BlLingoHandlerSymbolTable, string?>();
        foreach (var entry in map)
        {
            var type = InferReturnType(entry.Value);
            if (!string.IsNullOrEmpty(type))
            {
                result[entry.Key] = type;
            }
        }

        return result;
    }

    private static IReadOnlyList<BlLingoHandlerCodeBlock> AnnotateCallResults(
        IReadOnlyList<BlLingoHandlerCodeBlock> blocks,
        IReadOnlyDictionary<BlLingoHandlerSymbolTable, string?> returnTypes,
        BlLingoSymbolTable symbols)
    {
        if (blocks.Count == 0)
        {
            return blocks;
        }

        var updated = new List<BlLingoHandlerCodeBlock>(blocks.Count);
        var changed = false;

        foreach (var block in blocks)
        {
            if (block.Kind == BlLingoHandlerCodeBlockKind.SendSprite &&
                block.Data is BlLingoSendSpriteBlockData sendData &&
                sendData.UsesResult)
            {
                var resolved = ResolveHandlerReturnType(symbols, sendData.TargetScriptName, sendData.HandlerName, BlLingoScriptKind.Behavior, returnTypes);
                if (!string.IsNullOrEmpty(resolved) && !string.Equals(resolved, sendData.ResultTypeName, StringComparison.Ordinal))
                {
                    var newData = sendData with { ResultTypeName = resolved };
                    updated.Add(new BlLingoHandlerCodeBlock(block.Kind, block.Tokens, newData, block.CommentText));
                    changed = true;
                    continue;
                }
            }

            if (block.Kind == BlLingoHandlerCodeBlockKind.MovieCall &&
                block.Data is BlLingoMovieCallBlockData movieData &&
                movieData.UsesResult)
            {
                var resolved = ResolveHandlerReturnType(symbols, movieData.TargetScriptName, movieData.HandlerName, BlLingoScriptKind.Movie, returnTypes);
                if (!string.IsNullOrEmpty(resolved) && !string.Equals(resolved, movieData.ResultTypeName, StringComparison.Ordinal))
                {
                    var newData = movieData with { ResultTypeName = resolved };
                    updated.Add(new BlLingoHandlerCodeBlock(block.Kind, block.Tokens, newData, block.CommentText));
                    changed = true;
                    continue;
                }
            }

            updated.Add(block);
        }

        return changed ? updated : blocks;
    }

    private static void RegisterReturnTypes(
        IReadOnlyDictionary<BlLingoHandlerSymbolTable, string?> returnTypes,
        IReadOnlyDictionary<BlLingoHandlerSymbolTable, BlLingoClassSymbolTable> owners)
    {
        if (returnTypes.Count == 0)
        {
            return;
        }

        foreach (var pair in returnTypes)
        {
            var handler = pair.Key;
            var resultType = pair.Value;
            if (handler is null || string.IsNullOrEmpty(resultType))
            {
                continue;
            }

            var handlerName = handler.Symbol.Name;
            if (string.IsNullOrEmpty(handlerName))
            {
                handlerName = handler.OriginalName;
            }

            if (string.IsNullOrEmpty(handlerName))
            {
                continue;
            }

            owners.TryGetValue(handler, out var classScope);
            var scriptName = classScope?.Symbol.Name;
            var scriptKind = classScope?.ScriptKind ?? BlLingoScriptKind.Unknown;

            BlLegacyHandlerReturnTypeRegistry.Register(scriptKind, scriptName, handlerName, resultType);
        }
    }

    private static string? ResolveHandlerReturnType(
        BlLingoSymbolTable symbols,
        string? scriptName,
        string handlerName,
        BlLingoScriptKind expectedKind,
        IReadOnlyDictionary<BlLingoHandlerSymbolTable, string?> returnTypes)
    {
        var target = FindHandler(symbols, scriptName, handlerName, expectedKind);
        if (target is not null && returnTypes.TryGetValue(target, out var type) && !string.IsNullOrEmpty(type))
        {
            return type;
        }

        var resolved = BlLegacyHandlerReturnTypeRegistry.Resolve(expectedKind, scriptName, handlerName);
        if (!string.IsNullOrEmpty(resolved))
        {
            return resolved;
        }

        if (target is not null && returnTypes.TryGetValue(target, out var fallback))
        {
            return fallback;
        }

        return null;
    }

    private static BlLingoHandlerSymbolTable? FindHandler(
        BlLingoSymbolTable symbols,
        string? scriptName,
        string handlerName,
        BlLingoScriptKind expectedKind)
    {
        if (!string.IsNullOrEmpty(scriptName))
        {
            if (symbols.ClassScopes.TryGetValue(scriptName, out var classScope) &&
                classScope.Handlers.TryGetValue(handlerName, out var scopedHandler))
            {
                return scopedHandler;
            }

            if (string.Equals(symbols.MovieScript.Symbol.Name, scriptName, StringComparison.OrdinalIgnoreCase) &&
                symbols.MovieScript.Handlers.TryGetValue(handlerName, out var movieHandler))
            {
                return movieHandler;
            }

            foreach (var scope in symbols.ClassScopes.Values)
            {
                if (!string.Equals(scope.Symbol.Name, scriptName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (scope.Handlers.TryGetValue(handlerName, out var handler))
                {
                    return handler;
                }
            }
        }

        if (expectedKind == BlLingoScriptKind.Movie)
        {
            if (symbols.MovieScript.Handlers.TryGetValue(handlerName, out var movieHandler))
            {
                return movieHandler;
            }

            foreach (var scope in symbols.ClassScopes.Values)
            {
                if (!scope.IsMovieScript)
                {
                    continue;
                }

                if (scope.Handlers.TryGetValue(handlerName, out var handler))
                {
                    return handler;
                }
            }
        }
        else if (expectedKind == BlLingoScriptKind.Behavior)
        {
            foreach (var scope in symbols.ClassScopes.Values)
            {
                if (scope.IsMovieScript)
                {
                    continue;
                }

                if (scope.Handlers.TryGetValue(handlerName, out var handler))
                {
                    return handler;
                }
            }
        }

        if (symbols.MovieScript.Handlers.TryGetValue(handlerName, out var fallbackMovieHandler))
        {
            return fallbackMovieHandler;
        }

        foreach (var scope in symbols.ClassScopes.Values)
        {
            if (scope.Handlers.TryGetValue(handlerName, out var handler))
            {
                return handler;
            }
        }

        return null;
    }

    private static string? InferReturnType(IReadOnlyList<BlLingoHandlerCodeBlock> blocks)
    {
        string? inferred = null;
        foreach (var block in blocks)
        {
            var candidate = BlLegacyReturnTypeHelper.InferBlockResult(block);
            if (string.IsNullOrEmpty(candidate))
            {
                continue;
            }

            inferred = BlLegacyReturnTypeHelper.Merge(inferred, candidate);
        }

        return inferred;
    }

    private sealed class HandlerBody
    {
        public static HandlerBody Empty { get; } = new(Array.Empty<BlSyntaxToken>(), Array.Empty<BlSyntaxTrivia>());

        public HandlerBody(IReadOnlyList<BlSyntaxToken> tokens, IReadOnlyList<BlSyntaxTrivia> endTrivia)
        {
            Tokens = tokens ?? Array.Empty<BlSyntaxToken>();
            EndTrivia = endTrivia ?? Array.Empty<BlSyntaxTrivia>();
        }

        public IReadOnlyList<BlSyntaxToken> Tokens { get; }

        public IReadOnlyList<BlSyntaxTrivia> EndTrivia { get; }
    }
}
