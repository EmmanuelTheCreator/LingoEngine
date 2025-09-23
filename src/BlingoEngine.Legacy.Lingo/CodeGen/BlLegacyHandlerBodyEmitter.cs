using System;
using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.Analysis;

namespace BlingoEngine.Legacy.Lingo.CodeGen;

public sealed class BlLegacyHandlerBodyEmitter
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
                break;
            case BlLingoHandlerCodeBlockKind.Comment:
                WriteComment(block.CommentText);
                break;
            case BlLingoHandlerCodeBlockKind.End:
                _blockManager.CloseBlock();
                break;
            case BlLingoHandlerCodeBlockKind.If:
                EmitIf(block);
                break;
            case BlLingoHandlerCodeBlockKind.Else:
                EmitElse();
                break;
            case BlLingoHandlerCodeBlockKind.ElseIf:
                EmitElseIf(block);
                break;
            case BlLingoHandlerCodeBlockKind.RepeatWithRange:
                EmitRepeatWithRange(block);
                break;
            case BlLingoHandlerCodeBlockKind.RepeatWithEach:
                EmitRepeatWithEach(block);
                break;
            case BlLingoHandlerCodeBlockKind.RepeatWhile:
                EmitRepeatWhile(block);
                break;
            case BlLingoHandlerCodeBlockKind.RepeatUntil:
                EmitRepeatUntil(block);
                break;
            case BlLingoHandlerCodeBlockKind.RepeatForever:
                _writer.WriteLine("while (true)");
                _blockManager.OpenBlock(BlockKind.Loop);
                break;
            case BlLingoHandlerCodeBlockKind.Case:
                EmitCase(block);
                break;
            case BlLingoHandlerCodeBlockKind.CaseWhen:
                EmitCaseWhen(block);
                break;
            case BlLingoHandlerCodeBlockKind.CaseOtherwise:
                _blockManager.BeginSwitchSection();
                _writer.WriteLine("default:");
                _blockManager.BeginSwitchSectionBody();
                break;
            case BlLingoHandlerCodeBlockKind.Put:
                EmitPut(block);
                break;
            case BlLingoHandlerCodeBlockKind.ActorListAppend:
                EmitActorListAppend(block);
                break;
            case BlLingoHandlerCodeBlockKind.ActorListRemove:
                EmitActorListRemove(block);
                break;
            case BlLingoHandlerCodeBlockKind.SendSprite:
                EmitSendSprite(block);
                break;
            case BlLingoHandlerCodeBlockKind.ExitRepeat:
                _writer.WriteLine("break;");
                break;
            case BlLingoHandlerCodeBlockKind.ExitRepeatIf:
                EmitExitRepeatIf(block);
                break;
            case BlLingoHandlerCodeBlockKind.NextRepeat:
                _writer.WriteLine("continue;");
                break;
            case BlLingoHandlerCodeBlockKind.NextRepeatIf:
                EmitNextRepeatIf(block);
                break;
            case BlLingoHandlerCodeBlockKind.Return:
                _writer.WriteLine("return;");
                break;
            case BlLingoHandlerCodeBlockKind.MovieCall:
                EmitMovieCall(block);
                break;
            case BlLingoHandlerCodeBlockKind.Expression:
                EmitExpression(block);
                break;
            default:
                break;
        }
    }

    private void EmitIf(BlLingoHandlerCodeBlock block)
    {
        if (block.Data is not BlLingoIfBlockData data)
        {
            return;
        }

        _writer.WriteLine($"if ({data.Condition})");
        _blockManager.OpenBlock(BlockKind.If);
    }

    private void EmitElse()
    {
        _blockManager.CloseBlock(leaveOnStack: true);
        _writer.WriteLine("else");
        _blockManager.OpenBlock(BlockKind.If, reopenExisting: true);
    }

    private void EmitElseIf(BlLingoHandlerCodeBlock block)
    {
        if (block.Data is not BlLingoElseIfBlockData data)
        {
            return;
        }

        _blockManager.CloseBlock(leaveOnStack: true);
        _writer.WriteLine($"else if ({data.Condition})");
        _blockManager.OpenBlock(BlockKind.If, reopenExisting: true);
    }

    private void EmitRepeatWithRange(BlLingoHandlerCodeBlock block)
    {
        if (block.Data is not BlLingoRepeatWithRangeBlockData data)
        {
            return;
        }

        var variable = EnsureCounterName(data.VariableName);
        _writer.WriteLine($"for (int {variable} = {data.StartExpression}; {variable} <= {data.EndExpression}; {variable}++)");
        _blockManager.OpenBlock(BlockKind.Loop);
    }

    private void EmitRepeatWithEach(BlLingoHandlerCodeBlock block)
    {
        if (block.Data is not BlLingoRepeatWithEachBlockData data)
        {
            return;
        }

        var variable = string.IsNullOrEmpty(data.VariableName) ? "item" : data.VariableName;
        _writer.WriteLine($"foreach (var {variable} in {data.SourceExpression})");
        _blockManager.OpenBlock(BlockKind.Loop);
    }

    private void EmitRepeatWhile(BlLingoHandlerCodeBlock block)
    {
        if (block.Data is not BlLingoRepeatWhileBlockData data)
        {
            return;
        }

        _writer.WriteLine($"while ({data.Condition})");
        _blockManager.OpenBlock(BlockKind.Loop);
    }

    private void EmitRepeatUntil(BlLingoHandlerCodeBlock block)
    {
        if (block.Data is not BlLingoRepeatUntilBlockData data)
        {
            return;
        }

        _writer.WriteLine("do");
        _blockManager.OpenBlock(BlockKind.RepeatUntil, data.Condition);
    }

    private void EmitCase(BlLingoHandlerCodeBlock block)
    {
        if (block.Data is not BlLingoCaseBlockData data)
        {
            return;
        }

        _writer.WriteLine($"switch ({data.Expression})");
        _blockManager.OpenBlock(BlockKind.Switch);
    }

    private void EmitCaseWhen(BlLingoHandlerCodeBlock block)
    {
        if (block.Data is not BlLingoCaseWhenBlockData data)
        {
            return;
        }

        _blockManager.BeginSwitchSection();
        _writer.WriteLine($"case {data.Expression}:");
        _blockManager.BeginSwitchSectionBody();
    }

    private void EmitPut(BlLingoHandlerCodeBlock block)
    {
        if (block.Data is not BlLingoPutBlockData data)
        {
            return;
        }

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

    private void EmitActorListAppend(BlLingoHandlerCodeBlock block)
    {
        if (block.Data is not BlLingoActorListMutationBlockData data)
        {
            return;
        }

        _writer.WriteLine($"_Movie.ActorList.Add({data.ArgumentExpression});");
    }

    private void EmitActorListRemove(BlLingoHandlerCodeBlock block)
    {
        if (block.Data is not BlLingoActorListMutationBlockData data)
        {
            return;
        }

        _writer.WriteLine($"_Movie.ActorList.Remove({data.ArgumentExpression});");
    }

    private void EmitSendSprite(BlLingoHandlerCodeBlock block)
    {
        if (block.Data is not BlLingoSendSpriteBlockData data)
        {
            return;
        }

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

    private void EmitExitRepeatIf(BlLingoHandlerCodeBlock block)
    {
        if (block.Data is not BlLingoExitRepeatIfBlockData data)
        {
            return;
        }

        _writer.WriteLine($"if ({data.Condition}) break;");
    }

    private void EmitNextRepeatIf(BlLingoHandlerCodeBlock block)
    {
        if (block.Data is not BlLingoExitRepeatIfBlockData data)
        {
            return;
        }

        _writer.WriteLine($"if ({data.Condition}) continue;");
    }

    private void EmitMovieCall(BlLingoHandlerCodeBlock block)
    {
        if (block.Data is not BlLingoMovieCallBlockData data)
        {
            return;
        }

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

    private void EmitExpression(BlLingoHandlerCodeBlock block)
    {
        if (block.Data is not BlLingoExpressionBlockData data)
        {
            return;
        }

        if (string.IsNullOrEmpty(data.Expression))
        {
            return;
        }

        _writer.WriteLine(data.Expression + ";");
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
