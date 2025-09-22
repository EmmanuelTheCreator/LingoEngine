using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.Syntax.Expressions;

/// <summary>
/// Provides helper routines for mapping tokens to operator kinds and precedence values.
/// </summary>
public static class BlLingoOperatorFacts
{
    private sealed record BinaryOperatorInfo(BlBinaryOperatorKind Kind, int Precedence, bool IsRightAssociative);

    private static readonly Dictionary<string, BlUnaryOperatorKind> s_unaryOperators = new(StringComparer.OrdinalIgnoreCase)
    {
        ["not"] = BlUnaryOperatorKind.LogicalNot,
    };

    private static readonly Dictionary<string, BinaryOperatorInfo> s_keywordBinaryOperators = new(StringComparer.OrdinalIgnoreCase)
    {
        ["mod"] = new BinaryOperatorInfo(BlBinaryOperatorKind.Modulus, 6, false),
        ["and"] = new BinaryOperatorInfo(BlBinaryOperatorKind.LogicalAnd, 2, false),
        ["or"] = new BinaryOperatorInfo(BlBinaryOperatorKind.LogicalOr, 1, false),
    };

    private static readonly Dictionary<string, BinaryOperatorInfo> s_symbolBinaryOperators = new(StringComparer.OrdinalIgnoreCase)
    {
        ["^"] = new BinaryOperatorInfo(BlBinaryOperatorKind.Power, 7, true),
        ["*"] = new BinaryOperatorInfo(BlBinaryOperatorKind.Multiply, 6, false),
        ["/"] = new BinaryOperatorInfo(BlBinaryOperatorKind.Divide, 6, false),
        ["+"] = new BinaryOperatorInfo(BlBinaryOperatorKind.Add, 5, false),
        ["-"] = new BinaryOperatorInfo(BlBinaryOperatorKind.Subtract, 5, false),
        ["&"] = new BinaryOperatorInfo(BlBinaryOperatorKind.Concatenate, 4, false),
        ["&&"] = new BinaryOperatorInfo(BlBinaryOperatorKind.LogicalAnd, 2, false),
        ["="] = new BinaryOperatorInfo(BlBinaryOperatorKind.Equal, 3, false),
        ["<>"] = new BinaryOperatorInfo(BlBinaryOperatorKind.NotEqual, 3, false),
        ["<"] = new BinaryOperatorInfo(BlBinaryOperatorKind.LessThan, 3, false),
        ["<="] = new BinaryOperatorInfo(BlBinaryOperatorKind.LessThanOrEqual, 3, false),
        [">"] = new BinaryOperatorInfo(BlBinaryOperatorKind.GreaterThan, 3, false),
        [">="] = new BinaryOperatorInfo(BlBinaryOperatorKind.GreaterThanOrEqual, 3, false),
    };

    /// <summary>
    /// Attempts to map the supplied token to a unary operator kind.
    /// </summary>
    public static bool TryGetUnaryOperatorKind(BlSyntaxToken token, out BlUnaryOperatorKind operatorKind)
    {
        ArgumentNullException.ThrowIfNull(token);

        if (token.Kind == BlSyntaxKind.OperatorToken)
        {
            return TryGetUnaryOperatorKind(token.ValueText, out operatorKind);
        }

        if (token.Kind == BlSyntaxKind.KeywordToken)
        {
            return s_unaryOperators.TryGetValue(token.ValueText, out operatorKind);
        }

        operatorKind = BlUnaryOperatorKind.Unknown;
        return false;
    }

    /// <summary>
    /// Attempts to map a textual representation of an operator to a unary operator kind.
    /// </summary>
    public static bool TryGetUnaryOperatorKind(string text, out BlUnaryOperatorKind operatorKind)
    {
        ArgumentNullException.ThrowIfNull(text);

        switch (text)
        {
            case "+":
                operatorKind = BlUnaryOperatorKind.Positive;
                return true;
            case "-":
                operatorKind = BlUnaryOperatorKind.Negative;
                return true;
            default:
                return s_unaryOperators.TryGetValue(text, out operatorKind);
        }
    }

    /// <summary>
    /// Attempts to resolve a binary operator classification for the supplied token.
    /// </summary>
    public static bool TryGetBinaryOperatorInfo(
        BlSyntaxToken token,
        [MaybeNullWhen(false)] out BlBinaryOperatorKind operatorKind,
        out int precedence,
        out bool isRightAssociative)
    {
        ArgumentNullException.ThrowIfNull(token);

        if (token.Kind == BlSyntaxKind.OperatorToken)
        {
            return TryGetBinaryOperatorInfo(token.ValueText, out operatorKind, out precedence, out isRightAssociative);
        }

        if (token.Kind == BlSyntaxKind.KeywordToken && s_keywordBinaryOperators.TryGetValue(token.ValueText, out var info))
        {
            operatorKind = info.Kind;
            precedence = info.Precedence;
            isRightAssociative = info.IsRightAssociative;
            return true;
        }

        operatorKind = BlBinaryOperatorKind.Unknown;
        precedence = 0;
        isRightAssociative = false;
        return false;
    }

    /// <summary>
    /// Attempts to resolve a binary operator classification for the supplied operator text.
    /// </summary>
    public static bool TryGetBinaryOperatorInfo(
        string text,
        [MaybeNullWhen(false)] out BlBinaryOperatorKind operatorKind,
        out int precedence,
        out bool isRightAssociative)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (s_symbolBinaryOperators.TryGetValue(text, out var symbolInfo))
        {
            operatorKind = symbolInfo.Kind;
            precedence = symbolInfo.Precedence;
            isRightAssociative = symbolInfo.IsRightAssociative;
            return true;
        }

        if (s_keywordBinaryOperators.TryGetValue(text, out var keywordInfo))
        {
            operatorKind = keywordInfo.Kind;
            precedence = keywordInfo.Precedence;
            isRightAssociative = keywordInfo.IsRightAssociative;
            return true;
        }

        operatorKind = BlBinaryOperatorKind.Unknown;
        precedence = 0;
        isRightAssociative = false;
        return false;
    }
}
