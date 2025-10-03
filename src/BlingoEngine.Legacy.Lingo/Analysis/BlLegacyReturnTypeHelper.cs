using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BlingoEngine.Legacy.Lingo.Analysis;

internal static class BlLegacyReturnTypeHelper
{
    public static string? Merge(string? existing, string? candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return existing;
        }

        if (string.IsNullOrWhiteSpace(existing))
        {
            return candidate;
        }

        if (string.Equals(existing, candidate, StringComparison.Ordinal))
        {
            return existing;
        }

        if (string.Equals(existing, "object?", StringComparison.Ordinal) ||
            string.Equals(candidate, "object?", StringComparison.Ordinal))
        {
            return "object?";
        }

        var normalizedExisting = existing.Trim();
        var normalizedCandidate = candidate.Trim();
        if ((string.Equals(normalizedExisting, "int", StringComparison.Ordinal) &&
             string.Equals(normalizedCandidate, "double", StringComparison.Ordinal)) ||
            (string.Equals(normalizedExisting, "double", StringComparison.Ordinal) &&
             string.Equals(normalizedCandidate, "int", StringComparison.Ordinal)))
        {
            return "double";
        }

        return "object?";
    }

    public static string? InferBlockResult(BlLingoHandlerCodeBlock block)
    {
        if (block is null)
        {
            return null;
        }

        return block.Kind switch
        {
            BlLingoHandlerCodeBlockKind.Expression when block.Data is BlLingoExpressionBlockData expression
                => InferReturnStatement(expression.Expression),
            BlLingoHandlerCodeBlockKind.Put when block.Data is BlLingoPutBlockData put
                => InferResultAssignment(put),
            _ => null,
        };
    }

    public static string? InferReturnStatement(string? expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return null;
        }

        var trimmed = expression.Trim();
        if (!trimmed.StartsWith("return", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var value = trimmed.Length > 6 ? trimmed[6..].Trim() : string.Empty;
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        return InferLiteral(value);
    }

    public static string? InferResultAssignment(BlLingoPutBlockData data)
    {
        if (data is null || data.Kind != BlLingoPutAssignmentKind.Direct)
        {
            return null;
        }

        if (!IsResultTarget(data.TargetExpression))
        {
            return null;
        }

        return InferLiteral(data.ValueExpression);
    }

    public static string? InferLiteral(string? expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return null;
        }

        var trimmed = TrimOuterParentheses(expression.Trim());
        if (trimmed.EndsWith(";", StringComparison.Ordinal))
        {
            trimmed = trimmed.TrimEnd(';').Trim();
        }

        if (trimmed.Length == 0)
        {
            return null;
        }

        if (trimmed.StartsWith("Convert.ToInt32(", StringComparison.Ordinal))
        {
            return "int";
        }

        if (trimmed.Equals("true", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("false", StringComparison.OrdinalIgnoreCase))
        {
            return "bool";
        }

        if (trimmed.Equals("null", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("void", StringComparison.OrdinalIgnoreCase))
        {
            return "object?";
        }

        if ((trimmed.StartsWith("\"", StringComparison.Ordinal) && trimmed.EndsWith("\"", StringComparison.Ordinal)) ||
            (trimmed.StartsWith("'", StringComparison.Ordinal) && trimmed.EndsWith("'", StringComparison.Ordinal)))
        {
            return "string";
        }

        if (trimmed.StartsWith("#", StringComparison.Ordinal))
        {
            return "object?";
        }

        if (int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
        {
            return "int";
        }

        if (double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
        {
            return "double";
        }

        if (LooksLikeNumericArithmeticExpression(trimmed))
        {
            return ContainsFloatingPointLiteral(trimmed) ? "double" : "int";
        }

        return null;
    }

    public static bool IsResultTarget(string? target)
    {
        return !string.IsNullOrEmpty(target) &&
            target.Equals("result", StringComparison.OrdinalIgnoreCase);
    }

    private static bool LooksLikeNumericArithmeticExpression(string expression)
    {
        if (!Regex.IsMatch(expression, "[+\\-*/]"))
        {
            return false;
        }

        return Regex.IsMatch(expression, @"\d");
    }

    private static bool ContainsFloatingPointLiteral(string expression)
    {
        return Regex.IsMatch(expression, @"\d\.\d");
    }

    public static bool IsReturnWithValue(string? expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return false;
        }

        var trimmed = expression.Trim();
        if (!trimmed.StartsWith("return", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var value = trimmed.Length > 6 ? trimmed[6..].Trim() : string.Empty;
        return value.Length > 0 && !string.Equals(value, ";", StringComparison.Ordinal);
    }

    private static string TrimOuterParentheses(string expression)
    {
        var result = expression;
        while (result.Length >= 2 && result[0] == '(' && result[^1] == ')')
        {
            if (!AreParenthesesBalanced(result))
            {
                break;
            }

            result = result[1..^1].Trim();
        }

        return result;
    }

    private static bool AreParenthesesBalanced(string expression)
    {
        var depth = 0;
        for (var index = 0; index < expression.Length; index++)
        {
            var character = expression[index];
            if (character == '(')
            {
                depth++;
            }
            else if (character == ')')
            {
                depth--;
                if (depth < 0)
                {
                    return false;
                }
            }
        }

        return depth == 0;
    }

}
