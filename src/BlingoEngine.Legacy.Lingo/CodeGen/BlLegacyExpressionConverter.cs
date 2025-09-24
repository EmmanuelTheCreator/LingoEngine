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

    private readonly struct MemberPropertyMapping
    {
        public MemberPropertyMapping(string genericType, string propertyName)
        {
            GenericType = genericType;
            PropertyName = propertyName;
        }

        public string GenericType { get; }

        public string PropertyName { get; }
    }

    private static readonly Dictionary<string, MemberPropertyMapping> s_memberPropertyMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["text"] = new MemberPropertyMapping("IBlingoMemberTextBase", "Text"),
        ["line"] = new MemberPropertyMapping("IBlingoMemberTextBase", "Line"),
        ["word"] = new MemberPropertyMapping("IBlingoMemberTextBase", "Word"),
        ["char"] = new MemberPropertyMapping("IBlingoMemberTextBase", "Char"),
        ["editable"] = new MemberPropertyMapping("IBlingoMemberTextBase", "Editable"),
        ["wordwrap"] = new MemberPropertyMapping("IBlingoMemberTextBase", "WordWrap"),
        ["scrolltop"] = new MemberPropertyMapping("IBlingoMemberTextBase", "ScrollTop"),
        ["textfont"] = new MemberPropertyMapping("IBlingoMemberTextBase", "Font"),
        ["font"] = new MemberPropertyMapping("IBlingoMemberTextBase", "Font"),
        ["textsize"] = new MemberPropertyMapping("IBlingoMemberTextBase", "FontSize"),
        ["fontsize"] = new MemberPropertyMapping("IBlingoMemberTextBase", "FontSize"),
        ["textstyle"] = new MemberPropertyMapping("IBlingoMemberTextBase", "FontStyle"),
        ["fontstyle"] = new MemberPropertyMapping("IBlingoMemberTextBase", "FontStyle"),
        ["textcolor"] = new MemberPropertyMapping("IBlingoMemberTextBase", "Color"),
        ["color"] = new MemberPropertyMapping("IBlingoMemberTextBase", "Color"),
        ["bold"] = new MemberPropertyMapping("IBlingoMemberTextBase", "Bold"),
        ["italic"] = new MemberPropertyMapping("IBlingoMemberTextBase", "Italic"),
        ["underline"] = new MemberPropertyMapping("IBlingoMemberTextBase", "Underline"),
        ["alignment"] = new MemberPropertyMapping("IBlingoMemberTextBase", "Alignment"),
        ["margin"] = new MemberPropertyMapping("IBlingoMemberTextBase", "Margin"),
        ["loop"] = new MemberPropertyMapping("BlingoMemberSound", "Loop"),
        ["stereo"] = new MemberPropertyMapping("BlingoMemberSound", "Stereo"),
        ["length"] = new MemberPropertyMapping("BlingoMemberSound", "Length"),
        ["linked"] = new MemberPropertyMapping("BlingoMemberSound", "IsLinked"),
        ["islinked"] = new MemberPropertyMapping("BlingoMemberSound", "IsLinked"),
        ["linkedfilepath"] = new MemberPropertyMapping("BlingoMemberSound", "LinkedFilePath"),
        ["isexternal"] = new MemberPropertyMapping("BlingoMemberSound", "IsExternal"),
        ["imagedata"] = new MemberPropertyMapping("BlingoMemberBitmap", "ImageData"),
        ["isloaded"] = new MemberPropertyMapping("BlingoMemberBitmap", "IsLoaded"),
        ["format"] = new MemberPropertyMapping("BlingoMemberBitmap", "Format"),
        ["vertexlist"] = new MemberPropertyMapping("BlingoMemberShape", "VertexList"),
        ["shapetype"] = new MemberPropertyMapping("BlingoMemberShape", "ShapeType"),
        ["shapetypeint"] = new MemberPropertyMapping("BlingoMemberShape", "ShapeTypeInt"),
        ["fillcolor"] = new MemberPropertyMapping("BlingoMemberShape", "FillColor"),
        ["endcolor"] = new MemberPropertyMapping("BlingoMemberShape", "EndColor"),
        ["strokecolor"] = new MemberPropertyMapping("BlingoMemberShape", "StrokeColor"),
        ["strokewidth"] = new MemberPropertyMapping("BlingoMemberShape", "StrokeWidth"),
        ["closed"] = new MemberPropertyMapping("BlingoMemberShape", "Closed"),
        ["antialias"] = new MemberPropertyMapping("BlingoMemberShape", "AntiAlias"),
        ["filled"] = new MemberPropertyMapping("BlingoMemberShape", "Filled"),
        ["duration"] = new MemberPropertyMapping("BlingoMemberMedia", "Duration"),
        ["currenttime"] = new MemberPropertyMapping("BlingoMemberMedia", "CurrentTime"),
        ["mediastatus"] = new MemberPropertyMapping("BlingoMemberMedia", "MediaStatus"),
        ["scripttype"] = new MemberPropertyMapping("BlingoMemberScript", "ScriptType"),
        ["behaviortypename"] = new MemberPropertyMapping("BlingoMemberScript", "BehaviorTypeName"),
    };

    private static readonly Dictionary<string, string> s_thePropertyMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["mouseh"] = "_Mouse.MouseH",
        ["mousev"] = "_Mouse.MouseV",
        ["actorlist"] = "_Movie.ActorList",
        ["timeoutlist"] = "_Movie.TimeOutList",
        ["frame"] = "_Movie.Frame",
        ["currentframe"] = "_Movie.CurrentFrame",
        ["framecount"] = "_Movie.FrameCount",
        ["tempo"] = "_Movie.Tempo",
        ["isplaying"] = "_Movie.IsPlaying",
        ["timer"] = "_Movie.Timer",
        ["spritetotalcount"] = "_Movie.SpriteTotalCount",
        ["spritemaxnumber"] = "_Movie.SpriteMaxNumber",
        ["lastchannel"] = "_Movie.LastChannel",
        ["lastframe"] = "_Movie.LastFrame",
        ["markerlist"] = "_Movie.MarkerList",
        ["maxspritechannelcount"] = "_Movie.MaxSpriteChannelCount",
        ["about"] = "_Movie.About",
        ["copyright"] = "_Movie.Copyright",
        ["username"] = "_Movie.UserName",
        ["companyname"] = "_Movie.CompanyName",
        ["number of score"] = "_Movie.Number",
        ["castlib"] = "_Movie.CastLib",
        ["movie"] = "_Movie",
        ["player"] = "_Player",
        ["activecastlib"] = "_Player.ActiveCastLib",
        ["activemovie"] = "_Player.ActiveMovie",
        ["sound"] = "_Player.Sound",
        ["mediarequiresasyncpreload"] = "_Player.MediaRequiresAsyncPreload",
        ["currentspritenum"] = "_Player.CurrentSpriteNum",
        ["netpreset"] = "_Player.NetPreset",
        ["activewindow"] = "_Player.ActiveWindow",
        ["safeplayer"] = "_Player.SafePlayer",
        ["organizationname"] = "_Player.OrganizationName",
        ["applicationname"] = "_Player.ApplicationName",
        ["applicationpath"] = "_Player.ApplicationPath",
        ["productname"] = "_Player.ProductName",
        ["lastclick"] = "_Player.LastClick",
        ["lastevent"] = "_Player.LastEvent",
        ["lastkey"] = "_Player.LastKey",
        ["productversion"] = "_Player.ProductVersion",
        ["castlibs"] = "_Player.CastLibs",
        ["alerthook"] = "_Player.AlertHook",
        ["stage"] = "_Player.Stage",
    };

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
                TryHandleValueFunction() ||
                TryHandleAlertCommand() ||
                TryHandleGoToCurrentFrame() ||
                TryHandleMemberTypedAccess() ||
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

    private bool TryHandleValueFunction()
    {
        if (_index >= _tokens.Count ||
            !string.Equals(_tokens[_index].ValueText, "value", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (_index + 1 >= _tokens.Count || _tokens[_index + 1].Kind != BlSyntaxKind.LeftParenthesisToken)
        {
            return false;
        }

        var closeIndex = BlLegacyHandlerTokenUtilities.FindMatchingToken(
            _tokens,
            _index + 1,
            BlSyntaxKind.LeftParenthesisToken,
            BlSyntaxKind.RightParenthesisToken);

        if (closeIndex < 0)
        {
            return false;
        }

        var argumentTokens = BlLegacyHandlerTokenUtilities.SliceTokens(
            _tokens,
            _index + 2,
            closeIndex - (_index + 2));
        var argument = Convert(argumentTokens);

        AppendRaw("Convert.ToInt32");
        AppendRaw("(");
        if (!string.IsNullOrEmpty(argument))
        {
            AppendRaw(argument);
        }

        AppendRaw(")");
        _index = closeIndex + 1;
        _afterDot = false;
        return true;
    }

    private bool TryHandleAlertCommand()
    {
        if (_index >= _tokens.Count ||
            !string.Equals(_tokens[_index].ValueText, "alert", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        _index++;
        var argumentTokens = new List<BlSyntaxToken>();
        while (_index < _tokens.Count)
        {
            argumentTokens.Add(_tokens[_index]);
            _index++;
        }

        var argument = Convert(argumentTokens);
        AppendRaw("_Player.Alert");
        AppendRaw("(");
        if (!string.IsNullOrEmpty(argument))
        {
            AppendRaw(argument);
        }

        AppendRaw(")");
        _afterDot = false;
        return true;
    }

    private bool TryHandleGoToCurrentFrame()
    {
        if (_index >= _tokens.Count || !IsIdentifier(_tokens[_index], "go"))
        {
            return false;
        }

        var lookahead = 1;
        if (_index + lookahead >= _tokens.Count || !IsIdentifier(_tokens[_index + lookahead], "to"))
        {
            return false;
        }

        lookahead++;
        if (_index + lookahead < _tokens.Count && IsIdentifier(_tokens[_index + lookahead], "the"))
        {
            lookahead++;
        }

        if (_index + lookahead >= _tokens.Count || !IsIdentifier(_tokens[_index + lookahead], "frame"))
        {
            return false;
        }

        lookahead++;
        if (_index + lookahead < _tokens.Count && !ContainsNewLine(_tokens[_index + lookahead].LeadingTrivia))
        {
            return false;
        }

        AppendRaw("_Movie.GoTo(_Movie.CurrentFrame)");
        _index += lookahead;
        _afterDot = false;
        return true;
    }

    private bool TryHandleMemberTypedAccess()
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
            _tokens[argsClose + 1].Kind != BlSyntaxKind.PeriodToken)
        {
            return false;
        }

        var propertyToken = _tokens[argsClose + 2];
        if (propertyToken.Kind is not (BlSyntaxKind.IdentifierToken or BlSyntaxKind.KeywordToken))
        {
            return false;
        }

        if (!s_memberPropertyMap.TryGetValue(propertyToken.ValueText, out var mapping))
        {
            return false;
        }

        var argsTokens = BlLegacyHandlerTokenUtilities.SliceTokens(_tokens, _index + 2, argsClose - (_index + 2));
        var arguments = ConvertArguments(argsTokens);

        AppendRaw($"Member<{mapping.GenericType}>");
        AppendRaw("(");
        if (!string.IsNullOrEmpty(arguments))
        {
            AppendRaw(arguments);
        }

        AppendRaw(")");
        AppendRaw(".");
        AppendRaw(mapping.PropertyName);

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

        if (_parts.Count > 0)
        {
            var previous = _parts[^1];
            if (!string.IsNullOrEmpty(previous))
            {
                var lastChar = previous[^1];
                if (char.IsLetterOrDigit(lastChar) || lastChar == ')' || lastChar == ']')
                {
                    return false;
                }
            }
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

        var lookahead = 1;
        var builder = new StringBuilder();

        while (_index + lookahead < _tokens.Count)
        {
            var candidate = _tokens[_index + lookahead];
            if (candidate.Kind is BlSyntaxKind.IdentifierToken or BlSyntaxKind.KeywordToken)
            {
                if (builder.Length > 0)
                {
                    builder.Append(' ');
                }

                builder.Append(candidate.ValueText);
                lookahead++;
                continue;
            }

            break;
        }

        if (builder.Length == 0)
        {
            return false;
        }

        var propertyKey = builder.ToString();
        if (!s_thePropertyMap.TryGetValue(propertyKey, out var mapped))
        {
            return false;
        }

        AppendRaw(mapped);
        _index += lookahead;
        _afterDot = false;
        return true;
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

        if (string.Equals(text, "member", StringComparison.OrdinalIgnoreCase))
        {
            AppendRaw("Member");
            _afterDot = false;
            return;
        }

        if (string.Equals(text, "sprite", StringComparison.OrdinalIgnoreCase))
        {
            AppendRaw("Sprite");
            _afterDot = false;
            return;
        }

        if (string.Equals(text, "keypressed", StringComparison.OrdinalIgnoreCase))
        {
            AppendRaw("_Key.KeyPressed");
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

    private static bool IsIdentifier(BlSyntaxToken token, string value)
    {
        return token.Kind is BlSyntaxKind.IdentifierToken or BlSyntaxKind.KeywordToken &&
            string.Equals(token.ValueText, value, StringComparison.OrdinalIgnoreCase);
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
