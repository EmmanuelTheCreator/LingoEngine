using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.Analysis.Passes;

/// <summary>
/// Aggregates class member metadata such as property declarations, handler order, and global usage.
/// </summary>
public sealed class BlLegacyClassMemberPass : BlLingoAnalysisPass
{
    public const string ClassMemberInfoKey = "Legacy.ClassMembers";

    public BlLegacyClassMemberPass()
        : base("LegacyClassMembers")
    {
    }

    public override void Execute(BlLingoAnalysisContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var tokens = context.Tokens;
        var symbols = context.Symbols;
        var classData = new Dictionary<BlLingoClassSymbolTable, ClassData>();

        foreach (var scope in EnumerateClasses(symbols))
        {
            classData[scope] = new ClassData(scope);
        }

        if (classData.Count == 0)
        {
            context.SetData<IReadOnlyDictionary<BlLingoClassSymbolTable, BlLegacyClassMemberInfo>>(ClassMemberInfoKey, new Dictionary<BlLingoClassSymbolTable, BlLegacyClassMemberInfo>());
            return;
        }

        var currentClass = symbols.MovieScript;
        for (var index = 0; index < tokens.Count; index++)
        {
            var token = tokens[index];
            if (token.Kind != BlSyntaxKind.KeywordToken)
            {
                continue;
            }

            var keyword = token.ValueText;
            if (keyword.Equals("script", StringComparison.OrdinalIgnoreCase))
            {
                if (TryGetClassName(tokens, index + 1, out var className) &&
                    symbols.ClassScopes.TryGetValue(className, out var scope))
                {
                    currentClass = scope;
                }
                else
                {
                    currentClass = symbols.MovieScript;
                }

                continue;
            }

            if (!classData.TryGetValue(currentClass, out var data))
            {
                continue;
            }

            if (keyword.Equals("property", StringComparison.OrdinalIgnoreCase))
            {
                index = CollectPropertyDeclarations(tokens, index + 1, data);
                continue;
            }

            if (keyword.Equals("global", StringComparison.OrdinalIgnoreCase))
            {
                if (HasIdentifiersBeforeNewLine(tokens, index + 1))
                {
                    data.HasGlobalDeclarations = true;
                }

                continue;
            }

            if (!keyword.Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!TryGetHandlerNameToken(tokens, index + 1, out var handlerToken))
            {
                continue;
            }

            if (!data.Scope.Handlers.TryGetValue(handlerToken.ValueText, out var handler) || handler is null)
            {
                continue;
            }

            if (BlLegacyHandlerFilters.ShouldSkipHandler(handler))
            {
                continue;
            }

            data.TryAddHandler(handler);
        }

        foreach (var entry in classData.Values)
        {
            EnsureDeclaredProperties(entry);
        }

        var results = new Dictionary<BlLingoClassSymbolTable, BlLegacyClassMemberInfo>();
        foreach (var pair in classData)
        {
            var data = pair.Value;
            var orderedProperties = new List<PropertyState>(data.Properties.Values);
            orderedProperties.Sort(static (left, right) => left.Order.CompareTo(right.Order));

            var propertyInfos = new List<BlLegacyPropertyInfo>(orderedProperties.Count);
            foreach (var property in orderedProperties)
            {
                var typeName = ResolvePropertyType(data.Scope, property);
                propertyInfos.Add(new BlLegacyPropertyInfo(property.Name, typeName, property.Comment));
            }

            results[pair.Key] = new BlLegacyClassMemberInfo(
                new ReadOnlyCollection<BlLegacyPropertyInfo>(propertyInfos),
                new ReadOnlyCollection<BlLingoHandlerSymbolTable>(data.HandlerOrder),
                data.HasGlobalDeclarations);
        }

        context.SetData<IReadOnlyDictionary<BlLingoClassSymbolTable, BlLegacyClassMemberInfo>>(ClassMemberInfoKey, results);
    }

    private static IEnumerable<BlLingoClassSymbolTable> EnumerateClasses(BlLingoSymbolTable symbols)
    {
        yield return symbols.MovieScript;
        foreach (var scope in symbols.ClassScopes.Values)
        {
            yield return scope;
        }
    }

    private static void EnsureDeclaredProperties(ClassData data)
    {
        foreach (var property in data.Scope.Properties.Values)
        {
            if (property is null)
            {
                continue;
            }

            var order = property.Declarations.Count > 0 ? property.Declarations[0].Span.Start : int.MaxValue;
            data.GetOrAddProperty(property.Name, order, property);
        }
    }

    private static int CollectPropertyDeclarations(IReadOnlyList<BlSyntaxToken> tokens, int startIndex, ClassData data)
    {
        var lastIndex = startIndex - 1;
        for (var index = startIndex; index < tokens.Count; index++)
        {
            var current = tokens[index];
            if (ContainsNewLine(current.LeadingTrivia))
            {
                break;
            }

            if (current.Kind == BlSyntaxKind.IdentifierToken)
            {
                var name = current.ValueText;
                if (!string.IsNullOrWhiteSpace(name) && data.Scope.Properties.TryGetValue(name, out var symbol))
                {
                    var info = data.GetOrAddProperty(symbol.Name, current.Span.Start, symbol);
                    var comment = ExtractTrailingComment(current);
                    if (!string.IsNullOrWhiteSpace(comment) && info.Comment is null)
                    {
                        info.Comment = comment;
                    }
                }

                lastIndex = index;
                continue;
            }

            if (current.Kind == BlSyntaxKind.CommaToken)
            {
                lastIndex = index;
                continue;
            }

            break;
        }

        return lastIndex;
    }

    private static string ResolvePropertyType(BlLingoClassSymbolTable scope, PropertyState property)
    {
        if (property is null)
        {
            return "object";
        }

        var symbol = property.Symbol;
        if (symbol is null && scope.Properties.TryGetValue(property.Name, out var lookup) && lookup is not null)
        {
            symbol = lookup;
            property.Symbol = lookup;
        }

        var resolved = symbol?.ResolvedTypeName;
        if (string.IsNullOrWhiteSpace(resolved))
        {
            return "object";
        }

        var normalized = resolved.Trim();
        return string.Equals(normalized, "object?", StringComparison.Ordinal) ? "object" : normalized;
    }

    private static string? ExtractTrailingComment(BlSyntaxToken token)
    {
        if (token.TrailingTrivia is null || token.TrailingTrivia.Count == 0)
        {
            return null;
        }

        string? comment = null;
        foreach (var trivia in token.TrailingTrivia)
        {
            if (trivia.Kind == BlSyntaxKind.CommentTrivia)
            {
                comment = trivia.ValueText?.Trim();
            }
        }

        return string.IsNullOrWhiteSpace(comment) ? null : comment;
    }

    private static bool HasIdentifiersBeforeNewLine(IReadOnlyList<BlSyntaxToken> tokens, int startIndex)
    {
        for (var index = startIndex; index < tokens.Count; index++)
        {
            var token = tokens[index];
            if (ContainsNewLine(token.LeadingTrivia))
            {
                break;
            }

            if (token.Kind == BlSyntaxKind.IdentifierToken)
            {
                return true;
            }

            if (token.Kind != BlSyntaxKind.CommaToken)
            {
                break;
            }
        }

        return false;
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

    private static bool TryGetClassName(IReadOnlyList<BlSyntaxToken> tokens, int index, out string name)
    {
        name = string.Empty;
        if (!TryGetToken(tokens, index, out var token))
        {
            return false;
        }

        if (ContainsNewLine(token.LeadingTrivia))
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

    private static bool TryGetHandlerNameToken(
        IReadOnlyList<BlSyntaxToken> tokens,
        int index,
        out BlSyntaxToken handlerToken)
    {
        handlerToken = default!;
        if (!TryGetToken(tokens, index, out var token))
        {
            return false;
        }

        if (ContainsNewLine(token.LeadingTrivia))
        {
            return false;
        }

        if (token.Kind is BlSyntaxKind.IdentifierToken or BlSyntaxKind.KeywordToken)
        {
            handlerToken = token;
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

    private sealed class ClassData
    {
        private readonly HashSet<BlLingoHandlerSymbolTable> _seenHandlers = new();

        public ClassData(BlLingoClassSymbolTable scope)
        {
            Scope = scope ?? throw new ArgumentNullException(nameof(scope));
            Properties = new Dictionary<string, PropertyState>(StringComparer.OrdinalIgnoreCase);
            HandlerOrder = new List<BlLingoHandlerSymbolTable>();
        }

        public BlLingoClassSymbolTable Scope { get; }

        public Dictionary<string, PropertyState> Properties { get; }

        public List<BlLingoHandlerSymbolTable> HandlerOrder { get; }

        public bool HasGlobalDeclarations { get; set; }

        public PropertyState GetOrAddProperty(string name, int order, BlCodeSymbol? symbol)
        {
            if (!Properties.TryGetValue(name, out var info))
            {
                info = new PropertyState(name, order, symbol);
                Properties[name] = info;
            }
            else if (info.Order == int.MaxValue)
            {
                info.Order = order;
            }

            if (info.Symbol is null && symbol is not null)
            {
                info.Symbol = symbol;
            }

            return info;
        }

        public void TryAddHandler(BlLingoHandlerSymbolTable handler)
        {
            if (handler is null)
            {
                return;
            }

            if (_seenHandlers.Add(handler))
            {
                HandlerOrder.Add(handler);
            }
        }
    }

    private sealed class PropertyState
    {
        public PropertyState(string name, int order, BlCodeSymbol? symbol)
        {
            Name = name;
            Order = order;
            Symbol = symbol;
        }

        public string Name { get; }

        public int Order { get; set; }

        public BlCodeSymbol? Symbol { get; set; }

        public string? Comment { get; set; }
    }
}
