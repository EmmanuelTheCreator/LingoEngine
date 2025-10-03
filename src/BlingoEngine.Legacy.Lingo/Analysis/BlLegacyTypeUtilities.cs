using System;
using System.Collections.Generic;

namespace BlingoEngine.Legacy.Lingo.Analysis;

internal static class BlLegacyTypeUtilities
{
    public static string NormalizeTypeName(string? typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            return string.Empty;
        }

        var trimmed = typeName.Trim();
        return string.Equals(trimmed, "object?", StringComparison.Ordinal)
            ? "object"
            : trimmed;
    }

    public static string MergeTypeNames(string currentType, string candidate)
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

    public static string DetermineExpressionType(string? expression)
    {
        var typeName = NormalizeTypeName(BlLegacyReturnTypeHelper.InferLiteral(expression));
        if (!string.IsNullOrEmpty(typeName))
        {
            return typeName;
        }

        if (TryDetermineMemberPropertyType(expression, out var memberPropertyType))
        {
            return memberPropertyType;
        }

        if (IsMemberConstructor(expression))
        {
            return "IBlingoMember";
        }

        if (TryDetermineArrayType(expression, out var arrayType))
        {
            return arrayType;
        }

        return string.Empty;
    }

    public static string NormalizePropertyTarget(string? target)
    {
        if (string.IsNullOrWhiteSpace(target))
        {
            return string.Empty;
        }

        var trimmed = target.Trim();
        return trimmed.StartsWith("this.", StringComparison.OrdinalIgnoreCase)
            ? trimmed[5..]
            : trimmed;
    }

    public static List<BlCodeSymbol> GetOrderedParameters(BlLingoHandlerSymbolTable? handler)
    {
        var ordered = new List<BlCodeSymbol>();
        if (handler is null)
        {
            return ordered;
        }

        foreach (var symbol in handler.Parameters.Values)
        {
            if (symbol is not null)
            {
                ordered.Add(symbol);
            }
        }

        ordered.Sort(static (left, right) => GetFirstSpan(left).CompareTo(GetFirstSpan(right)));
        return ordered;
    }

    private static bool IsMemberConstructor(string? expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return false;
        }

        var trimmed = expression.TrimStart();
        if (!trimmed.StartsWith("Member(", StringComparison.Ordinal) &&
            !trimmed.StartsWith("Member<", StringComparison.Ordinal))
        {
            return false;
        }

        var openIndex = trimmed.IndexOf('(');
        if (openIndex < 0)
        {
            return false;
        }

        var closeIndex = FindClosingParenthesisIndex(trimmed, openIndex);
        if (closeIndex < 0)
        {
            return false;
        }

        for (var index = closeIndex + 1; index < trimmed.Length; index++)
        {
            var ch = trimmed[index];
            if (char.IsWhiteSpace(ch))
            {
                continue;
            }

            return ch != '.';
        }

        return true;
    }

    private static int FindClosingParenthesisIndex(string expression, int openIndex)
    {
        var depth = 0;
        for (var index = openIndex; index < expression.Length; index++)
        {
            var ch = expression[index];
            if (ch == '(')
            {
                depth++;
            }
            else if (ch == ')')
            {
                depth--;
                if (depth == 0)
                {
                    return index;
                }
            }
        }

        return -1;
    }

    private static bool TryDetermineArrayType(string? expression, out string typeName)
    {
        typeName = string.Empty;
        if (string.IsNullOrWhiteSpace(expression))
        {
            return false;
        }

        var trimmed = expression.Trim();
        if (trimmed.Length == 0)
        {
            return false;
        }

        if (trimmed[0] == '[' && trimmed[^1] == ']')
        {
            typeName = "object[]";
            return true;
        }

        if (trimmed.StartsWith("new[]", StringComparison.Ordinal) ||
            trimmed.StartsWith("new []", StringComparison.Ordinal))
        {
            typeName = "object[]";
            return true;
        }

        return false;
    }

    private static bool TryDetermineMemberPropertyType(string? expression, out string typeName)
    {
        typeName = string.Empty;
        if (string.IsNullOrWhiteSpace(expression))
        {
            return false;
        }

        var trimmed = expression.Trim();
        if (!trimmed.StartsWith("Member", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var openIndex = trimmed.IndexOf('(');
        if (openIndex < 0)
        {
            return false;
        }

        var closeIndex = FindClosingParenthesisIndex(trimmed, openIndex);
        if (closeIndex < 0)
        {
            return false;
        }

        var propertyName = ExtractLastMemberPropertyName(trimmed, closeIndex + 1);
        if (propertyName.Length == 0)
        {
            return false;
        }

        if (!BlLegacyMemberPropertyFacts.TryGetValueType(propertyName, out var candidate))
        {
            return false;
        }

        typeName = NormalizeTypeName(candidate);
        return true;
    }

    private static string ExtractLastMemberPropertyName(string expression, int startIndex)
    {
        var last = string.Empty;
        for (var index = startIndex; index < expression.Length; index++)
        {
            if (expression[index] != '.')
            {
                continue;
            }

            index++;
            if (index >= expression.Length)
            {
                break;
            }

            var begin = index;
            var startChar = expression[begin];
            if (!char.IsLetter(startChar) && startChar != '_')
            {
                continue;
            }

            index++;
            while (index < expression.Length)
            {
                var ch = expression[index];
                if (char.IsLetterOrDigit(ch) || ch == '_')
                {
                    index++;
                    continue;
                }

                break;
            }

            last = expression[begin..index];
        }

        return last;
    }

    private static int GetFirstSpan(BlCodeSymbol symbol)
    {
        if (symbol?.Declarations is null || symbol.Declarations.Count == 0)
        {
            return int.MaxValue;
        }

        var span = symbol.Declarations[0].Span.Start;
        for (var index = 1; index < symbol.Declarations.Count; index++)
        {
            var candidate = symbol.Declarations[index].Span.Start;
            if (candidate < span)
            {
                span = candidate;
            }
        }

        return span;
    }
}

