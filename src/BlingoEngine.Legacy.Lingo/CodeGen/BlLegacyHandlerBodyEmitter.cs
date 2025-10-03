using System;
using System.Collections.Generic;
using System.Text;
using BlingoEngine.Legacy.Lingo.Analysis;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.CodeGen;

public sealed class BlLegacyHandlerBodyEmitter : IBlLegacyHandlerBlockDataVisitor
{
    private readonly BlCSharpCodeWriter _writer;
    private readonly IReadOnlyList<BlLingoHandlerCodeBlock> _blocks;
    private readonly BlLegacyClassGeneratorOptions _options;
    private readonly BlLingoAnalysisResult _analysis;
    private readonly HandlerBlockManager _blockManager;
    private readonly BlLingoHandlerSymbolTable _handler;
    private readonly bool _isPropertyDescriptionHandler;

    public BlLegacyHandlerBodyEmitter(
        BlCSharpCodeWriter writer,
        BlLingoHandlerSymbolTable handler,
        IReadOnlyList<BlLingoHandlerCodeBlock> blocks,
        BlLegacyClassGeneratorOptions options,
        BlLingoAnalysisResult analysis)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        _blocks = blocks ?? Array.Empty<BlLingoHandlerCodeBlock>();
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _analysis = analysis ?? throw new ArgumentNullException(nameof(analysis));
        _blockManager = new HandlerBlockManager(_writer);
        _isPropertyDescriptionHandler = string.Equals(
            handler.Symbol.Name,
            "GetPropertyDescriptionList",
            StringComparison.Ordinal);
    }

    public void Emit()
    {
        if (_isPropertyDescriptionHandler)
        {
            EmitPropertyDescriptionHandler();
            return;
        }

        if (_blocks.Count == 0)
        {
            return;
        }

        foreach (var block in _blocks)
        {
            EmitBlock(block);
        }

        _blockManager.CloseAll();
    }

    private void EmitPropertyDescriptionHandler()
    {
        var entries = new List<PropertyDescription>();
        var positions = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var block in _blocks)
        {
            if (block.Kind != BlLingoHandlerCodeBlockKind.Expression)
            {
                continue;
            }

            if (!TryParseAddProp(block.Tokens, out var description))
            {
                continue;
            }

            if (positions.TryGetValue(description.PropertyName, out var existingIndex))
            {
                entries[existingIndex] = description;
            }
            else
            {
                positions[description.PropertyName] = entries.Count;
                entries.Add(description);
            }
        }

        if (entries.Count == 0)
        {
            _writer.WriteLine("return new BehaviorPropertyDescriptionList();");
            return;
        }

        _writer.WriteLine("return new BehaviorPropertyDescriptionList()");
        using (_writer.IndentScope())
        {
            for (var index = 0; index < entries.Count; index++)
            {
                var entry = entries[index];
                var suffix = index == entries.Count - 1 ? ";" : string.Empty;
                _writer.WriteLine($".Add(this, x => x.{entry.PropertyName}, {entry.Comment}, {entry.DefaultValue}){suffix}");
            }
        }
    }

    private bool TryParseAddProp(IReadOnlyList<BlSyntaxToken> tokens, out PropertyDescription description)
    {
        description = default!;

        if (tokens is null || tokens.Count == 0)
        {
            return false;
        }

        var index = 0;
        if (!TryMatchIdentifier(tokens, ref index, "addProp"))
        {
            return false;
        }

        // Skip the description variable name.
        if (index >= tokens.Count || tokens[index].Kind != BlSyntaxKind.IdentifierToken)
        {
            return false;
        }

        index++;

        if (!TryConsume(tokens, ref index, BlSyntaxKind.CommaToken))
        {
            return false;
        }

        if (!TryConsumeSymbol(tokens, ref index, out var propertyName))
        {
            return false;
        }

        if (!TryConsume(tokens, ref index, BlSyntaxKind.CommaToken))
        {
            return false;
        }

        if (!TryConsume(tokens, ref index, BlSyntaxKind.LeftBracketToken))
        {
            return false;
        }

        string? comment = null;
        string? format = null;
        IReadOnlyList<BlSyntaxToken>? defaultTokens = null;

        while (index < tokens.Count)
        {
            if (tokens[index].Kind == BlSyntaxKind.RightBracketToken)
            {
                index++;
                break;
            }

            if (!TryConsumeSymbol(tokens, ref index, out var key))
            {
                return false;
            }

            if (!TryConsume(tokens, ref index, BlSyntaxKind.ColonToken))
            {
                return false;
            }

            var valueTokens = ReadValueTokens(tokens, ref index);

            if (string.Equals(key, "comment", StringComparison.OrdinalIgnoreCase))
            {
                comment ??= ExtractComment(valueTokens);
            }
            else if (string.Equals(key, "default", StringComparison.OrdinalIgnoreCase))
            {
                defaultTokens = valueTokens;
            }
            else if (string.Equals(key, "format", StringComparison.OrdinalIgnoreCase))
            {
                format = ExtractFormat(valueTokens);
            }

            if (index < tokens.Count && tokens[index].Kind == BlSyntaxKind.CommaToken)
            {
                index++;
            }
            else if (index < tokens.Count && tokens[index].Kind == BlSyntaxKind.RightBracketToken)
            {
                index++;
                break;
            }
        }

        if (string.IsNullOrEmpty(propertyName) || comment is null || defaultTokens is null)
        {
            return false;
        }

        var defaultValue = FormatDefault(defaultTokens, format);
        if (defaultValue is null)
        {
            return false;
        }

        description = new PropertyDescription(propertyName, ToCSharpStringLiteral(comment), defaultValue);
        return true;
    }

    private static bool TryMatchIdentifier(IReadOnlyList<BlSyntaxToken> tokens, ref int index, string expected)
    {
        if (index >= tokens.Count)
        {
            return false;
        }

        var token = tokens[index];
        if (token.Kind != BlSyntaxKind.IdentifierToken ||
            !token.ValueText.Equals(expected, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        index++;
        return true;
    }

    private static bool TryConsume(IReadOnlyList<BlSyntaxToken> tokens, ref int index, BlSyntaxKind kind)
    {
        if (index >= tokens.Count || tokens[index].Kind != kind)
        {
            return false;
        }

        index++;
        return true;
    }

    private static bool TryConsumeSymbol(IReadOnlyList<BlSyntaxToken> tokens, ref int index, out string value)
    {
        value = string.Empty;
        if (index >= tokens.Count)
        {
            return false;
        }

        var token = tokens[index];
        if (token.Kind != BlSyntaxKind.SymbolToken)
        {
            return false;
        }

        value = token.ValueText;
        index++;
        return true;
    }

    private static IReadOnlyList<BlSyntaxToken> ReadValueTokens(IReadOnlyList<BlSyntaxToken> tokens, ref int index)
    {
        var result = new List<BlSyntaxToken>();
        var depth = 0;

        while (index < tokens.Count)
        {
            var token = tokens[index];

            if (token.Kind == BlSyntaxKind.LeftBracketToken || token.Kind == BlSyntaxKind.LeftParenthesisToken)
            {
                depth++;
            }
            else if (token.Kind == BlSyntaxKind.RightBracketToken || token.Kind == BlSyntaxKind.RightParenthesisToken)
            {
                if (depth == 0)
                {
                    break;
                }

                depth--;
            }

            if (depth == 0 && (token.Kind == BlSyntaxKind.CommaToken || token.Kind == BlSyntaxKind.RightBracketToken))
            {
                break;
            }

            result.Add(token);
            index++;
        }

        return result;
    }

    private static string? ExtractComment(IReadOnlyList<BlSyntaxToken> tokens)
    {
        if (tokens.Count == 0)
        {
            return null;
        }

        if (tokens.Count == 1 && tokens[0].Kind == BlSyntaxKind.StringLiteralToken)
        {
            return tokens[0].ValueText;
        }

        var builder = new StringBuilder();
        foreach (var token in tokens)
        {
            if (token.Kind == BlSyntaxKind.StringLiteralToken)
            {
                builder.Append(token.ValueText);
            }
            else
            {
                builder.Append(token.ValueText.Length > 0 ? token.ValueText : token.Text);
            }
        }

        return builder.ToString();
    }

    private static string? ExtractFormat(IReadOnlyList<BlSyntaxToken> tokens)
    {
        if (tokens.Count == 0)
        {
            return null;
        }

        var token = tokens[0];
        if (token.Kind == BlSyntaxKind.SymbolToken || token.Kind == BlSyntaxKind.IdentifierToken)
        {
            return token.ValueText;
        }

        if (token.Kind == BlSyntaxKind.StringLiteralToken)
        {
            return token.ValueText;
        }

        return null;
    }

    private static string? FormatDefault(IReadOnlyList<BlSyntaxToken> tokens, string? format)
    {
        if (tokens.Count == 0)
        {
            return null;
        }

        var treatAsString = string.Equals(format, "string", StringComparison.OrdinalIgnoreCase)
            || string.Equals(format, "symbol", StringComparison.OrdinalIgnoreCase);

        if (treatAsString)
        {
            var text = ExtractComment(tokens) ?? string.Empty;
            return ToCSharpStringLiteral(text);
        }

        if (tokens.Count == 1)
        {
            var token = tokens[0];
            switch (token.Kind)
            {
                case BlSyntaxKind.StringLiteralToken:
                    return ToCSharpStringLiteral(token.ValueText);
                case BlSyntaxKind.NumberToken:
                    return token.ValueText;
                case BlSyntaxKind.IdentifierToken:
                    if (IsBooleanLiteral(token.ValueText))
                    {
                        return token.ValueText.ToLowerInvariant();
                    }

                    return ToCSharpStringLiteral(token.ValueText);
                case BlSyntaxKind.SymbolToken:
                    if (IsBooleanLiteral(token.ValueText))
                    {
                        return token.ValueText.ToLowerInvariant();
                    }

                    return ToCSharpStringLiteral(token.ValueText);
            }
        }

        if (tokens.Count == 2 &&
            tokens[0].Kind == BlSyntaxKind.OperatorToken &&
            tokens[0].Text == "-" &&
            tokens[1].Kind == BlSyntaxKind.NumberToken)
        {
            return "-" + tokens[1].ValueText;
        }

        if (tokens.Count > 0 &&
            tokens[0].Kind == BlSyntaxKind.IdentifierToken &&
            string.Equals(tokens[0].ValueText, "rgb", StringComparison.OrdinalIgnoreCase))
        {
            return FormatRgb(tokens);
        }

        var builder = new StringBuilder();
        foreach (var token in tokens)
        {
            builder.Append(token.Text);
        }

        var combined = builder.ToString();
        if (treatAsString)
        {
            return ToCSharpStringLiteral(combined);
        }

        return combined.Length > 0 ? combined : null;
    }

    private static string? FormatRgb(IReadOnlyList<BlSyntaxToken> tokens)
    {
        if (tokens.Count < 4 || tokens[1].Kind != BlSyntaxKind.LeftParenthesisToken)
        {
            return null;
        }

        var components = new List<string>(3);
        var index = 2;
        var current = new List<BlSyntaxToken>();
        var depth = 0;

        while (index < tokens.Count)
        {
            var token = tokens[index];

            if (token.Kind == BlSyntaxKind.RightParenthesisToken && depth == 0)
            {
                if (current.Count > 0)
                {
                    var value = FormatDefault(current, null);
                    if (value is null)
                    {
                        return null;
                    }

                    components.Add(value);
                }

                break;
            }

            if (token.Kind == BlSyntaxKind.CommaToken && depth == 0)
            {
                var value = FormatDefault(current, null);
                if (value is null)
                {
                    return null;
                }

                components.Add(value);
                current.Clear();
                index++;
                continue;
            }

            if (token.Kind == BlSyntaxKind.LeftParenthesisToken || token.Kind == BlSyntaxKind.LeftBracketToken)
            {
                depth++;
            }
            else if (token.Kind == BlSyntaxKind.RightParenthesisToken || token.Kind == BlSyntaxKind.RightBracketToken)
            {
                if (depth > 0)
                {
                    depth--;
                }
            }

            current.Add(token);
            index++;
        }

        if (components.Count == 2 && current.Count > 0)
        {
            var value = FormatDefault(current, null);
            if (value is null)
            {
                return null;
            }

            components.Add(value);
        }

        if (components.Count != 3)
        {
            return null;
        }

        return $"AColor.FromCode({string.Join(',', components)})";
    }

    private static bool IsBooleanLiteral(string? value)
    {
        return value is not null &&
            (value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
             value.Equals("false", StringComparison.OrdinalIgnoreCase));
    }

    private static string ToCSharpStringLiteral(string value)
    {
        var builder = new StringBuilder(value.Length + 2);
        builder.Append('"');

        foreach (var ch in value)
        {
            builder.Append(ch switch
            {
                '\\' => "\\\\",
                '"' => "\\\"",
                '\r' => "\\r",
                '\n' => "\\n",
                '\t' => "\\t",
                _ => ch.ToString(),
            });
        }

        builder.Append('"');
        return builder.ToString();
    }

    private sealed record PropertyDescription(string PropertyName, string Comment, string DefaultValue);

    private void EmitBlock(BlLingoHandlerCodeBlock block)
    {
        switch (block.Kind)
        {
            case BlLingoHandlerCodeBlockKind.Blank:
                _writer.WriteLine();
                return;
            case BlLingoHandlerCodeBlockKind.Comment:
                WriteComment(block.CommentText);
                return;
            case BlLingoHandlerCodeBlockKind.End:
                _blockManager.CloseBlock();
                return;
            case BlLingoHandlerCodeBlockKind.Else:
                EmitElse();
                return;
            case BlLingoHandlerCodeBlockKind.RepeatForever:
                _writer.WriteLine("while (true)");
                _blockManager.OpenBlock(BlockKind.Loop);
                return;
            case BlLingoHandlerCodeBlockKind.CaseOtherwise:
                _blockManager.BeginSwitchSection();
                _writer.WriteLine("default:");
                _blockManager.BeginSwitchSectionBody();
                return;
            case BlLingoHandlerCodeBlockKind.ExitRepeat:
                _writer.WriteLine("break;");
                return;
            case BlLingoHandlerCodeBlockKind.NextRepeat:
                _writer.WriteLine("continue;");
                return;
            case BlLingoHandlerCodeBlockKind.Return:
                _writer.WriteLine("return;");
                return;
        }

        block.Data?.Visit(this);
    }

    public void Visit(BlLingoIfBlockData data)
    {
        _writer.WriteLine($"if ({data.Condition})");
        _blockManager.OpenBlock(BlockKind.If);
    }

    private void EmitElse()
    {
        _blockManager.CloseBlock(leaveOnStack: true);
        _writer.WriteLine("else");
        _blockManager.OpenBlock(BlockKind.If, reopenExisting: true);
    }

    public void Visit(BlLingoElseIfBlockData data)
    {
        _blockManager.CloseBlock(leaveOnStack: true);
        _writer.WriteLine($"else if ({data.Condition})");
        _blockManager.OpenBlock(BlockKind.If, reopenExisting: true);
    }

    public void Visit(BlLingoRepeatWithRangeBlockData data)
    {
        var variable = EnsureCounterName(data.VariableName);
        _writer.WriteLine($"for (int {variable} = {data.StartExpression}; {variable} <= {data.EndExpression}; {variable}++)");
        _blockManager.OpenBlock(BlockKind.Loop);
    }

    public void Visit(BlLingoRepeatWithEachBlockData data)
    {
        var variable = string.IsNullOrEmpty(data.VariableName) ? "item" : data.VariableName;
        _writer.WriteLine($"foreach (var {variable} in {data.SourceExpression})");
        _blockManager.OpenBlock(BlockKind.Loop);
    }

    public void Visit(BlLingoRepeatWhileBlockData data)
    {
        _writer.WriteLine($"while ({data.Condition})");
        _blockManager.OpenBlock(BlockKind.Loop);
    }

    public void Visit(BlLingoRepeatUntilBlockData data)
    {
        _writer.WriteLine("do");
        _blockManager.OpenBlock(BlockKind.RepeatUntil, data.Condition);
    }

    public void Visit(BlLingoCaseBlockData data)
    {
        _writer.WriteLine($"switch ({data.Expression})");
        _blockManager.OpenBlock(BlockKind.Switch);
    }

    public void Visit(BlLingoCaseWhenBlockData data)
    {
        _blockManager.BeginSwitchSection();
        _writer.WriteLine($"case {data.Expression}:");
        _blockManager.BeginSwitchSectionBody();
    }

    public void Visit(BlLingoPutBlockData data)
    {
        switch (data.Kind)
        {
            case BlLingoPutAssignmentKind.Direct:
                _writer.WriteLine($"{data.TargetExpression} = {data.ValueExpression};");
                break;
            case BlLingoPutAssignmentKind.Field:
                EmitFieldAssignment(data);
                break;
            case BlLingoPutAssignmentKind.SpriteProperty:
                _writer.WriteLine($"Sprite({data.SpriteIndexExpression}).{data.SpritePropertyName} = {data.ValueExpression};");
                break;
            case BlLingoPutAssignmentKind.SpriteMember:
                EmitSpriteMemberAssignment(data);
                break;
            case BlLingoPutAssignmentKind.ListElement:
                _writer.WriteLine($"{data.ListExpression}.SetAt({data.ListIndexExpression}, {data.ValueExpression});");
                break;
        }
    }

    public void Visit(BlLingoActorListMutationBlockData data)
    {
        var method = data.Kind switch
        {
            BlLingoActorListMutationKind.Append => "Append",
            BlLingoActorListMutationKind.DeleteOne => "DeleteOne",
            _ => "Remove",
        };
        _writer.WriteLine($"_Movie.ActorList.{method}({data.ArgumentExpression});");
    }

    public void Visit(BlLingoSendSpriteBlockData data)
    {
        var call = ComposeLambdaInvocation(data.HandlerName, data.ParameterName, data.Arguments);
        var typeSuffix = ComposeBehaviorType(data.TargetScriptName, BlLingoScriptKind.Behavior, data.ParameterName, data.HandlerName);
        var invocation = ComposeSendSpriteInvocation(data, typeSuffix, call);

        if (data.UsesResult && !string.IsNullOrEmpty(data.ResultTargetExpression))
        {
            _writer.WriteLine($"{data.ResultTargetExpression} = {invocation};");
        }
        else
        {
            _writer.WriteLine(invocation + ";");
        }
    }

    public void Visit(BlLingoExitRepeatIfBlockData data)
    {
        var action = data.UseContinue ? "continue" : "break";
        _writer.WriteLine($"if ({data.Condition}) {action};");
    }

    public void Visit(BlLingoMovieCallBlockData data)
    {
        var call = ComposeLambdaInvocation(data.HandlerName, data.ParameterName ?? "movie", data.Arguments);
        var typeSuffix = ComposeBehaviorType(data.TargetScriptName, BlLingoScriptKind.Movie, data.ParameterName ?? "movie", data.HandlerName);
        var invocation = ComposeMovieScriptInvocation(data, typeSuffix, call);

        if (data.UsesResult && !string.IsNullOrEmpty(data.ResultTargetExpression))
        {
            _writer.WriteLine($"{data.ResultTargetExpression} = {invocation};");
        }
        else
        {
            _writer.WriteLine(invocation + ";");
        }
    }

    public void Visit(BlLingoExpressionBlockData data)
    {
        if (string.IsNullOrEmpty(data.Expression))
        {
            return;
        }

        _writer.WriteLine(data.Expression + ";");
    }

    private void EmitFieldAssignment(BlLingoPutBlockData data)
    {
        var fieldName = data.FieldName ?? string.Empty;
        if (string.IsNullOrEmpty(fieldName))
        {
            return;
        }

        _writer.WriteLine($"TryMember<IBlingoMemberField>({fieldName}, field => field.Text = {data.ValueExpression});");
    }

    private void EmitSpriteMemberAssignment(BlLingoPutBlockData data)
    {
        var args = data.SpriteMemberArguments ?? data.ValueExpression;
        _writer.WriteLine($"Sprite({data.SpriteIndexExpression}).SetMember({args});");
    }

    private void WriteComment(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            _writer.WriteLine("//");
            return;
        }

        _writer.WriteLine("// " + text);
    }

    private static string ComposeLambdaInvocation(string handlerName, string parameterName, IReadOnlyList<string> arguments)
    {
        if (arguments.Count == 0)
        {
            return $"{parameterName}.{handlerName}()";
        }

        return $"{parameterName}.{handlerName}({string.Join(", ", arguments)})";
    }

    private string ComposeSendSpriteInvocation(BlLingoSendSpriteBlockData data, string? typeSuffix, string call)
    {
        var lambda = $"{data.ParameterName} => {call}";

        if (data.UsesResult)
        {
            var behaviorType = string.IsNullOrEmpty(typeSuffix) ? "IBlingoSpriteBehavior" : typeSuffix;
            var resultType = string.IsNullOrEmpty(data.ResultTypeName) ? "object?" : data.ResultTypeName;
            return $"SendSprite<{behaviorType}, {resultType}>({data.ChannelExpression}, {lambda})";
        }

        if (!string.IsNullOrEmpty(typeSuffix))
        {
            return $"SendSprite<{typeSuffix}>({data.ChannelExpression}, {lambda})";
        }

        return $"SendSprite({data.ChannelExpression}, {lambda})";
    }

    private string ComposeMovieScriptInvocation(BlLingoMovieCallBlockData data, string? typeSuffix, string call)
    {
        var lambda = $"{data.ParameterName ?? "movie"} => {call}";
        var scriptType = string.IsNullOrEmpty(typeSuffix) ? "IBlingoMovieScript" : typeSuffix;

        if (data.UsesResult)
        {
            var resultType = string.IsNullOrEmpty(data.ResultTypeName) ? "object?" : data.ResultTypeName;
            return $"CallMovieScript<{scriptType}, {resultType}>({lambda})";
        }

        return $"CallMovieScript<{scriptType}>({lambda})";
    }

    private string ComposeBehaviorType(string? scriptName, BlLingoScriptKind kind, string parameterName, string? fallbackBaseName = null)
    {
        if (string.IsNullOrEmpty(scriptName))
        {
            if (string.IsNullOrEmpty(fallbackBaseName))
            {
                return string.Empty;
            }

            scriptName = DeriveScriptNameFromHandler(fallbackBaseName);
            if (string.IsNullOrEmpty(scriptName))
            {
                return string.Empty;
            }
        }

        var typeName = BlCSharpName.ComposeClassName(scriptName, kind, _options);
        return typeName ?? string.Empty;
    }

    private static string DeriveScriptNameFromHandler(string handlerName)
    {
        if (string.IsNullOrEmpty(handlerName))
        {
            return string.Empty;
        }

        var normalized = BlCSharpName.NormalizeScriptName(handlerName);
        const string HandlerSuffix = "Handler";
        if (normalized.EndsWith(HandlerSuffix, StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[..^HandlerSuffix.Length];
        }

        if (normalized.Length > 0)
        {
            normalized = char.ToUpperInvariant(normalized[0]) + normalized[1..];
        }

        return normalized;
    }

    private static string EnsureCounterName(string candidate)
    {
        if (string.IsNullOrEmpty(candidate))
        {
            return "index";
        }

        return char.IsLetter(candidate[0])
            ? candidate
            : "index";
    }
}
