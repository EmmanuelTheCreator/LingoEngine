using System;
using System.Collections.Generic;
using System.Globalization;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.CodeGen;

public static class BlLegacyHandlerTokenUtilities
{
    public static IReadOnlyList<BlSyntaxToken> SliceTokens(IReadOnlyList<BlSyntaxToken> tokens, int start, int length)
    {
        if (tokens is null || length <= 0)
        {
            return Array.Empty<BlSyntaxToken>();
        }

        var list = new List<BlSyntaxToken>(Math.Max(length, 0));
        for (var index = 0; index < length && start + index < tokens.Count; index++)
        {
            list.Add(tokens[start + index]);
        }

        return list;
    }

    public static int FindKeyword(IReadOnlyList<BlSyntaxToken> tokens, string keyword)
    {
        for (var index = 0; index < tokens.Count; index++)
        {
            if (IsIdentifier(tokens[index], keyword))
            {
                return index;
            }
        }

        return -1;
    }

    public static int FindOperator(IReadOnlyList<BlSyntaxToken> tokens, string op)
    {
        for (var index = 0; index < tokens.Count; index++)
        {
            if (tokens[index].Kind == BlSyntaxKind.OperatorToken &&
                string.Equals(tokens[index].ValueText, op, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return -1;
    }

    public static int FindToken(IReadOnlyList<BlSyntaxToken> tokens, BlSyntaxKind kind)
    {
        for (var index = 0; index < tokens.Count; index++)
        {
            if (tokens[index].Kind == kind)
            {
                return index;
            }
        }

        return -1;
    }

    public static int FindMatchingToken(
        IReadOnlyList<BlSyntaxToken> tokens,
        int startIndex,
        BlSyntaxKind openKind,
        BlSyntaxKind closeKind)
    {
        var depth = 0;
        for (var index = startIndex; index < tokens.Count; index++)
        {
            var token = tokens[index];
            if (token.Kind == openKind)
            {
                depth++;
            }
            else if (token.Kind == closeKind)
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

    public static bool IsKeyword(BlSyntaxToken token, string keyword)
    {
        return token.Kind == BlSyntaxKind.KeywordToken &&
            string.Equals(token.ValueText, keyword, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsIdentifier(BlSyntaxToken token, string identifier)
    {
        return token.Kind is BlSyntaxKind.IdentifierToken or BlSyntaxKind.KeywordToken &&
            string.Equals(token.ValueText, identifier, StringComparison.OrdinalIgnoreCase);
    }

    public static bool TryGetSpritePropertyTarget(
        IReadOnlyList<BlSyntaxToken> tokens,
        out string spriteIndex,
        out string propertyName)
    {
        spriteIndex = string.Empty;
        propertyName = string.Empty;

        if (tokens.Count < 5 || !IsIdentifier(tokens[0], "sprite"))
        {
            return false;
        }

        if (tokens[1].Kind != BlSyntaxKind.LeftParenthesisToken)
        {
            return false;
        }

        var closeIndex = FindMatchingToken(tokens, 1, BlSyntaxKind.LeftParenthesisToken, BlSyntaxKind.RightParenthesisToken);
        if (closeIndex < 0 || closeIndex + 2 >= tokens.Count)
        {
            return false;
        }

        if (tokens[closeIndex + 1].Kind != BlSyntaxKind.PeriodToken)
        {
            return false;
        }

        var propertyToken = tokens[closeIndex + 2];
        if (propertyToken.Kind != BlSyntaxKind.IdentifierToken)
        {
            return false;
        }

        var indexTokens = SliceTokens(tokens, 2, closeIndex - 2);
        spriteIndex = BlLegacyExpressionConverter.Convert(indexTokens);
        propertyName = ToPascalCase(propertyToken.ValueText);
        return true;
    }

    public static bool TryGetListTarget(
        IReadOnlyList<BlSyntaxToken> tokens,
        out string listExpression,
        out string indexExpression)
    {
        listExpression = string.Empty;
        indexExpression = string.Empty;

        var bracketIndex = FindToken(tokens, BlSyntaxKind.LeftBracketToken);
        if (bracketIndex <= 0)
        {
            return false;
        }

        var closeIndex = FindMatchingToken(tokens, bracketIndex, BlSyntaxKind.LeftBracketToken, BlSyntaxKind.RightBracketToken);
        if (closeIndex < 0)
        {
            return false;
        }

        var listTokens = SliceTokens(tokens, 0, bracketIndex);
        var indexTokens = SliceTokens(tokens, bracketIndex + 1, closeIndex - bracketIndex - 1);
        listExpression = BlLegacyExpressionConverter.Convert(listTokens);
        indexExpression = BlLegacyExpressionConverter.Convert(indexTokens);
        return listExpression.Length > 0 && indexExpression.Length > 0;
    }

    public static List<IReadOnlyList<BlSyntaxToken>> SplitByComma(IReadOnlyList<BlSyntaxToken> tokens)
    {
        var result = new List<IReadOnlyList<BlSyntaxToken>>();
        var start = 0;
        var depthParenthesis = 0;
        var depthBracket = 0;
        var depthBrace = 0;

        for (var index = 0; index < tokens.Count; index++)
        {
            var token = tokens[index];
            switch (token.Kind)
            {
                case BlSyntaxKind.LeftParenthesisToken:
                    depthParenthesis++;
                    break;
                case BlSyntaxKind.RightParenthesisToken:
                    depthParenthesis--;
                    break;
                case BlSyntaxKind.LeftBracketToken:
                    depthBracket++;
                    break;
                case BlSyntaxKind.RightBracketToken:
                    depthBracket--;
                    break;
                case BlSyntaxKind.LeftBraceToken:
                    depthBrace++;
                    break;
                case BlSyntaxKind.RightBraceToken:
                    depthBrace--;
                    break;
                case BlSyntaxKind.CommaToken when depthParenthesis == 0 && depthBracket == 0 && depthBrace == 0:
                    result.Add(SliceTokens(tokens, start, index - start));
                    start = index + 1;
                    break;
            }
        }

        if (start < tokens.Count)
        {
            result.Add(SliceTokens(tokens, start, tokens.Count - start));
        }

        return result;
    }

    public static string ToPascalCase(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        if (name.Length == 1)
        {
            return name.ToUpper(CultureInfo.InvariantCulture);
        }

        return char.ToUpper(name[0], CultureInfo.InvariantCulture) + name[1..];
    }
}
