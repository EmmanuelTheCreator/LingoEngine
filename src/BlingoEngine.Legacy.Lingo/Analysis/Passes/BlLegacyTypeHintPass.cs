using System;
using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.CodeGen;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.Analysis.Passes;

public sealed class BlLegacyTypeHintPass : BlLingoAnalysisPass
{
    private static readonly HashSet<string> s_mouseHandlers = new(StringComparer.OrdinalIgnoreCase)
    {
        "MouseDown",
        "MouseUp",
        "MouseWithin",
        "MouseLeave",
        "MouseMove",
        "MouseEnter",
        "MouseExit",
        "MouseWheel",
    };

    private static readonly HashSet<string> s_keyboardHandlers = new(StringComparer.OrdinalIgnoreCase)
    {
        "KeyDown",
        "KeyUp",
    };

    public BlLegacyTypeHintPass()
        : base("LegacyTypeHints")
    {
    }

    public override void Execute(BlLingoAnalysisContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.TryGetData(BlLegacyTypeCollectionPass.TypeCollectionKey, out BlLegacyTypeCollection? collection) ||
            collection is null)
        {
            return;
        }

        IReadOnlyDictionary<BlLingoHandlerSymbolTable, IReadOnlyList<BlLingoHandlerCodeBlock>>? handlerBlocks = null;
        if (context.TryGetData(BlLingoHandlerCodeBlockPass.HandlerCodeBlocksKey, out IReadOnlyDictionary<BlLingoHandlerSymbolTable, IReadOnlyList<BlLingoHandlerCodeBlock>>? blocks) &&
            blocks is not null)
        {
            handlerBlocks = blocks;
        }

        foreach (var scope in collection.Scopes)
        {
            foreach (var handler in scope.Handlers)
            {
                AssignEventParameterTypes(collection, handler);

                if (handlerBlocks is null || handler.Symbol is null || !handlerBlocks.TryGetValue(handler.Symbol, out var blocksForHandler))
                {
                    continue;
                }

                foreach (var block in blocksForHandler)
                {
                    switch (block.Kind)
                    {
                        case BlLingoHandlerCodeBlockKind.Put when block.Data is BlLingoPutBlockData put:
                            ProcessPutBlock(collection, scope, handler, put);
                            break;
                        case BlLingoHandlerCodeBlockKind.Expression:
                            ProcessExpressionBlock(collection, scope, handler, block);
                            break;
                        case BlLingoHandlerCodeBlockKind.SendSprite when block.Data is BlLingoSendSpriteBlockData send:
                            ProcessSendSpriteBlock(collection, send);
                            break;
                        case BlLingoHandlerCodeBlockKind.MovieCall when block.Data is BlLingoMovieCallBlockData movie:
                            ProcessMovieCallBlock(collection, movie);
                            break;
                    }
                }
            }
        }
    }

    private static void AssignEventParameterTypes(BlLegacyTypeCollection collection, BlLegacyTypeCollection.HandlerScope handler)
    {
        var canonicalName = handler.CanonicalName;
        if (string.IsNullOrWhiteSpace(canonicalName))
        {
            return;
        }

        string? typeName = null;
        if (s_mouseHandlers.Contains(canonicalName))
        {
            typeName = "BlingoMouseEvent";
        }
        else if (s_keyboardHandlers.Contains(canonicalName))
        {
            typeName = "BlingoKeyEvent";
        }

        if (typeName is null)
        {
            return;
        }

        if (!handler.TryGetFirstNonSelfParameter(out var parameter))
        {
            return;
        }

        parameter.AddHint(typeName);
        var index = handler.GetArgumentIndex(parameter);
        if (index >= 0)
        {
            collection.AddSharedHint(handler.Owner.ScriptKind, handler.Owner.Name, handler.LookupName, index, typeName);
        }
    }

    private static void ProcessPutBlock(
        BlLegacyTypeCollection collection,
        BlLegacyTypeCollection.Scope scope,
        BlLegacyTypeCollection.HandlerScope handler,
        BlLingoPutBlockData block)
    {
        if (block is null || block.Kind != BlLingoPutAssignmentKind.Direct)
        {
            return;
        }

        var target = BlLegacyTypeUtilities.NormalizePropertyTarget(block.TargetExpression);
        if (string.IsNullOrEmpty(target))
        {
            return;
        }

        var typeName = BlLegacyTypeUtilities.DetermineExpressionType(block.ValueExpression);
        if (typeName.Length == 0)
        {
            return;
        }

        MergePropertyOrParameter(collection, scope, handler, target, typeName);
    }

    private static void ProcessExpressionBlock(
        BlLegacyTypeCollection collection,
        BlLegacyTypeCollection.Scope scope,
        BlLegacyTypeCollection.HandlerScope handler,
        BlLingoHandlerCodeBlock block)
    {
        if (block is null || block.Tokens.Count == 0)
        {
            return;
        }

        if (!TryExtractAssignment(block.Tokens, out var name, out var valueTokens))
        {
            return;
        }

        var expression = BlLegacyExpressionConverter.Convert(valueTokens);
        var typeName = BlLegacyTypeUtilities.DetermineExpressionType(expression);
        if (typeName.Length == 0)
        {
            return;
        }

        MergePropertyOrParameter(collection, scope, handler, name, typeName);
    }

    private static void ProcessSendSpriteBlock(BlLegacyTypeCollection collection, BlLingoSendSpriteBlockData data)
    {
        if (data is null || data.Arguments.Count == 0)
        {
            return;
        }

        var targetHandler = collection.FindHandler(data.TargetScriptName, data.HandlerName, BlLingoScriptKind.Behavior);
        if (targetHandler is null)
        {
            RegisterArgumentHints(collection, data.TargetScriptName, BlLingoScriptKind.Behavior, data.HandlerName, data.Arguments);
            return;
        }

        ApplyArgumentTypes(collection, targetHandler, data.Arguments);
    }

    private static void ProcessMovieCallBlock(BlLegacyTypeCollection collection, BlLingoMovieCallBlockData data)
    {
        if (data is null || data.Arguments.Count == 0)
        {
            return;
        }

        var targetHandler = collection.FindHandler(data.TargetScriptName, data.HandlerName, BlLingoScriptKind.Movie);
        if (targetHandler is null)
        {
            RegisterArgumentHints(collection, data.TargetScriptName, BlLingoScriptKind.Movie, data.HandlerName, data.Arguments);
            return;
        }

        ApplyArgumentTypes(collection, targetHandler, data.Arguments);
    }

    private static void ApplyArgumentTypes(
        BlLegacyTypeCollection collection,
        BlLegacyTypeCollection.HandlerScope handler,
        IReadOnlyList<string> arguments)
    {
        if (arguments.Count == 0)
        {
            return;
        }

        for (var index = 0; index < arguments.Count; index++)
        {
            var typeName = BlLegacyTypeUtilities.DetermineExpressionType(arguments[index]);
            if (typeName.Length == 0)
            {
                continue;
            }

            handler.AddArgumentHint(index, typeName);
            collection.AddSharedHint(handler.Owner.ScriptKind, handler.Owner.Name, handler.LookupName, index, typeName);
        }
    }

    private static void RegisterArgumentHints(
        BlLegacyTypeCollection collection,
        string? scriptName,
        BlLingoScriptKind scriptKind,
        string handlerName,
        IReadOnlyList<string> arguments)
    {
        if (arguments.Count == 0 || string.IsNullOrWhiteSpace(handlerName))
        {
            return;
        }

        for (var index = 0; index < arguments.Count; index++)
        {
            var typeName = BlLegacyTypeUtilities.DetermineExpressionType(arguments[index]);
            if (typeName.Length == 0)
            {
                continue;
            }

            collection.AddSharedHint(scriptKind, scriptName, handlerName, index, typeName);
        }
    }

    private static void MergePropertyOrParameter(
        BlLegacyTypeCollection collection,
        BlLegacyTypeCollection.Scope scope,
        BlLegacyTypeCollection.HandlerScope handler,
        string name,
        string typeName)
    {
        if (scope.TryGetProperty(name, out var property))
        {
            property.AddHint(typeName);
            return;
        }

        if (handler.TryGetParameter(name, out var parameter))
        {
            parameter.AddHint(typeName);
            var index = handler.GetArgumentIndex(parameter);
            if (index >= 0)
            {
                collection.AddSharedHint(scope.ScriptKind, scope.Name, handler.LookupName, index, typeName);
            }
        }
    }

    private static bool TryExtractAssignment(
        IReadOnlyList<BlSyntaxToken> tokens,
        out string name,
        out IReadOnlyList<BlSyntaxToken> valueTokens)
    {
        name = string.Empty;
        valueTokens = Array.Empty<BlSyntaxToken>();
        if (tokens.Count == 0)
        {
            return false;
        }

        var equalsIndex = BlLegacyHandlerTokenUtilities.FindOperator(tokens, "=");
        if (equalsIndex < 0)
        {
            return false;
        }

        if (!TryExtractName(tokens, equalsIndex - 1, out name))
        {
            return false;
        }

        valueTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, equalsIndex + 1, tokens.Count - (equalsIndex + 1));
        return valueTokens.Count > 0;
    }

    private static bool TryExtractName(IReadOnlyList<BlSyntaxToken> tokens, int index, out string name)
    {
        name = string.Empty;
        if (!TryGetToken(tokens, index, out var token))
        {
            return false;
        }

        if (token.Kind is BlSyntaxKind.IdentifierToken or BlSyntaxKind.SymbolToken or BlSyntaxKind.StringLiteralToken)
        {
            if (!string.IsNullOrWhiteSpace(token.ValueText))
            {
                name = token.ValueText;
                return true;
            }
        }

        return false;
    }

    private static bool TryGetToken(IReadOnlyList<BlSyntaxToken> tokens, int index, out BlSyntaxToken token)
    {
        if (tokens is not null && index >= 0 && index < tokens.Count)
        {
            token = tokens[index];
            return true;
        }

        token = default!;
        return false;
    }
}

