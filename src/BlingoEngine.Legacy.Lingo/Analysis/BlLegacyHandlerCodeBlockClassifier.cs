using System;
using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.CodeGen;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.Analysis;

internal static class BlLegacyHandlerCodeBlockClassifier
{
    public static IReadOnlyList<BlLingoHandlerCodeBlock> BuildBlocks(
        IReadOnlyList<BlSyntaxToken> tokens,
        IReadOnlyList<BlSyntaxTrivia> endTrivia,
        BlLingoSymbolTable symbols)
    {
        var result = new List<BlLingoHandlerCodeBlock>();
        var lines = HandlerLine.Split(tokens, endTrivia);
        foreach (var line in lines)
        {
            switch (line.Kind)
            {
                case HandlerLineKind.Blank:
                    result.Add(new BlLingoHandlerCodeBlock(BlLingoHandlerCodeBlockKind.Blank, Array.Empty<BlSyntaxToken>()));
                    break;
                case HandlerLineKind.Comment:
                    result.Add(new BlLingoHandlerCodeBlock(BlLingoHandlerCodeBlockKind.Comment, Array.Empty<BlSyntaxToken>(), commentText: line.CommentText));
                    break;
                case HandlerLineKind.Tokens:
                    result.Add(ClassifyTokens(line.Tokens, symbols));
                    break;
            }
        }

        return result;
    }

    private static BlLingoHandlerCodeBlock ClassifyTokens(IReadOnlyList<BlSyntaxToken> tokens, BlLingoSymbolTable symbols)
    {
        if (tokens.Count == 0)
        {
            return new BlLingoHandlerCodeBlock(BlLingoHandlerCodeBlockKind.Expression, tokens, new BlLingoExpressionBlockData(string.Empty));
        }

        if (BlLegacyHandlerTokenUtilities.IsKeyword(tokens[0], "end"))
        {
            return new BlLingoHandlerCodeBlock(BlLingoHandlerCodeBlockKind.End, tokens);
        }

        if (TryClassifyElse(tokens, out var elseBlock))
        {
            return elseBlock;
        }

        if (TryClassifyRepeat(tokens, out var repeatBlock))
        {
            return repeatBlock;
        }

        if (TryClassifyCase(tokens, out var caseBlock))
        {
            return caseBlock;
        }

        if (TryClassifySendSprite(tokens, symbols, out var sendSpriteBlock))
        {
            return sendSpriteBlock;
        }

        if (TryClassifyMovieCall(tokens, symbols, out var movieCallBlock))
        {
            return movieCallBlock;
        }

        if (TryClassifyPut(tokens, out var putBlock))
        {
            return putBlock;
        }

        if (TryClassifySpriteMemberAssignment(tokens, out var spriteMemberBlock))
        {
            return spriteMemberBlock;
        }

        if (TryClassifyActorListMutation(tokens, out var actorListBlock))
        {
            return actorListBlock;
        }

        if (TryClassifyIf(tokens, out var ifBlock))
        {
            return ifBlock;
        }

        if (TryClassifyExitNext(tokens, out var exitBlock))
        {
            return exitBlock;
        }

        var expression = BlLegacyExpressionConverter.Convert(tokens);
        return new BlLingoHandlerCodeBlock(
            BlLingoHandlerCodeBlockKind.Expression,
            tokens,
            new BlLingoExpressionBlockData(expression));
    }

    private static bool TryClassifyElse(IReadOnlyList<BlSyntaxToken> tokens, out BlLingoHandlerCodeBlock block)
    {
        block = null!;
        if (tokens.Count == 0 || !BlLegacyHandlerTokenUtilities.IsKeyword(tokens[0], "else"))
        {
            return false;
        }

        if (tokens.Count > 1 && BlLegacyHandlerTokenUtilities.IsKeyword(tokens[1], "if"))
        {
            var thenIndex = BlLegacyHandlerTokenUtilities.FindKeyword(tokens, "then");
            if (thenIndex < 0)
            {
                return false;
            }

            var conditionTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 2, thenIndex - 2);
            var condition = BlLegacyExpressionConverter.Convert(conditionTokens);
            block = new BlLingoHandlerCodeBlock(
                BlLingoHandlerCodeBlockKind.ElseIf,
                tokens,
                new BlLingoElseIfBlockData(condition));
            return true;
        }

        block = new BlLingoHandlerCodeBlock(BlLingoHandlerCodeBlockKind.Else, tokens);
        return true;
    }

    private static bool TryClassifyRepeat(IReadOnlyList<BlSyntaxToken> tokens, out BlLingoHandlerCodeBlock block)
    {
        block = null!;
        if (tokens.Count == 0 || !BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[0], "repeat"))
        {
            return false;
        }

        if (tokens.Count >= 4 && BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[1], "with"))
        {
            var variableToken = tokens[2];
            var variableName = BlCSharpName.SanitizeIdentifier(variableToken.ValueText);
            if (string.IsNullOrEmpty(variableName))
            {
                variableName = "item";
            }

            var inIndex = BlLegacyHandlerTokenUtilities.FindKeyword(tokens, "in");
            if (inIndex > 0)
            {
                var sourceTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, inIndex + 1, tokens.Count - (inIndex + 1));
                var source = BlLegacyExpressionConverter.Convert(sourceTokens);
                block = new BlLingoHandlerCodeBlock(
                    BlLingoHandlerCodeBlockKind.RepeatWithEach,
                    tokens,
                    new BlLingoRepeatWithEachBlockData(variableName, source));
                return true;
            }

            var toIndex = BlLegacyHandlerTokenUtilities.FindKeyword(tokens, "to");
            var equalsIndex = BlLegacyHandlerTokenUtilities.FindOperator(tokens, "=");
            if (equalsIndex > 0 && toIndex > equalsIndex)
            {
                var startTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, equalsIndex + 1, toIndex - equalsIndex - 1);
                var endTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, toIndex + 1, tokens.Count - (toIndex + 1));
                var start = BlLegacyExpressionConverter.Convert(startTokens);
                var end = BlLegacyExpressionConverter.Convert(endTokens);
                block = new BlLingoHandlerCodeBlock(
                    BlLingoHandlerCodeBlockKind.RepeatWithRange,
                    tokens,
                    new BlLingoRepeatWithRangeBlockData(variableName, start, end));
                return true;
            }

            return false;
        }

        if (tokens.Count >= 3 && BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[1], "while"))
        {
            var conditionTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 2, tokens.Count - 2);
            var condition = BlLegacyExpressionConverter.Convert(conditionTokens);
            block = new BlLingoHandlerCodeBlock(
                BlLingoHandlerCodeBlockKind.RepeatWhile,
                tokens,
                new BlLingoRepeatWhileBlockData(condition));
            return true;
        }

        if (tokens.Count >= 3 && BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[1], "until"))
        {
            var conditionTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 2, tokens.Count - 2);
            var condition = BlLegacyExpressionConverter.Convert(conditionTokens);
            block = new BlLingoHandlerCodeBlock(
                BlLingoHandlerCodeBlockKind.RepeatUntil,
                tokens,
                new BlLingoRepeatUntilBlockData(condition));
            return true;
        }

        if (tokens.Count >= 2 && BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[1], "forever"))
        {
            block = new BlLingoHandlerCodeBlock(BlLingoHandlerCodeBlockKind.RepeatForever, tokens);
            return true;
        }

        return false;
    }

    private static bool TryClassifyCase(IReadOnlyList<BlSyntaxToken> tokens, out BlLingoHandlerCodeBlock block)
    {
        block = null!;
        if (tokens.Count == 0)
        {
            return false;
        }

        if (BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[0], "case"))
        {
            var ofIndex = BlLegacyHandlerTokenUtilities.FindKeyword(tokens, "of");
            if (ofIndex < 0)
            {
                return false;
            }

            var expressionTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 1, ofIndex - 1);
            var expression = BlLegacyExpressionConverter.Convert(expressionTokens);
            block = new BlLingoHandlerCodeBlock(
                BlLingoHandlerCodeBlockKind.Case,
                tokens,
                new BlLingoCaseBlockData(expression));
            return true;
        }

        if (BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[0], "when"))
        {
            var expressionTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 1, tokens.Count - 1);
            var expression = BlLegacyExpressionConverter.Convert(expressionTokens);
            block = new BlLingoHandlerCodeBlock(
                BlLingoHandlerCodeBlockKind.CaseWhen,
                tokens,
                new BlLingoCaseWhenBlockData(expression));
            return true;
        }

        if (tokens.Count == 1 && BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[0], "otherwise"))
        {
            block = new BlLingoHandlerCodeBlock(BlLingoHandlerCodeBlockKind.CaseOtherwise, tokens);
            return true;
        }

        return false;
    }

    private static bool TryClassifyPut(IReadOnlyList<BlSyntaxToken> tokens, out BlLingoHandlerCodeBlock block)
    {
        block = null!;
        if (tokens.Count == 0 || !BlLegacyHandlerTokenUtilities.IsKeyword(tokens[0], "put"))
        {
            return false;
        }

        var intoIndex = BlLegacyHandlerTokenUtilities.FindKeyword(tokens, "into");
        if (intoIndex < 0)
        {
            return false;
        }

        var valueTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 1, intoIndex - 1);
        var targetTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, intoIndex + 1, tokens.Count - (intoIndex + 1));
        var valueExpression = BlLegacyExpressionConverter.Convert(valueTokens);

        if (targetTokens.Count >= 2 && BlLegacyHandlerTokenUtilities.IsIdentifier(targetTokens[0], "field"))
        {
            var fieldTokens = BlLegacyHandlerTokenUtilities.SliceTokens(targetTokens, 1, targetTokens.Count - 1);
            var fieldNameExpression = BlLegacyExpressionConverter.Convert(fieldTokens);
            block = new BlLingoHandlerCodeBlock(
                BlLingoHandlerCodeBlockKind.Put,
                tokens,
                new BlLingoPutBlockData(
                    BlLingoPutAssignmentKind.Field,
                    valueExpression,
                    FieldName: fieldNameExpression));
            return true;
        }

        if (BlLegacyHandlerTokenUtilities.TryGetSpritePropertyTarget(targetTokens, out var spriteIndex, out var propertyName))
        {
            if (string.Equals(propertyName, "Member", StringComparison.Ordinal))
            {
                if (valueTokens.Count > 1 &&
                    BlLegacyHandlerTokenUtilities.IsIdentifier(valueTokens[0], "member") &&
                    valueTokens[1].Kind == BlSyntaxKind.LeftParenthesisToken)
                {
                    var closeIndex = BlLegacyHandlerTokenUtilities.FindMatchingToken(valueTokens, 1, BlSyntaxKind.LeftParenthesisToken, BlSyntaxKind.RightParenthesisToken);
                    if (closeIndex == valueTokens.Count - 1)
                    {
                        var memberTokens = BlLegacyHandlerTokenUtilities.SliceTokens(valueTokens, 2, closeIndex - 2);
                        var memberArgs = BlLegacyExpressionConverter.Convert(memberTokens);
                        block = new BlLingoHandlerCodeBlock(
                            BlLingoHandlerCodeBlockKind.Put,
                            tokens,
                            new BlLingoPutBlockData(
                                BlLingoPutAssignmentKind.SpriteMember,
                                memberArgs,
                                SpriteIndexExpression: spriteIndex,
                                SpriteMemberArguments: memberArgs));
                        return true;
                    }
                }

                block = new BlLingoHandlerCodeBlock(
                    BlLingoHandlerCodeBlockKind.Put,
                    tokens,
                    new BlLingoPutBlockData(
                        BlLingoPutAssignmentKind.SpriteProperty,
                        valueExpression,
                        SpriteIndexExpression: spriteIndex,
                        SpritePropertyName: propertyName));
                return true;
            }

            block = new BlLingoHandlerCodeBlock(
                BlLingoHandlerCodeBlockKind.Put,
                tokens,
                new BlLingoPutBlockData(
                    BlLingoPutAssignmentKind.SpriteProperty,
                    valueExpression,
                    SpriteIndexExpression: spriteIndex,
                    SpritePropertyName: propertyName));
            return true;
        }

        if (BlLegacyHandlerTokenUtilities.TryGetListTarget(targetTokens, out var listExpression, out var listIndex))
        {
            block = new BlLingoHandlerCodeBlock(
                BlLingoHandlerCodeBlockKind.Put,
                tokens,
                new BlLingoPutBlockData(
                    BlLingoPutAssignmentKind.ListElement,
                    valueExpression,
                    ListExpression: listExpression,
                    ListIndexExpression: listIndex));
            return true;
        }

        var targetExpression = BlLegacyExpressionConverter.Convert(targetTokens);
        if (targetExpression.Length == 0)
        {
            return false;
        }

        block = new BlLingoHandlerCodeBlock(
            BlLingoHandlerCodeBlockKind.Put,
            tokens,
            new BlLingoPutBlockData(
                BlLingoPutAssignmentKind.Direct,
                valueExpression,
                TargetExpression: targetExpression));
        return true;
    }

    private static bool TryClassifySpriteMemberAssignment(IReadOnlyList<BlSyntaxToken> tokens, out BlLingoHandlerCodeBlock block)
    {
        block = null!;
        if (tokens.Count == 0 || !BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[0], "sprite"))
        {
            return false;
        }

        var openIndex = 1;
        if (openIndex >= tokens.Count || tokens[openIndex].Kind != BlSyntaxKind.LeftParenthesisToken)
        {
            return false;
        }

        var closeIndex = BlLegacyHandlerTokenUtilities.FindMatchingToken(tokens, openIndex, BlSyntaxKind.LeftParenthesisToken, BlSyntaxKind.RightParenthesisToken);
        if (closeIndex < 0 || closeIndex + 3 >= tokens.Count)
        {
            return false;
        }

        if (tokens[closeIndex + 1].Kind != BlSyntaxKind.PeriodToken ||
            !BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[closeIndex + 2], "member"))
        {
            return false;
        }

        if (tokens[closeIndex + 3].Kind != BlSyntaxKind.OperatorToken || tokens[closeIndex + 3].ValueText != "=")
        {
            return false;
        }

        var valueIndex = closeIndex + 4;
        if (valueIndex >= tokens.Count ||
            !BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[valueIndex], "member"))
        {
            return false;
        }

        if (valueIndex + 1 >= tokens.Count || tokens[valueIndex + 1].Kind != BlSyntaxKind.LeftParenthesisToken)
        {
            return false;
        }

        var valueClose = BlLegacyHandlerTokenUtilities.FindMatchingToken(tokens, valueIndex + 1, BlSyntaxKind.LeftParenthesisToken, BlSyntaxKind.RightParenthesisToken);
        if (valueClose < 0)
        {
            return false;
        }

        var indexTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, openIndex + 1, closeIndex - (openIndex + 1));
        var memberTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, valueIndex + 2, valueClose - (valueIndex + 2));
        var indexExpression = BlLegacyExpressionConverter.Convert(indexTokens);
        var memberExpression = BlLegacyExpressionConverter.Convert(memberTokens);
        block = new BlLingoHandlerCodeBlock(
            BlLingoHandlerCodeBlockKind.Put,
            tokens,
            new BlLingoPutBlockData(
                BlLingoPutAssignmentKind.SpriteMember,
                memberExpression,
                SpriteIndexExpression: indexExpression,
                SpriteMemberArguments: memberExpression));
        return true;
    }

    private static bool TryClassifyActorListMutation(IReadOnlyList<BlSyntaxToken> tokens, out BlLingoHandlerCodeBlock block)
    {
        block = null!;
        if (tokens.Count < 6)
        {
            return false;
        }

        if (!BlLegacyHandlerTokenUtilities.IsKeyword(tokens[0], "the") ||
            !BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[1], "actorList") ||
            tokens[2].Kind != BlSyntaxKind.PeriodToken)
        {
            return false;
        }

        if (!BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[3], "append") &&
            !BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[3], "deleteOne") &&
            !BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[3], "delete"))
        {
            return false;
        }

        if (tokens[4].Kind != BlSyntaxKind.LeftParenthesisToken)
        {
            return false;
        }

        var closeIndex = BlLegacyHandlerTokenUtilities.FindMatchingToken(tokens, 4, BlSyntaxKind.LeftParenthesisToken, BlSyntaxKind.RightParenthesisToken);
        if (closeIndex < 0)
        {
            return false;
        }

        var argumentTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 5, closeIndex - 5);
        var argument = BlLegacyExpressionConverter.Convert(argumentTokens);

        if (BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[3], "append"))
        {
            block = new BlLingoHandlerCodeBlock(
                BlLingoHandlerCodeBlockKind.ActorListAppend,
                tokens,
                new BlLingoActorListMutationBlockData(argument));
            return true;
        }

        block = new BlLingoHandlerCodeBlock(
            BlLingoHandlerCodeBlockKind.ActorListRemove,
            tokens,
            new BlLingoActorListMutationBlockData(argument));
        return true;
    }

    private static bool TryClassifySendSprite(
        IReadOnlyList<BlSyntaxToken> tokens,
        BlLingoSymbolTable symbols,
        out BlLingoHandlerCodeBlock block)
    {
        block = null!;
        if (tokens.Count == 0)
        {
            return false;
        }

        if (BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[0], "sendSprite"))
        {
            if (!TryParseSendSpriteCall(tokens, symbols, out var data))
            {
                return false;
            }

            block = new BlLingoHandlerCodeBlock(BlLingoHandlerCodeBlockKind.SendSprite, tokens, data);
            return true;
        }

        var equalsIndex = BlLegacyHandlerTokenUtilities.FindOperator(tokens, "=");
        if (equalsIndex > 0 && equalsIndex + 1 < tokens.Count)
        {
            var callTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, equalsIndex + 1, tokens.Count - (equalsIndex + 1));
            if (callTokens.Count > 0 &&
                BlLegacyHandlerTokenUtilities.IsIdentifier(callTokens[0], "sendSprite") &&
                TryParseSendSpriteCall(callTokens, symbols, out var callData))
            {
                var targetTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 0, equalsIndex);
                var targetExpression = BlLegacyExpressionConverter.Convert(targetTokens);
                var resultData = callData with
                {
                    UsesResult = true,
                    ResultTargetExpression = string.IsNullOrEmpty(targetExpression) ? callData.ResultTargetExpression : targetExpression,
                };
                block = new BlLingoHandlerCodeBlock(BlLingoHandlerCodeBlockKind.SendSprite, tokens, resultData);
                return true;
            }
        }

        if (BlLegacyHandlerTokenUtilities.IsKeyword(tokens[0], "put"))
        {
            var intoIndex = BlLegacyHandlerTokenUtilities.FindKeyword(tokens, "into");
            if (intoIndex > 0)
            {
                var valueTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 1, intoIndex - 1);
                if (valueTokens.Count > 0 &&
                    BlLegacyHandlerTokenUtilities.IsIdentifier(valueTokens[0], "sendSprite") &&
                    TryParseSendSpriteCall(valueTokens, symbols, out var callData))
                {
                    var targetTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, intoIndex + 1, tokens.Count - (intoIndex + 1));
                    var targetExpression = BlLegacyExpressionConverter.Convert(targetTokens);
                    var resultData = callData with
                    {
                        UsesResult = true,
                        ResultTargetExpression = string.IsNullOrEmpty(targetExpression) ? callData.ResultTargetExpression : targetExpression,
                    };
                    block = new BlLingoHandlerCodeBlock(BlLingoHandlerCodeBlockKind.SendSprite, tokens, resultData);
                    return true;
                }
            }
        }

        return false;
    }

    private static bool TryParseSendSpriteCall(
        IReadOnlyList<BlSyntaxToken> tokens,
        BlLingoSymbolTable symbols,
        out BlLingoSendSpriteBlockData data)
    {
        data = null!;
        if (tokens.Count == 0 || !BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[0], "sendSprite"))
        {
            return false;
        }

        var commaIndex = BlLegacyHandlerTokenUtilities.FindToken(tokens, BlSyntaxKind.CommaToken);
        if (commaIndex < 0)
        {
            return false;
        }

        var channelTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 1, commaIndex - 1);
        if (commaIndex + 1 >= tokens.Count)
        {
            return false;
        }

        var handlerToken = tokens[commaIndex + 1];
        if (handlerToken.Kind != BlSyntaxKind.SymbolToken)
        {
            return false;
        }

        var channelExpression = BlLegacyExpressionConverter.Convert(channelTokens);
        var handlerName = BlCSharpName.SanitizeIdentifier(handlerToken.ValueText);
        string? scriptName = null;
        var parameterName = "sprite";

        if (channelTokens.Count == 1 &&
            channelTokens[0].Kind == BlSyntaxKind.NumberToken &&
            int.TryParse(channelTokens[0].ValueText, out var channelNumber))
        {
            scriptName = $"B{channelNumber}";
            var sanitized = BlCSharpName.SanitizeIdentifier(scriptName + "Behavior");
            if (!string.IsNullOrEmpty(sanitized))
            {
                parameterName = sanitized.ToLowerInvariant();
            }
            else
            {
                parameterName = $"b{channelNumber}";
            }
        }

        var behaviorName = FindBehaviorForHandler(symbols, handlerName);
        if (!string.IsNullOrEmpty(behaviorName))
        {
            scriptName = behaviorName;
            var sanitized = BlCSharpName.SanitizeIdentifier(behaviorName + "Behavior");
            if (!string.IsNullOrEmpty(sanitized))
            {
                parameterName = sanitized.ToLowerInvariant();
            }
        }

        var argumentTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, commaIndex + 2, tokens.Count - (commaIndex + 2));
        var arguments = ParseArgumentExpressions(argumentTokens);

        data = new BlLingoSendSpriteBlockData(
            channelExpression,
            handlerName,
            parameterName,
            scriptName,
            arguments,
            UsesResult: false);
        return true;
    }

    private static bool TryClassifyIf(IReadOnlyList<BlSyntaxToken> tokens, out BlLingoHandlerCodeBlock block)
    {
        block = null!;
        if (tokens.Count == 0 || !BlLegacyHandlerTokenUtilities.IsKeyword(tokens[0], "if"))
        {
            return false;
        }

        var thenIndex = BlLegacyHandlerTokenUtilities.FindKeyword(tokens, "then");
        if (thenIndex < 0)
        {
            return false;
        }

        var conditionTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 1, thenIndex - 1);
        var condition = BlLegacyExpressionConverter.Convert(conditionTokens);
        block = new BlLingoHandlerCodeBlock(
            BlLingoHandlerCodeBlockKind.If,
            tokens,
            new BlLingoIfBlockData(condition));
        return true;
    }

    private static bool TryClassifyExitNext(IReadOnlyList<BlSyntaxToken> tokens, out BlLingoHandlerCodeBlock block)
    {
        block = null!;
        if (tokens.Count == 0)
        {
            return false;
        }

        if (BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[0], "exit"))
        {
            if (tokens.Count >= 3 &&
                BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[1], "repeat") &&
                BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[2], "if"))
            {
                var conditionTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 3, tokens.Count - 3);
                var condition = BlLegacyExpressionConverter.Convert(conditionTokens);
                block = new BlLingoHandlerCodeBlock(
                    BlLingoHandlerCodeBlockKind.ExitRepeatIf,
                    tokens,
                    new BlLingoExitRepeatIfBlockData(condition));
                return true;
            }

            if (tokens.Count >= 2 && BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[1], "repeat"))
            {
                block = new BlLingoHandlerCodeBlock(BlLingoHandlerCodeBlockKind.ExitRepeat, tokens);
                return true;
            }

            if (tokens.Count == 1)
            {
                block = new BlLingoHandlerCodeBlock(BlLingoHandlerCodeBlockKind.Return, tokens);
                return true;
            }
        }

        if (BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[0], "next") &&
            tokens.Count >= 2 &&
            BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[1], "repeat"))
        {
            if (tokens.Count >= 3 && BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[2], "if"))
            {
                var conditionTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 3, tokens.Count - 3);
                var condition = BlLegacyExpressionConverter.Convert(conditionTokens);
                block = new BlLingoHandlerCodeBlock(
                    BlLingoHandlerCodeBlockKind.NextRepeatIf,
                    tokens,
                    new BlLingoExitRepeatIfBlockData(condition));
                return true;
            }

            block = new BlLingoHandlerCodeBlock(BlLingoHandlerCodeBlockKind.NextRepeat, tokens);
            return true;
        }

        return false;
    }

    private static bool TryClassifyMovieCall(
        IReadOnlyList<BlSyntaxToken> tokens,
        BlLingoSymbolTable symbols,
        out BlLingoHandlerCodeBlock block)
    {
        block = null!;
        if (tokens.Count == 0)
        {
            return false;
        }

        if (TryParseMovieCallExpression(tokens, symbols, out var data))
        {
            block = new BlLingoHandlerCodeBlock(BlLingoHandlerCodeBlockKind.MovieCall, tokens, data);
            return true;
        }

        var equalsIndex = BlLegacyHandlerTokenUtilities.FindOperator(tokens, "=");
        if (equalsIndex > 0 && equalsIndex + 1 < tokens.Count)
        {
            var callTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, equalsIndex + 1, tokens.Count - (equalsIndex + 1));
            if (TryParseMovieCallExpression(callTokens, symbols, out var callData))
            {
                var targetTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 0, equalsIndex);
                var targetExpression = BlLegacyExpressionConverter.Convert(targetTokens);
                var resultData = callData with
                {
                    UsesResult = true,
                    ResultTargetExpression = string.IsNullOrEmpty(targetExpression) ? callData.ResultTargetExpression : targetExpression,
                };
                block = new BlLingoHandlerCodeBlock(BlLingoHandlerCodeBlockKind.MovieCall, tokens, resultData);
                return true;
            }
        }

        if (BlLegacyHandlerTokenUtilities.IsKeyword(tokens[0], "put"))
        {
            var intoIndex = BlLegacyHandlerTokenUtilities.FindKeyword(tokens, "into");
            if (intoIndex > 0)
            {
                var valueTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 1, intoIndex - 1);
                if (TryParseMovieCallExpression(valueTokens, symbols, out var callData))
                {
                    var targetTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, intoIndex + 1, tokens.Count - (intoIndex + 1));
                    var targetExpression = BlLegacyExpressionConverter.Convert(targetTokens);
                    var resultData = callData with
                    {
                        UsesResult = true,
                        ResultTargetExpression = string.IsNullOrEmpty(targetExpression) ? callData.ResultTargetExpression : targetExpression,
                    };
                    block = new BlLingoHandlerCodeBlock(BlLingoHandlerCodeBlockKind.MovieCall, tokens, resultData);
                    return true;
                }
            }
        }

        return false;
    }

    private static bool TryParseMovieCallExpression(
        IReadOnlyList<BlSyntaxToken> tokens,
        BlLingoSymbolTable symbols,
        out BlLingoMovieCallBlockData data)
    {
        data = null!;
        if (tokens.Count == 0)
        {
            return false;
        }

        var handlerToken = tokens[0];
        if (handlerToken.Kind != BlSyntaxKind.IdentifierToken && handlerToken.Kind != BlSyntaxKind.SymbolToken)
        {
            return false;
        }

        var handlerName = BlCSharpName.SanitizeIdentifier(handlerToken.ValueText);
        var parameterName = "movie";
        var arguments = new List<string>();

        if (tokens.Count == 1)
        {
            // no arguments
        }
        else if (tokens.Count >= 2 && tokens[1].Kind == BlSyntaxKind.LeftParenthesisToken)
        {
            var closeIndex = BlLegacyHandlerTokenUtilities.FindMatchingToken(tokens, 1, BlSyntaxKind.LeftParenthesisToken, BlSyntaxKind.RightParenthesisToken);
            if (closeIndex != tokens.Count - 1)
            {
                return false;
            }

            var argumentTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 2, closeIndex - 2);
            arguments = ParseArgumentExpressions(argumentTokens);
        }
        else
        {
            return false;
        }

        var targetScript = FindMovieHandlerScope(symbols, handlerName);
        if (string.IsNullOrEmpty(targetScript) &&
            !handlerName.EndsWith("Handler", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        data = new BlLingoMovieCallBlockData(handlerName, targetScript, UsesResult: false, arguments, parameterName);
        return true;
    }

    private static List<string> ParseArgumentExpressions(IReadOnlyList<BlSyntaxToken> tokens)
    {
        var arguments = new List<string>();
        if (tokens.Count == 0)
        {
            return arguments;
        }

        var segments = BlLegacyHandlerTokenUtilities.SplitByComma(tokens);
        foreach (var segment in segments)
        {
            var expression = BlLegacyExpressionConverter.Convert(segment);
            if (expression.Length > 0)
            {
                arguments.Add(expression);
            }
        }

        return arguments;
    }

    private static string? FindBehaviorForHandler(BlLingoSymbolTable symbols, string handlerName)
    {
        if (string.IsNullOrEmpty(handlerName))
        {
            return null;
        }

        foreach (var scope in symbols.ClassScopes.Values)
        {
            if (scope is null || scope.IsMovieScript)
            {
                continue;
            }

            if (scope.Handlers.ContainsKey(handlerName))
            {
                return scope.Symbol.Name;
            }
        }

        return null;
    }

    private static string? FindMovieHandlerScope(BlLingoSymbolTable symbols, string handlerName)
    {
        if (string.IsNullOrEmpty(handlerName))
        {
            return null;
        }

        if (symbols.MovieScript.Handlers.ContainsKey(handlerName))
        {
            return symbols.MovieScript.Symbol.Name;
        }

        foreach (var scope in symbols.ClassScopes.Values)
        {
            if (scope.IsMovieScript && scope.Handlers.ContainsKey(handlerName))
            {
                return scope.Symbol.Name;
            }
        }

        return null;
    }
}
