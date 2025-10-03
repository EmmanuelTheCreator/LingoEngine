using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using BlingoEngine.Legacy.Lingo.Analysis;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.CodeGen;

public sealed class BlLegacyExpressionConverter
{
    private readonly IReadOnlyList<BlSyntaxToken> _tokens;
    private readonly bool _treatReturnAsStatement;
    private readonly List<string> _parts = new();
    private int _index;
    private bool _afterDot;


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

    private sealed record PropertyListCommandInfo
    {
        public PropertyListCommandInfo(string methodName, int requiredArgumentCount, int[]? symbolArgumentIndices = null)
        {
            MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
            RequiredArgumentCount = requiredArgumentCount;
            SymbolArgumentIndices = symbolArgumentIndices ?? Array.Empty<int>();
        }

        public string MethodName { get; }

        public int RequiredArgumentCount { get; }

        public int[] SymbolArgumentIndices { get; }
    }

    private sealed record PropertyListFunctionInfo
    {
        public PropertyListFunctionInfo(
            string methodName,
            int requiredArgumentCount,
            int[]? symbolArgumentIndices = null,
            bool isPropertyAccess = false)
        {
            MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
            RequiredArgumentCount = requiredArgumentCount;
            SymbolArgumentIndices = symbolArgumentIndices ?? Array.Empty<int>();
            IsPropertyAccess = isPropertyAccess;
        }

        public string MethodName { get; }

        public int RequiredArgumentCount { get; }

        public int[] SymbolArgumentIndices { get; }

        public bool IsPropertyAccess { get; }
    }

    private sealed record ListCommandInfo
    {
        public ListCommandInfo(string methodName, int requiredArgumentCount)
        {
            MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
            RequiredArgumentCount = requiredArgumentCount;
        }

        public string MethodName { get; }

        public int RequiredArgumentCount { get; }
    }

    private sealed record ListFunctionInfo
    {
        public ListFunctionInfo(string methodName, int requiredArgumentCount, bool isPropertyAccess = false)
        {
            MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
            RequiredArgumentCount = requiredArgumentCount;
            IsPropertyAccess = isPropertyAccess;
        }

        public string MethodName { get; }

        public int RequiredArgumentCount { get; }

        public bool IsPropertyAccess { get; }
    }

    private static readonly Dictionary<string, PropertyListCommandInfo> s_propertyListCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        ["addprop"] = new PropertyListCommandInfo("Add", 3, new[] { 0 }),
        ["setprop"] = new PropertyListCommandInfo("SetProp", 3, new[] { 0 }),
        ["deleteprop"] = new PropertyListCommandInfo("DeleteProp", 2, new[] { 0 }),
        ["setaprop"] = new PropertyListCommandInfo("SetaProp", 3, new[] { 0 }),
        ["addat"] = new PropertyListCommandInfo("AddAt", 4, new[] { 1 }),
        ["deleteat"] = new PropertyListCommandInfo("DeleteAt", 2),
        ["setat"] = new PropertyListCommandInfo("SetAt", 3),
    };

    private static readonly Dictionary<string, PropertyListFunctionInfo> s_propertyListFunctions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["getprop"] = new PropertyListFunctionInfo("GetProp", 2, new[] { 0 }),
        ["getaprop"] = new PropertyListFunctionInfo("GetaProp", 2, new[] { 0 }),
        ["getpropat"] = new PropertyListFunctionInfo("GetPropAt", 2),
        ["findpos"] = new PropertyListFunctionInfo("FindPos", 2, new[] { 0 }),
        ["findposnear"] = new PropertyListFunctionInfo("FindPosNear", 2, new[] { 0 }),
        ["getpos"] = new PropertyListFunctionInfo("GetPos", 2),
        ["getat"] = new PropertyListFunctionInfo("GetAt", 2),
        ["count"] = new PropertyListFunctionInfo("Count", 1, isPropertyAccess: true),
        ["duplicate"] = new PropertyListFunctionInfo("Duplicate", 1),
    };

    private static readonly Dictionary<string, ListCommandInfo> s_listCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        ["add"] = new ListCommandInfo("Add", 2),
        ["addat"] = new ListCommandInfo("AddAt", 3),
        ["deleteat"] = new ListCommandInfo("DeleteAt", 2),
        ["setat"] = new ListCommandInfo("SetAt", 3),
        ["deleteone"] = new ListCommandInfo("DeleteOne", 2),
        ["deleteall"] = new ListCommandInfo("DeleteAll", 1),
        ["sort"] = new ListCommandInfo("Sort", 1),
    };

    private static readonly Dictionary<string, ListFunctionInfo> s_listFunctions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["getone"] = new ListFunctionInfo("GetOne", 1),
        ["getlast"] = new ListFunctionInfo("GetLast", 1),
        ["getavalue"] = new ListFunctionInfo("GetAValue", 1),
        ["ilk"] = new ListFunctionInfo("Ilk", 1),
        ["listp"] = new ListFunctionInfo("ListP", 1),
        ["max"] = new ListFunctionInfo("Max", 1),
        ["min"] = new ListFunctionInfo("Min", 1),
    };

    public BlLegacyExpressionConverter(IReadOnlyList<BlSyntaxToken> tokens, bool treatReturnAsStatement = false)
    {
        _tokens = tokens ?? Array.Empty<BlSyntaxToken>();
        _treatReturnAsStatement = treatReturnAsStatement;
    }

    public static string Convert(IReadOnlyList<BlSyntaxToken> tokens, bool treatReturnAsStatement = false)
    {
        if (tokens is null || tokens.Count == 0)
        {
            return string.Empty;
        }

        return new BlLegacyExpressionConverter(tokens, treatReturnAsStatement).Build();
    }

    public string Build()
    {
        while (_index < _tokens.Count)
        {
            if (TryHandlePropertyListCommand() ||
                TryHandlePropertyListFunction() ||
                TryHandleListCommand() ||
                TryHandleListFunction() ||
                TryHandleVoidPredicate() ||
                TryHandleObjectPredicate() ||
                TryHandleScriptInstantiation() ||
                TryHandleStringFunction() ||
                TryHandleValueFunction() ||
                TryHandleAlertCommand() ||
                TryHandleGoToMovieNavigation() ||
                TryHandleGoToCurrentFrame() ||
                TryHandleMemberTypedAccess() ||
                TryHandleListLiteral() ||
                TryHandleTheKeywords())
            {
                continue;
            }

            var currentIndex = _index;
            var token = _tokens[_index++];
            switch (token.Kind)
            {
                case BlSyntaxKind.IdentifierToken:
                case BlSyntaxKind.KeywordToken:
                    AppendIdentifier(token, currentIndex);
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

    private bool TryHandleObjectPredicate()
    {
        if (_index >= _tokens.Count ||
            !string.Equals(_tokens[_index].ValueText, "objectp", StringComparison.OrdinalIgnoreCase))
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

        var innerTokens = BlLegacyHandlerTokenUtilities.SliceTokens(
            _tokens,
            _index + 2,
            closeIndex - (_index + 2));
        var expression = Convert(innerTokens);

        if (string.IsNullOrEmpty(expression))
        {
            AppendRaw("false");
        }
        else
        {
            AppendRaw(expression);
            AppendRaw("is");
            AppendRaw("BlingoEngine.Core.IBlingoScriptBase");
            AppendRaw("or");
            AppendRaw("BlingoEngine.Xtras.IBlingoXtra");
            AppendRaw("or");
            AppendRaw("BlingoEngine.Core.IBlingoWindow");
        }

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

    private bool TryHandleStringFunction()
    {
        if (_index >= _tokens.Count ||
            !string.Equals(_tokens[_index].ValueText, "string", StringComparison.OrdinalIgnoreCase))
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

        if (string.IsNullOrEmpty(argument))
        {
            AppendRaw("string.Empty");
        }
        else
        {
            if (RequiresParentheses(argumentTokens))
            {
                AppendRaw("(");
                AppendRaw(argument);
                AppendRaw(")");
            }
            else
            {
                AppendRaw(argument);
            }

            AppendRaw(".ToString()");
        }

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

    private bool TryHandleGoToMovieNavigation()
    {
        if (_index >= _tokens.Count || !IsIdentifier(_tokens[_index], "go"))
        {
            return false;
        }

        if (_index + 1 >= _tokens.Count || !IsIdentifier(_tokens[_index + 1], "to"))
        {
            return false;
        }

        var argumentStart = _index + 2;
        if (argumentStart >= _tokens.Count)
        {
            return false;
        }

        if (IsIdentifier(_tokens[argumentStart], "the"))
        {
            argumentStart++;
            if (argumentStart >= _tokens.Count)
            {
                return false;
            }
        }

        if (IsIdentifier(_tokens[argumentStart], "frame"))
        {
            var nextIndex = argumentStart + 1;
            if (nextIndex >= _tokens.Count || ContainsNewLine(_tokens[nextIndex].LeadingTrivia))
            {
                return false;
            }

            var nextToken = _tokens[nextIndex];
            if (IsValueToken(nextToken))
            {
                argumentStart = nextIndex;
            }
        }

        if (argumentStart >= _tokens.Count)
        {
            return false;
        }

        var argumentTokens = BlLegacyHandlerTokenUtilities.SliceTokens(_tokens, argumentStart, _tokens.Count - argumentStart);
        var argumentExpression = Convert(argumentTokens);
        if (string.IsNullOrWhiteSpace(argumentExpression))
        {
            return false;
        }

        AppendRaw("_movie.GoTo(");
        AppendRaw(argumentExpression);
        AppendRaw(")");
        _index = argumentStart + argumentTokens.Count;
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

        AppendRaw("_movie.GoTo(_movie.CurrentFrame)");
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

        if (!BlLegacyMemberPropertyFacts.TryGet(propertyToken.ValueText, out var mapping))
        {
            return false;
        }

        var argsTokens = BlLegacyHandlerTokenUtilities.SliceTokens(_tokens, _index + 2, argsClose - (_index + 2));
        var arguments = ConvertArguments(argsTokens);

        AppendRaw($"Member<{mapping.MemberTypeName}>");
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
    private static bool RequiresParentheses(IReadOnlyList<BlSyntaxToken> tokens)
    {
        if (tokens.Count == 0)
        {
            return false;
        }

        if (tokens.Count == 1)
        {
            return tokens[0].Kind switch
            {
                BlSyntaxKind.IdentifierToken => false,
                BlSyntaxKind.KeywordToken => false,
                BlSyntaxKind.NumberToken => false,
                BlSyntaxKind.StringLiteralToken => false,
                _ => true,
            };
        }

        if (tokens[0].Kind == BlSyntaxKind.LeftParenthesisToken)
        {
            var closeIndex = BlLegacyHandlerTokenUtilities.FindMatchingToken(
                tokens,
                0,
                BlSyntaxKind.LeftParenthesisToken,
                BlSyntaxKind.RightParenthesisToken);
            if (closeIndex == tokens.Count - 1)
            {
                return false;
            }
        }

        return true;
    }


    private void AppendIdentifier(BlSyntaxToken token, int tokenIndex)

    {
        var text = token.ValueText;

        if (string.Equals(text, "return", StringComparison.OrdinalIgnoreCase))
        {
            if (_treatReturnAsStatement || IsReturnStatement(tokenIndex))
            {
                AppendRaw("return");
            }
            else
            {
                AppendRaw("Environment.NewLine");
            }
            _afterDot = false;
            return;
        }

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

        if (string.Equals(text, "or", StringComparison.OrdinalIgnoreCase))
        {
            AppendRaw("||");
            _afterDot = false;
            return;
        }

        if (string.Equals(text, "and", StringComparison.OrdinalIgnoreCase))
        {
            AppendRaw("&&");
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

    private bool IsReturnStatement(int tokenIndex)
    {
        if ((uint)tokenIndex >= (uint)_tokens.Count)
        {
            return false;
        }

        for (var index = tokenIndex + 1; index < _tokens.Count; index++)
        {
            var token = _tokens[index];
            switch (token.Kind)
            {
                case BlSyntaxKind.OperatorToken:
                    if (string.Equals(token.ValueText, "-", StringComparison.Ordinal) ||
                        string.Equals(token.ValueText, "+", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    return false;
                case BlSyntaxKind.IdentifierToken:
                case BlSyntaxKind.KeywordToken:
                case BlSyntaxKind.NumberToken:
                case BlSyntaxKind.StringLiteralToken:
                case BlSyntaxKind.SymbolToken:
                case BlSyntaxKind.LeftParenthesisToken:
                case BlSyntaxKind.HashToken:
                    return true;
                case BlSyntaxKind.CommaToken:
                case BlSyntaxKind.RightParenthesisToken:
                case BlSyntaxKind.RightBracketToken:
                case BlSyntaxKind.RightBraceToken:
                case BlSyntaxKind.SemicolonToken:
                    return false;
            }
        }

        return false;
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

        if (string.Equals(op, "or", StringComparison.OrdinalIgnoreCase))
        {
            AppendRaw("||");
            return;
        }

        if (string.Equals(op, "and", StringComparison.OrdinalIgnoreCase))
        {
            AppendRaw("&&");
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

    private static bool IsValueToken(BlSyntaxToken token)
    {
        return token.Kind is BlSyntaxKind.NumberToken or
            BlSyntaxKind.StringLiteralToken or
            BlSyntaxKind.IdentifierToken or
            BlSyntaxKind.SymbolToken;
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

    private bool TryHandleListCommand()
    {
        if (_index >= _tokens.Count)
        {
            return false;
        }

        var token = _tokens[_index];
        if (token.Kind is not (BlSyntaxKind.IdentifierToken or BlSyntaxKind.KeywordToken))
        {
            return false;
        }

        if (!s_listCommands.TryGetValue(token.ValueText, out var info))
        {
            return false;
        }

        if (_index + 1 >= _tokens.Count)
        {
            return false;
        }

        var argumentTokens = BlLegacyHandlerTokenUtilities.SliceTokens(_tokens, _index + 1, _tokens.Count - (_index + 1));
        var segments = BlLegacyHandlerTokenUtilities.SplitByComma(argumentTokens);
        if (segments.Count < info.RequiredArgumentCount)
        {
            return false;
        }

        var listExpression = Convert(segments[0]);
        if (string.IsNullOrEmpty(listExpression))
        {
            return false;
        }

        var arguments = new List<string>(segments.Count - 1);
        for (var i = 1; i < segments.Count; i++)
        {
            arguments.Add(Convert(segments[i]));
        }

        AppendRaw(listExpression);
        AppendRaw(".");
        AppendRaw(info.MethodName);
        AppendRaw("(");
        for (var i = 0; i < arguments.Count; i++)
        {
            AppendRaw(arguments[i]);
            if (i < arguments.Count - 1)
            {
                AppendRaw(", ");
            }
        }

        AppendRaw(")");
        _index = _tokens.Count;
        _afterDot = false;
        return true;
    }

    private bool TryHandleListFunction()
    {
        if (_index >= _tokens.Count)
        {
            return false;
        }

        var token = _tokens[_index];
        if (token.Kind is not (BlSyntaxKind.IdentifierToken or BlSyntaxKind.KeywordToken))
        {
            return false;
        }

        if (!s_listFunctions.TryGetValue(token.ValueText, out var info))
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
        var segments = BlLegacyHandlerTokenUtilities.SplitByComma(argumentTokens);
        if (segments.Count < info.RequiredArgumentCount)
        {
            return false;
        }

        if (info.IsPropertyAccess && segments.Count > info.RequiredArgumentCount)
        {
            return false;
        }

        var listExpression = Convert(segments[0]);
        if (string.IsNullOrEmpty(listExpression))
        {
            return false;
        }

        var arguments = new List<string>(segments.Count - 1);
        for (var i = 1; i < segments.Count; i++)
        {
            arguments.Add(Convert(segments[i]));
        }

        AppendRaw(listExpression);
        AppendRaw(".");
        AppendRaw(info.MethodName);
        if (!info.IsPropertyAccess)
        {
            AppendRaw("(");
            for (var i = 0; i < arguments.Count; i++)
            {
                AppendRaw(arguments[i]);
                if (i < arguments.Count - 1)
                {
                    AppendRaw(", ");
                }
            }

            AppendRaw(")");
        }

        _index = closeIndex + 1;
        _afterDot = false;
        return true;
    }

    private bool TryHandlePropertyListCommand()
    {
        if (_index >= _tokens.Count)
        {
            return false;
        }

        var token = _tokens[_index];
        if (token.Kind is not (BlSyntaxKind.IdentifierToken or BlSyntaxKind.KeywordToken))
        {
            return false;
        }

        if (!s_propertyListCommands.TryGetValue(token.ValueText, out var info))
        {
            return false;
        }

        if (_index + 1 >= _tokens.Count)
        {
            return false;
        }

        var argumentTokens = BlLegacyHandlerTokenUtilities.SliceTokens(_tokens, _index + 1, _tokens.Count - (_index + 1));
        var segments = BlLegacyHandlerTokenUtilities.SplitByComma(argumentTokens);
        if (segments.Count < info.RequiredArgumentCount)
        {
            return false;
        }

        var listExpression = Convert(segments[0]);
        if (string.IsNullOrEmpty(listExpression))
        {
            return false;
        }

        var arguments = new List<string>(segments.Count - 1);
        for (var i = 1; i < segments.Count; i++)
        {
            var segmentTokens = segments[i];
            var expression = Convert(segmentTokens);
            if (ContainsSymbolArgument(info.SymbolArgumentIndices, i - 1))
            {
                expression = EnsureSymbol(segmentTokens, expression);
            }

            arguments.Add(expression);
        }

        AppendRaw(listExpression);
        AppendRaw(".");
        AppendRaw(info.MethodName);
        AppendRaw("(");
        for (var i = 0; i < arguments.Count; i++)
        {
            AppendRaw(arguments[i]);
            if (i < arguments.Count - 1)
            {
                AppendRaw(", ");
            }
        }

        AppendRaw(")");
        _index = _tokens.Count;
        _afterDot = false;
        return true;
    }

    private bool TryHandlePropertyListFunction()
    {
        if (_index >= _tokens.Count)
        {
            return false;
        }

        var token = _tokens[_index];
        if (token.Kind is not (BlSyntaxKind.IdentifierToken or BlSyntaxKind.KeywordToken))
        {
            return false;
        }

        if (!s_propertyListFunctions.TryGetValue(token.ValueText, out var info))
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
        var segments = BlLegacyHandlerTokenUtilities.SplitByComma(argumentTokens);
        if (segments.Count < info.RequiredArgumentCount)
        {
            return false;
        }

        if (info.IsPropertyAccess && segments.Count > info.RequiredArgumentCount)
        {
            return false;
        }

        var listExpression = Convert(segments[0]);
        if (string.IsNullOrEmpty(listExpression))
        {
            return false;
        }

        var arguments = new List<string>(segments.Count - 1);
        for (var i = 1; i < segments.Count; i++)
        {
            var segmentTokens = segments[i];
            var expression = Convert(segmentTokens);
            if (ContainsSymbolArgument(info.SymbolArgumentIndices, i - 1))
            {
                expression = EnsureSymbol(segmentTokens, expression);
            }

            arguments.Add(expression);
        }

        AppendRaw(listExpression);
        AppendRaw(".");
        AppendRaw(info.MethodName);
        if (!info.IsPropertyAccess)
        {
            AppendRaw("(");
            for (var i = 0; i < arguments.Count; i++)
            {
                AppendRaw(arguments[i]);
                if (i < arguments.Count - 1)
                {
                    AppendRaw(", ");
                }
            }

            AppendRaw(")");
        }

        _index = closeIndex + 1;
        _afterDot = false;
        return true;
    }

    private static bool ContainsSymbolArgument(int[] indices, int index)
    {
        if (indices is null || indices.Length == 0)
        {
            return false;
        }

        return Array.IndexOf(indices, index) >= 0;
    }

    private static string EnsureSymbol(IReadOnlyList<BlSyntaxToken> tokens, string expression)
    {
        if (string.IsNullOrEmpty(expression))
        {
            return expression;
        }

        if (expression.StartsWith("Symbol(", StringComparison.Ordinal))
        {
            return expression;
        }

        if (tokens.Count > 0 && tokens[0].Kind == BlSyntaxKind.SymbolToken)
        {
            return $"Symbol({ToStringLiteral(tokens[0].ValueText)})";
        }

        if (expression[0] == '#')
        {
            return $"Symbol({ToStringLiteral(expression[1..])})";
        }

        return expression;
    }

    private static string ToStringLiteral(string value)
    {
        var builder = new StringBuilder();
        builder.Append('"');
        if (!string.IsNullOrEmpty(value))
        {
            for (var index = 0; index < value.Length; index++)
            {
                var ch = value[index];
                builder.Append(ch switch
                {
                    '\\' => "\\\\",
                    '"' => "\\\"",
                    '\n' => "\\n",
                    '\r' => "\\r",
                    '\t' => "\\t",
                    _ => ch.ToString(),
                });
            }
        }

        builder.Append('"');
        return builder.ToString();
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
