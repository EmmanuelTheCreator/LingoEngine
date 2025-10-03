using System;
using System.Collections.Generic;
using System.Globalization;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.Analysis;

internal static class BlLegacyExpressionTokenAnalyzer
{
    public static bool TryInferLiteralType(IReadOnlyList<BlSyntaxToken> tokens, out string typeName)
    {
        typeName = string.Empty;
        if (tokens is null || tokens.Count == 0)
        {
            return false;
        }

        var (start, end) = GetTrimmedBounds(tokens, 0, tokens.Count - 1);
        if (end < start)
        {
            return false;
        }

        var index = start;
        var token = tokens[index];
        var sign = 0;

        if (token.Kind == BlSyntaxKind.OperatorToken &&
            (token.ValueText == "+" || token.ValueText == "-"))
        {
            sign = token.ValueText == "-" ? -1 : 1;
            index++;
            if (index > end)
            {
                return false;
            }

            token = tokens[index];
        }

        if (index != end)
        {
            return false;
        }

        switch (token.Kind)
        {
            case BlSyntaxKind.NumberToken:
                {
                    var text = token.ValueText;
                    if (sign < 0)
                    {
                        text = "-" + text;
                    }
                    else if (sign > 0)
                    {
                        text = "+" + text;
                    }

                    if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                    {
                        typeName = "int";
                        return true;
                    }

                    if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                    {
                        typeName = "double";
                        return true;
                    }

                    return false;
                }

            case BlSyntaxKind.StringLiteralToken:
                typeName = "string";
                return true;

            case BlSyntaxKind.KeywordToken:
                if (string.Equals(token.ValueText, "true", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(token.ValueText, "false", StringComparison.OrdinalIgnoreCase))
                {
                    typeName = "bool";
                    return true;
                }

                if (string.Equals(token.ValueText, "null", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(token.ValueText, "void", StringComparison.OrdinalIgnoreCase))
                {
                    typeName = "object?";
                    return true;
                }

                break;

            case BlSyntaxKind.HashToken:
            case BlSyntaxKind.SymbolToken:
                typeName = "object?";
                return true;
        }

        return false;
    }

    public static bool TryGetIdentifier(IReadOnlyList<BlSyntaxToken> tokens, out string identifier)
    {
        identifier = string.Empty;
        if (tokens is null || tokens.Count == 0)
        {
            return false;
        }

        var (start, end) = GetTrimmedBounds(tokens, 0, tokens.Count - 1);
        if (end < start)
        {
            return false;
        }

        var length = end - start + 1;

        if (length == 1)
        {
            var token = tokens[start];
            if (IsIdentifierLike(token))
            {
                identifier = token.ValueText;
                return identifier.Length > 0;
            }

            return false;
        }

        if (length == 2 &&
            tokens[start].Kind == BlSyntaxKind.KeywordToken &&
            string.Equals(tokens[start].ValueText, "the", StringComparison.OrdinalIgnoreCase) &&
            IsIdentifierLike(tokens[start + 1]))
        {
            identifier = tokens[start + 1].ValueText;
            return identifier.Length > 0;
        }

        if (length == 3 &&
            tokens[start + 1].Kind == BlSyntaxKind.PeriodToken &&
            IsIdentifierLike(tokens[start + 2]) &&
            IsSelfReference(tokens[start]))
        {
            identifier = tokens[start + 2].ValueText;
            return identifier.Length > 0;
        }

        return false;
    }

    private static bool IsIdentifierLike(BlSyntaxToken token)
    {
        if (token.ValueText.Length == 0)
        {
            return false;
        }

        return token.Kind is BlSyntaxKind.IdentifierToken or BlSyntaxKind.SymbolToken or BlSyntaxKind.KeywordToken;
    }

    private static bool IsSelfReference(BlSyntaxToken token)
    {
        return token.Kind is BlSyntaxKind.IdentifierToken or BlSyntaxKind.KeywordToken &&
            (string.Equals(token.ValueText, "me", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(token.ValueText, "this", StringComparison.OrdinalIgnoreCase));
    }

    private static (int start, int end) GetTrimmedBounds(
        IReadOnlyList<BlSyntaxToken> tokens,
        int start,
        int end)
    {
        if (tokens is null || tokens.Count == 0)
        {
            return (0, -1);
        }

        while (start <= end && IsIgnorable(tokens[start]))
        {
            start++;
        }

        while (end >= start && IsIgnorable(tokens[end]))
        {
            end--;
        }

        var changed = true;
        while (changed && start < end)
        {
            changed = false;
            if (tokens[start].Kind == BlSyntaxKind.LeftParenthesisToken &&
                tokens[end].Kind == BlSyntaxKind.RightParenthesisToken &&
                AreParenthesesBalanced(tokens, start, end))
            {
                start++;
                end--;

                while (start <= end && IsIgnorable(tokens[start]))
                {
                    start++;
                }

                while (end >= start && IsIgnorable(tokens[end]))
                {
                    end--;
                }

                changed = true;
            }
        }

        return (start, end);
    }

    private static bool AreParenthesesBalanced(IReadOnlyList<BlSyntaxToken> tokens, int start, int end)
    {
        var depth = 0;
        for (var index = start; index <= end; index++)
        {
            var kind = tokens[index].Kind;
            if (kind == BlSyntaxKind.LeftParenthesisToken)
            {
                depth++;
            }
            else if (kind == BlSyntaxKind.RightParenthesisToken)
            {
                depth--;
                if (depth == 0 && index < end)
                {
                    return false;
                }

                if (depth < 0)
                {
                    return false;
                }
            }
        }

        return depth == 0;
    }

    private static bool IsIgnorable(BlSyntaxToken token)
    {
        return token.Kind is BlSyntaxKind.SemicolonToken or BlSyntaxKind.EndOfFileToken;
    }
}
