using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BlingoEngine.Legacy.Lingo.CodeGen;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.Analysis.Passes;

/// <summary>
/// Builds the legacy type model, extracts hints, and applies resolved property and parameter types.
/// </summary>
public sealed class BlLegacyTypeAnalysisPass : BlLingoAnalysisPass
{
    public const string TypeCollectionKey = "LegacyTypeCollection";

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

    public BlLegacyTypeAnalysisPass()
        : base("LegacyTypeAnalysis")
    {
    }

    public override void Execute(BlLingoAnalysisContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var symbols = context.Symbols;
        var collection = BuildTypeCollection(symbols);
        context.SetData(TypeCollectionKey, collection);

        var handlerBlocks = TryGetHandlerBlocks(context);
        var handlerReturnTypes = TryGetHandlerReturnTypes(context);

        foreach (var scope in collection.Scopes)
        {
            foreach (var handler in scope.Handlers)
            {
                AssignEventParameterTypes(collection, handler);

                if (handlerBlocks is null || handler.Symbol is null ||
                    !handlerBlocks.TryGetValue(handler.Symbol, out var blocksForHandler))
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
                            ProcessExpressionBlock(symbols, collection, scope, handler, block, handlerReturnTypes);
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

        collection.ApplyResolvedTypes();
    }

    private static BlLegacyTypeCollection BuildTypeCollection(BlLingoSymbolTable symbols)
    {
        var collection = new BlLegacyTypeCollection();

        foreach (var classScope in EnumerateClasses(symbols))
        {
            var scope = collection.GetOrAddScope(classScope);

            foreach (var property in classScope.Properties.Values)
            {
                if (property is not null)
                {
                    scope.RegisterProperty(property);
                }
            }

            foreach (var handler in classScope.Handlers.Values)
            {
                if (handler is null)
                {
                    continue;
                }

                var handlerScope = scope.RegisterHandler(handler);
                collection.RegisterHandler(handlerScope);
            }
        }

        return collection;
    }

    private static IReadOnlyDictionary<BlLingoHandlerSymbolTable, IReadOnlyList<BlLingoHandlerCodeBlock>>? TryGetHandlerBlocks(
        BlLingoAnalysisContext context)
    {
        if (context.TryGetData(BlLingoHandlerCodeBlockPass.HandlerCodeBlocksKey,
                out IReadOnlyDictionary<BlLingoHandlerSymbolTable, IReadOnlyList<BlLingoHandlerCodeBlock>>? blocks) &&
            blocks is not null)
        {
            return blocks;
        }

        return null;
    }

    private static IReadOnlyDictionary<BlLingoHandlerSymbolTable, string?>? TryGetHandlerReturnTypes(
        BlLingoAnalysisContext context)
    {
        if (context.TryGetData(BlLingoHandlerCodeBlockPass.HandlerReturnTypesKey,
                out IReadOnlyDictionary<BlLingoHandlerSymbolTable, string?>? returnTypes) &&
            returnTypes is not null)
        {
            return returnTypes;
        }

        return null;
    }

    private static IEnumerable<BlLingoClassSymbolTable> EnumerateClasses(BlLingoSymbolTable symbols)
    {
        yield return symbols.MovieScript;
        foreach (var scope in symbols.ClassScopes.Values)
        {
            yield return scope;
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
        BlLingoSymbolTable symbols,
        BlLegacyTypeCollection collection,
        BlLegacyTypeCollection.Scope scope,
        BlLegacyTypeCollection.HandlerScope handler,
        BlLingoHandlerCodeBlock block,
        IReadOnlyDictionary<BlLingoHandlerSymbolTable, string?>? handlerReturnTypes)
    {
        if (block is null || block.Tokens.Count == 0)
        {
            return;
        }

        if (!TryExtractAssignment(block.Tokens, out var name, out var valueTokens))
        {
            return;
        }

        var invocationType = DetermineInvocationReturnType(
            symbols,
            collection,
            scope,
            handler,
            valueTokens,
            handlerReturnTypes);
        if (invocationType.Length > 0)
        {
            MergePropertyOrParameter(collection, scope, handler, name, invocationType);
        }

        var expression = BlLegacyExpressionConverter.Convert(valueTokens);
        var typeName = BlLegacyTypeUtilities.DetermineExpressionType(expression);
        if (typeName.Length == 0)
        {
            if (TryExtractSimpleIdentifier(expression, out var identifier) &&
                scope.TryGetProperty(name, out var propertyTarget))
            {
                if (scope.TryGetProperty(identifier, out var sourceProperty))
                {
                    propertyTarget.LinkTo(sourceProperty);
                }
                else if (handler.TryGetParameter(identifier, out var parameterTarget))
                {
                    propertyTarget.LinkTo(parameterTarget);
                }
            }

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

    private static string DetermineInvocationReturnType(
        BlLingoSymbolTable symbols,
        BlLegacyTypeCollection collection,
        BlLegacyTypeCollection.Scope scope,
        BlLegacyTypeCollection.HandlerScope handler,
        IReadOnlyList<BlSyntaxToken> valueTokens,
        IReadOnlyDictionary<BlLingoHandlerSymbolTable, string?>? handlerReturnTypes)
    {
        if (!TryParseInvocation(valueTokens, out var targetName, out var handlerName))
        {
            return string.Empty;
        }

        BlLegacyTypeCollection.Scope? resolvedScope = null;
        BlLingoScriptKind scriptKind;
        string? scriptName;

        if (string.Equals(targetName, "me", StringComparison.OrdinalIgnoreCase))
        {
            resolvedScope = scope;
            scriptKind = scope.ScriptKind;
            scriptName = scope.Name;
        }
        else
        {
            var symbol = ResolveSymbol(symbols, scope, handler, targetName);
            if (symbol is null)
            {
                return string.Empty;
            }

            var candidateType = symbol.ResolvedTypeName;
            if (string.IsNullOrWhiteSpace(candidateType))
            {
                candidateType = symbol.TypeCode;
            }

            var normalizedScript = ExtractScriptName(candidateType);
            if (normalizedScript.Length == 0)
            {
                return string.Empty;
            }

            if (!collection.TryFindScopeByName(normalizedScript, out resolvedScope) || resolvedScope is null)
            {
                return string.Empty;
            }

            scriptKind = resolvedScope.ScriptKind;
            scriptName = resolvedScope.Name;
        }

        if (resolvedScope is null)
        {
            return string.Empty;
        }

        var targetHandler = collection.FindHandler(scriptName, handlerName, scriptKind);
        string? resolvedType = null;

        if (targetHandler is not null &&
            targetHandler.Symbol is not null &&
            handlerReturnTypes is not null &&
            handlerReturnTypes.TryGetValue(targetHandler.Symbol, out var inferred) &&
            !string.IsNullOrWhiteSpace(inferred))
        {
            resolvedType = inferred;
        }

        if (string.IsNullOrWhiteSpace(resolvedType))
        {
            resolvedType = BlLegacyHandlerReturnTypeRegistry.Resolve(scriptKind, scriptName, handlerName);
        }

        if (string.IsNullOrWhiteSpace(resolvedType) && targetHandler is not null)
        {
            var canonical = BlLingoHandlerFacts.GetClassification(handlerName).CanonicalName;
            resolvedType = BlLegacyHandlerReturnTypeRegistry.Resolve(scriptKind, scriptName, canonical);
        }

        if (string.IsNullOrWhiteSpace(resolvedType))
        {
            var sanitized = BlCSharpName.SanitizeIdentifier(handlerName);
            resolvedType = BlLegacyHandlerReturnTypeRegistry.Resolve(scriptKind, scriptName, sanitized);
        }

        var normalizedType = BlLegacyTypeUtilities.NormalizeTypeName(resolvedType);
        return normalizedType;
    }

    private static bool TryParseInvocation(
        IReadOnlyList<BlSyntaxToken> tokens,
        out string targetName,
        out string handlerName)
    {
        targetName = string.Empty;
        handlerName = string.Empty;

        if (tokens.Count < 4)
        {
            return false;
        }

        var index = 0;
        while (index < tokens.Count && tokens[index].Kind == BlSyntaxKind.LeftParenthesisToken)
        {
            index++;
        }

        if (!TryGetToken(tokens, index, out var targetToken) || !IsIdentifierLike(targetToken))
        {
            return false;
        }

        targetName = targetToken.ValueText;
        index++;

        if (!TryGetToken(tokens, index, out var separator))
        {
            return false;
        }

        if (separator.Kind == BlSyntaxKind.QuestionToken)
        {
            index++;
            if (!TryGetToken(tokens, index, out separator))
            {
                return false;
            }
        }

        if (separator.Kind != BlSyntaxKind.PeriodToken)
        {
            return false;
        }

        index++;
        if (!TryGetToken(tokens, index, out var handlerToken) || !IsIdentifierLike(handlerToken))
        {
            return false;
        }

        handlerName = handlerToken.ValueText;
        index++;

        if (!TryGetToken(tokens, index, out var openParen) || openParen.Kind != BlSyntaxKind.LeftParenthesisToken)
        {
            return false;
        }

        return true;
    }

    private static BlCodeSymbol? ResolveSymbol(
        BlLingoSymbolTable symbols,
        BlLegacyTypeCollection.Scope scope,
        BlLegacyTypeCollection.HandlerScope handler,
        string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var handlerSymbol = handler.Symbol;
        if (handlerSymbol is not null)
        {
            if (handlerSymbol.Locals.TryGetValue(name, out var local))
            {
                return local;
            }

            if (handlerSymbol.Parameters.TryGetValue(name, out var parameter))
            {
                return parameter;
            }
        }

        var classScope = scope.Symbol;
        if (classScope.Properties.TryGetValue(name, out var property))
        {
            return property;
        }

        if (!scope.IsMovieScript && symbols.MovieScript.Properties.TryGetValue(name, out var movieProperty))
        {
            return movieProperty;
        }

        if (symbols.Globals.TryGetValue(name, out var global))
        {
            return global;
        }

        return null;
    }

    private static string ExtractScriptName(string? typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            return string.Empty;
        }

        var candidate = typeName.Trim();
        if (candidate.StartsWith("global::", StringComparison.Ordinal))
        {
            candidate = candidate[8..];
        }

        if (candidate.EndsWith("?", StringComparison.Ordinal))
        {
            candidate = candidate[..^1];
        }

        var genericIndex = candidate.IndexOf('<');
        if (genericIndex >= 0)
        {
            candidate = candidate[..genericIndex];
        }

        var lastDot = candidate.LastIndexOf('.');
        if (lastDot >= 0)
        {
            candidate = candidate[(lastDot + 1)..];
        }

        return candidate;
    }

    private static bool IsIdentifierLike(BlSyntaxToken token)
    {
        return token.Kind is BlSyntaxKind.IdentifierToken or BlSyntaxKind.KeywordToken or BlSyntaxKind.SymbolToken;
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

    private static bool TryExtractSimpleIdentifier(string expression, out string identifier)
    {
        identifier = string.Empty;
        if (string.IsNullOrWhiteSpace(expression))
        {
            return false;
        }

        var trimmed = expression.Trim();
        if (Regex.IsMatch(trimmed, "^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.CultureInvariant))
        {
            identifier = trimmed;
            return true;
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
