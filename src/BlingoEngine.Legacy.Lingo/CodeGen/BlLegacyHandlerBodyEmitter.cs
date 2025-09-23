using System;
using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.Analysis;

namespace BlingoEngine.Legacy.Lingo.CodeGen;

public sealed class BlLegacyHandlerBodyEmitter : IBlLegacyHandlerBlockDataVisitor
{
    private readonly BlCSharpCodeWriter _writer;
    private readonly IReadOnlyList<BlLingoHandlerCodeBlock> _blocks;
    private readonly BlLegacyClassGeneratorOptions _options;
    private readonly BlLingoAnalysisResult _analysis;
    private readonly HandlerBlockManager _blockManager;

    public BlLegacyHandlerBodyEmitter(
        BlCSharpCodeWriter writer,
        IReadOnlyList<BlLingoHandlerCodeBlock> blocks,
        BlLegacyClassGeneratorOptions options,
        BlLingoAnalysisResult analysis)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        _blocks = blocks ?? Array.Empty<BlLingoHandlerCodeBlock>();
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _analysis = analysis ?? throw new ArgumentNullException(nameof(analysis));
        _blockManager = new HandlerBlockManager(_writer);
    }

    public void Emit()
    {
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
        var method = data.IsRemoval ? "Remove" : "Add";
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
