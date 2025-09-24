using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BlingoEngine.Legacy.Lingo.CodeGen;
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

        IReadOnlyDictionary<BlLingoHandlerSymbolTable, IReadOnlyList<BlLingoHandlerCodeBlock>>? handlerBlocks = null;
        if (context.TryGetData(BlLingoHandlerCodeBlockPass.HandlerCodeBlocksKey, out IReadOnlyDictionary<BlLingoHandlerSymbolTable, IReadOnlyList<BlLingoHandlerCodeBlock>>? retrieved))
        {
            handlerBlocks = retrieved;
        }

        var results = new Dictionary<BlLingoClassSymbolTable, BlLegacyClassMemberInfo>();
        foreach (var pair in classData)
        {
            var data = pair.Value;
            InferPropertyTypes(data, handlerBlocks);

            var orderedProperties = new List<PropertyState>(data.Properties.Values);
            orderedProperties.Sort(static (left, right) => left.Order.CompareTo(right.Order));

            var propertyInfos = new List<BlLegacyPropertyInfo>(orderedProperties.Count);
            foreach (var property in orderedProperties)
            {
                propertyInfos.Add(new BlLegacyPropertyInfo(property.Name, property.Type, property.Comment));
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
            data.GetOrAddProperty(property.Name, order);
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
                    var info = data.GetOrAddProperty(symbol.Name, current.Span.Start);
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

    private static void InferPropertyTypes(
        ClassData data,
        IReadOnlyDictionary<BlLingoHandlerSymbolTable, IReadOnlyList<BlLingoHandlerCodeBlock>>? handlerBlocks)
    {
        foreach (var property in data.Properties.Values)
        {
            property.Type = "object";
        }

        if (handlerBlocks is null || handlerBlocks.Count == 0)
        {
            return;
        }

        foreach (var handler in data.Scope.Handlers.Values)
        {
            if (handler is null || !handlerBlocks.TryGetValue(handler, out var blocks))
            {
                continue;
            }

            foreach (var block in blocks)
            {
                switch (block.Kind)
                {
                    case BlLingoHandlerCodeBlockKind.Put when block.Data is BlLingoPutBlockData put:
                        ProcessPutBlock(data.Properties, put);
                        break;
                    case BlLingoHandlerCodeBlockKind.Expression:
                        ProcessExpressionBlock(data.Properties, block);
                        break;
                }
            }
        }
    }

    private static void ProcessPutBlock(
        IDictionary<string, PropertyState> propertyMap,
        BlLingoPutBlockData data)
    {
        if (data is null || data.Kind != BlLingoPutAssignmentKind.Direct)
        {
            return;
        }

        var target = NormalizePropertyTarget(data.TargetExpression);
        if (string.IsNullOrEmpty(target) || !propertyMap.TryGetValue(target, out var property))
        {
            return;
        }

        var typeName = DeterminePropertyType(data.ValueExpression);
        if (typeName.Length == 0)
        {
            return;
        }

        property.Type = MergePropertyTypes(property.Type, typeName);
    }

    private static void ProcessExpressionBlock(
        IDictionary<string, PropertyState> propertyMap,
        BlLingoHandlerCodeBlock block)
    {
        if (block is null || block.Tokens.Count == 0)
        {
            return;
        }

        if (!TryExtractAssignment(block.Tokens, out var propertyName, out var valueTokens))
        {
            return;
        }

        if (!propertyMap.TryGetValue(propertyName, out var property))
        {
            return;
        }

        var expression = BlLegacyExpressionConverter.Convert(valueTokens);
        var typeName = DeterminePropertyType(expression);
        if (typeName.Length == 0)
        {
            return;
        }

        property.Type = MergePropertyTypes(property.Type, typeName);
    }

    private static string NormalizePropertyTarget(string? target)
    {
        if (string.IsNullOrWhiteSpace(target))
        {
            return string.Empty;
        }

        var trimmed = target.Trim();
        if (trimmed.StartsWith("this.", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed[5..];
        }

        return trimmed;
    }

    private static string NormalizeTypeName(string? typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            return string.Empty;
        }

        var trimmed = typeName.Trim();
        if (string.Equals(trimmed, "object?", StringComparison.Ordinal))
        {
            return "object";
        }

        return trimmed;
    }

    private static string MergePropertyTypes(string currentType, string candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return currentType;
        }

        if (string.IsNullOrWhiteSpace(currentType) || string.Equals(currentType, "object", StringComparison.Ordinal))
        {
            return candidate;
        }

        if (string.Equals(currentType, candidate, StringComparison.Ordinal))
        {
            return currentType;
        }

        if ((string.Equals(currentType, "int", StringComparison.Ordinal) && string.Equals(candidate, "double", StringComparison.Ordinal)) ||
            (string.Equals(currentType, "double", StringComparison.Ordinal) && string.Equals(candidate, "int", StringComparison.Ordinal)))
        {
            return "double";
        }

        if (string.Equals(candidate, "object", StringComparison.Ordinal))
        {
            return currentType;
        }

        return "object";
    }

    private static string DeterminePropertyType(string? expression)
    {
        var typeName = NormalizeTypeName(BlLegacyReturnTypeHelper.InferLiteral(expression));
        if (!string.IsNullOrEmpty(typeName))
        {
            return typeName;
        }

        if (IsStringMemberAccess(expression))
        {
            return "string";
        }

        return string.Empty;
    }

    private static bool IsStringMemberAccess(string? expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return false;
        }

        var trimmed = expression.Trim();
        if (!trimmed.StartsWith("Member<", StringComparison.Ordinal) &&
            !trimmed.StartsWith("Member(", StringComparison.Ordinal))
        {
            return false;
        }

        return ContainsSegment(trimmed, ".Line") ||
            ContainsSegment(trimmed, ".Word") ||
            ContainsSegment(trimmed, ".Text") ||
            ContainsSegment(trimmed, ".Char");
    }

    private static bool ContainsSegment(string expression, string segment)
    {
        return expression.IndexOf(segment, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static bool TryExtractAssignment(
        IReadOnlyList<BlSyntaxToken> tokens,
        out string propertyName,
        out IReadOnlyList<BlSyntaxToken> valueTokens)
    {
        propertyName = string.Empty;
        valueTokens = Array.Empty<BlSyntaxToken>();

        if (tokens is null || tokens.Count < 3)
        {
            return false;
        }

        var index = 0;
        string? candidate = null;

        if (IsMeToken(tokens[index]))
        {
            index++;
            if (index >= tokens.Count || tokens[index].Kind != BlSyntaxKind.PeriodToken)
            {
                return false;
            }

            index++;
            if (index >= tokens.Count || tokens[index].Kind != BlSyntaxKind.IdentifierToken)
            {
                return false;
            }

            candidate = tokens[index].ValueText;
            index++;
        }
        else if (tokens[index].Kind == BlSyntaxKind.IdentifierToken)
        {
            candidate = tokens[index].ValueText;
            index++;
        }
        else
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(candidate) || index >= tokens.Count)
        {
            return false;
        }

        var token = tokens[index];
        if (token.Kind != BlSyntaxKind.OperatorToken || !string.Equals(token.ValueText, "=", StringComparison.Ordinal))
        {
            return false;
        }

        index++;
        if (index >= tokens.Count)
        {
            return false;
        }

        var values = new List<BlSyntaxToken>(tokens.Count - index);
        for (; index < tokens.Count; index++)
        {
            values.Add(tokens[index]);
        }

        if (values.Count == 0)
        {
            return false;
        }

        propertyName = candidate!;
        valueTokens = values;
        return true;
    }

    private static bool IsMeToken(BlSyntaxToken token)
    {
        if (token.Kind is BlSyntaxKind.KeywordToken or BlSyntaxKind.IdentifierToken)
        {
            return string.Equals(token.ValueText, "me", StringComparison.OrdinalIgnoreCase);
        }

        return false;
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

        public PropertyState GetOrAddProperty(string name, int order)
        {
            if (!Properties.TryGetValue(name, out var info))
            {
                info = new PropertyState(name, order);
                Properties[name] = info;
            }
            else if (info.Order == int.MaxValue)
            {
                info.Order = order;
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
        public PropertyState(string name, int order)
        {
            Name = name;
            Order = order;
            Type = "object";
        }

        public string Name { get; }

        public int Order { get; set; }

        public string Type { get; set; }

        public string? Comment { get; set; }
    }
}
