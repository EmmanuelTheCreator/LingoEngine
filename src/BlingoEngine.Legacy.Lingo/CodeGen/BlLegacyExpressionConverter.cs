using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.CodeGen;

public sealed class BlLegacyExpressionConverter
{
    private readonly IReadOnlyList<BlSyntaxToken> _tokens;
    private readonly List<string> _parts = new();
    private int _index;
    private bool _afterDot;

    public BlLegacyExpressionConverter(IReadOnlyList<BlSyntaxToken> tokens)
    {
        _tokens = tokens ?? Array.Empty<BlSyntaxToken>();
    }

    public static string Convert(IReadOnlyList<BlSyntaxToken> tokens)
    {
        if (tokens is null || tokens.Count == 0)
        {
            return string.Empty;
        }

        return new BlLegacyExpressionConverter(tokens).Build();
    }

    public string Build()
    {
        while (_index < _tokens.Count)
        {
            if (TryHandleVoidPredicate() ||
                TryHandleScriptInstantiation() ||
                TryHandleMemberTextAccess() ||
                TryHandleListLiteral() ||
                TryHandleTheKeywords())
            {
                continue;
            }

            var token = _tokens[_index++];
            switch (token.Kind)
            {
                case BlSyntaxKind.IdentifierToken:
                case BlSyntaxKind.KeywordToken:
                    AppendIdentifier(token.ValueText);
                    break;
                case BlSyntaxKind.NumberToken:
                case BlSyntaxKind.StringLiteralToken:
                    AppendRaw(token.Text);
                    break;
                case BlSyntaxKind.OperatorToken:
                    AppendOperator(token.ValueText);
                    break;
                case BlSyntaxKind.PeriodToken:
                    AppendRaw(".");
                    _afterDot = true;
                    break;
                case BlSyntaxKind.LeftParenthesisToken:
                    AppendRaw("(");
                    break;
                case BlSyntaxKind.RightParenthesisToken:
                    AppendRaw(")");
                    _afterDot = false;
                    break;
                case BlSyntaxKind.LeftBracketToken:
                    AppendRaw("[");
                    break;
                case BlSyntaxKind.RightBracketToken:
                    AppendRaw("]");
                    _afterDot = false;
                    break;
                case BlSyntaxKind.CommaToken:
                    AppendRaw(",");
                    break;
                default:
                    AppendRaw(token.Text);
                    break;
            }
        }

        return NormalizeSpacing(_parts);
    }

    private bool TryHandleVoidPredicate()
    {
        if (_index >= _tokens.Count ||
            !string.Equals(_tokens[_index].ValueText, "voidp", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (_index + 1 >= _tokens.Count || _tokens[_index + 1].Kind != BlSyntaxKind.LeftParenthesisToken)
        {
            return false;
        }

        var closeIndex = BlLegacyHandlerTokenUtilities.FindMatchingToken(_tokens, _index + 1, BlSyntaxKind.LeftParenthesisToken, BlSyntaxKind.RightParenthesisToken);
        if (closeIndex < 0)
        {
            return false;
        }

        var innerTokens = BlLegacyHandlerTokenUtilities.SliceTokens(_tokens, _index + 2, closeIndex - (_index + 2));
        var expression = Convert(innerTokens);
        AppendRaw(expression);
        AppendRaw("==");
        AppendRaw("null");
        _index = closeIndex + 1;
        _afterDot = false;
        return true;
    }

    private bool TryHandleScriptInstantiation()
    {
        if (_index >= _tokens.Count ||
            !string.Equals(_tokens[_index].ValueText, "script", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (_index + 1 >= _tokens.Count || _tokens[_index + 1].Kind != BlSyntaxKind.LeftParenthesisToken)
        {
            return false;
        }

        var nameClose = BlLegacyHandlerTokenUtilities.FindMatchingToken(_tokens, _index + 1, BlSyntaxKind.LeftParenthesisToken, BlSyntaxKind.RightParenthesisToken);
        if (nameClose < 0)
        {
            return false;
        }

        var afterName = nameClose + 1;
        if (afterName + 2 >= _tokens.Count ||
            _tokens[afterName].Kind != BlSyntaxKind.PeriodToken ||
            !string.Equals(_tokens[afterName + 1].ValueText, "new", StringComparison.OrdinalIgnoreCase) ||
            _tokens[afterName + 2].Kind != BlSyntaxKind.LeftParenthesisToken)
        {
            return false;
        }

        var argsClose = BlLegacyHandlerTokenUtilities.FindMatchingToken(_tokens, afterName + 2, BlSyntaxKind.LeftParenthesisToken, BlSyntaxKind.RightParenthesisToken);
        if (argsClose < 0)
        {
            return false;
        }

        var nameTokens = BlLegacyHandlerTokenUtilities.SliceTokens(_tokens, _index + 2, nameClose - (_index + 2));
        var argsTokens = BlLegacyHandlerTokenUtilities.SliceTokens(_tokens, afterName + 3, argsClose - (afterName + 3));
        var className = ResolveClassName(nameTokens);
        var arguments = ConvertArguments(argsTokens);

        AppendRaw("new");
        AppendRaw(className);
        AppendRaw("(");
        if (!string.IsNullOrEmpty(arguments))
        {
            AppendRaw(arguments);
        }

        AppendRaw(")");
        _index = argsClose + 1;
        _afterDot = false;
        return true;
    }

    private bool TryHandleMemberTextAccess()
    {
        if (_index >= _tokens.Count ||
            !string.Equals(_tokens[_index].ValueText, "member", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (_index + 1 >= _tokens.Count || _tokens[_index + 1].Kind != BlSyntaxKind.LeftParenthesisToken)
        {
            return false;
        }

        var argsClose = BlLegacyHandlerTokenUtilities.FindMatchingToken(_tokens, _index + 1, BlSyntaxKind.LeftParenthesisToken, BlSyntaxKind.RightParenthesisToken);
        if (argsClose < 0)
        {
            return false;
        }

        if (argsClose + 2 >= _tokens.Count ||
            _tokens[argsClose + 1].Kind != BlSyntaxKind.PeriodToken ||
            !string.Equals(_tokens[argsClose + 2].ValueText, "text", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var argsTokens = BlLegacyHandlerTokenUtilities.SliceTokens(_tokens, _index + 2, argsClose - (_index + 2));
        var arguments = ConvertArguments(argsTokens);

        AppendRaw("Member<BlingoMemberText>");
        AppendRaw("(");
        if (!string.IsNullOrEmpty(arguments))
        {
            AppendRaw(arguments);
        }

        AppendRaw(")");
        AppendRaw(".");
        AppendRaw("Text");

        _index = argsClose + 3;
        _afterDot = false;
        return true;
    }

    private bool TryHandleListLiteral()
    {
        if (_index >= _tokens.Count || _tokens[_index].Kind != BlSyntaxKind.LeftBracketToken)
        {
            return false;
        }

        var closeIndex = BlLegacyHandlerTokenUtilities.FindMatchingToken(_tokens, _index, BlSyntaxKind.LeftBracketToken, BlSyntaxKind.RightBracketToken);
        if (closeIndex < 0)
        {
            return false;
        }

        var innerTokens = BlLegacyHandlerTokenUtilities.SliceTokens(_tokens, _index + 1, closeIndex - (_index + 1));
        var segments = BlLegacyHandlerTokenUtilities.SplitByComma(innerTokens);
        AppendRaw("new[]");
        AppendRaw("{");
        for (var i = 0; i < segments.Count; i++)
        {
            var value = Convert(segments[i]);
            if (value.Length > 0)
            {
                AppendRaw(value);
            }

            if (i < segments.Count - 1)
            {
                AppendRaw(",");
            }
        }

        AppendRaw("}");
        _index = closeIndex + 1;
        _afterDot = false;
        return true;
    }

    private bool TryHandleTheKeywords()
    {
        if (_index >= _tokens.Count ||
            !string.Equals(_tokens[_index].ValueText, "the", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (_index + 1 >= _tokens.Count)
        {
            return false;
        }

        var next = _tokens[_index + 1];
        if (string.Equals(next.ValueText, "mouseH", StringComparison.OrdinalIgnoreCase))
        {
            AppendRaw("_Mouse.MouseH");
            _index += 2;
            _afterDot = false;
            return true;
        }

        if (string.Equals(next.ValueText, "actorList", StringComparison.OrdinalIgnoreCase))
        {
            AppendRaw("_Movie.ActorList");
            _index += 2;
            _afterDot = false;
            return true;
        }

        return false;
    }

    private void AppendIdentifier(string text)
    {
        if (string.Equals(text, "me", StringComparison.OrdinalIgnoreCase))
        {
            AppendRaw("this");
            _afterDot = false;
            return;
        }

        if (string.Equals(text, "void", StringComparison.OrdinalIgnoreCase))
        {
            AppendRaw("null");
            _afterDot = false;
            return;
        }

        if (string.Equals(text, "sprite", StringComparison.OrdinalIgnoreCase))
        {
            AppendRaw("Sprite");
            _afterDot = false;
            return;
        }

        if (_afterDot)
        {
            AppendRaw(BlLegacyHandlerTokenUtilities.ToPascalCase(text));
            _afterDot = false;
            return;
        }

        AppendRaw(text);
        _afterDot = false;
    }

    private void AppendOperator(string op)
    {
        if (string.Equals(op, "<>", StringComparison.Ordinal))
        {
            AppendRaw("!=");
            return;
        }

        if (string.Equals(op, "&", StringComparison.Ordinal))
        {
            AppendRaw("+");
            return;
        }

        AppendRaw(op);
    }

    private void AppendRaw(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            _parts.Add(text);
        }
    }

    private static string NormalizeSpacing(List<string> parts)
    {
        if (parts.Count == 0)
        {
            return string.Empty;
        }

        var raw = string.Join(' ', parts);
        var builder = new StringBuilder(raw.Length * 2);
        var insideArrayLiteral = false;
        var arrayDepth = 0;

        for (var index = 0; index < raw.Length; index++)
        {
            var ch = raw[index];

            if (ch == '{')
            {
                var startsArray = !insideArrayLiteral && EndsWithNewArray(builder);
                builder.Append('{');

                if (startsArray)
                {
                    insideArrayLiteral = true;
                    arrayDepth = 1;
                }
                else if (insideArrayLiteral)
                {
                    arrayDepth++;
                }

                continue;
            }

            if (ch == '}')
            {
                if (insideArrayLiteral && arrayDepth > 0)
                {
                    arrayDepth--;
                    if (arrayDepth == 0)
                    {
                        insideArrayLiteral = false;
                    }
                }

                builder.Append('}');
                continue;
            }

            if (ch == ' ')
            {
                var next = index + 1 < raw.Length ? raw[index + 1] : '\0';
                var previous = builder.Length > 0 ? builder[^1] : '\0';

                if (insideArrayLiteral && arrayDepth <= 1 && previous == ',')
                {
                    continue;
                }

                if (previous == ' ' || previous == '.' || next == '\0' || next is '.' or ',' or ')' or ']' ||
                    previous is '(' or '[' or '<')
                {
                    continue;
                }
            }

            if (builder.Length > 0 && builder[^1] == ' ' && (ch is '(' or '['))
            {
                builder.Length--;
            }

            if (ch == ',')
            {
                builder.Append(',');
                if (!insideArrayLiteral || arrayDepth > 1)
                {
                    if (index + 1 < raw.Length && raw[index + 1] != ' ')
                    {
                        builder.Append(' ');
                    }
                }

                continue;
            }

            if (builder.Length > 0 && builder[^1] == ' ' && (ch is '.' or ')' or ']' or ';' or ':'))
            {
                builder.Length--;
            }

            builder.Append(ch);
        }

        return builder.ToString().Trim();
    }

    private static bool EndsWithNewArray(StringBuilder builder)
    {
        const string pattern = "new[]";
        var index = builder.Length - 1;

        while (index >= 0 && char.IsWhiteSpace(builder[index]))
        {
            index--;
        }

        if (index < pattern.Length - 1)
        {
            return false;
        }

        for (var patternIndex = pattern.Length - 1; patternIndex >= 0; patternIndex--, index--)
        {
            if (index < 0 || builder[index] != pattern[patternIndex])
            {
                return false;
            }
        }

        return true;
    }

    private static string ResolveClassName(IReadOnlyList<BlSyntaxToken> tokens)
    {
        if (tokens.Count == 0)
        {
            return "Script";
        }

        var token = tokens[0];
        if (token.Kind == BlSyntaxKind.StringLiteralToken)
        {
            return BlCSharpName.SanitizeIdentifier(token.ValueText);
        }

        return BlCSharpName.SanitizeIdentifier(token.ValueText);
    }

    private static string ConvertArguments(IReadOnlyList<BlSyntaxToken> tokens)
    {
        if (tokens.Count == 0)
        {
            return string.Empty;
        }

        var segments = BlLegacyHandlerTokenUtilities.SplitByComma(tokens);
        var builder = new StringBuilder();
        for (var index = 0; index < segments.Count; index++)
        {
            var expression = Convert(segments[index]);
            if (expression.Length > 0)
            {
                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(expression);
            }
        }

        return builder.ToString();
    }
}
