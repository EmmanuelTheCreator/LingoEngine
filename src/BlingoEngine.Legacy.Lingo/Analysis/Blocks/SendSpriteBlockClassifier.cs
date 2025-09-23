using System;
using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.CodeGen;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.Analysis.Blocks;

internal sealed class SendSpriteBlockClassifier : IHandlerBlockClassifier
{
    public static SendSpriteBlockClassifier Instance { get; } = new();

    private SendSpriteBlockClassifier()
    {
    }

    public bool TryCreate(IReadOnlyList<BlSyntaxToken> tokens, BlLingoSymbolTable symbols, out BlLingoHandlerCodeBlock block)
    {
        block = null!;
        if (tokens.Count == 0)
        {
            return false;
        }

        if (BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[0], "sendSprite"))
        {
            if (!TryParseCall(tokens, symbols, out var data))
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
                TryParseCall(callTokens, symbols, out var callData))
            {
                var targetTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 0, equalsIndex);
                var targetExpression = BlLegacyExpressionConverter.Convert(targetTokens);
                block = new BlLingoHandlerCodeBlock(
                    BlLingoHandlerCodeBlockKind.SendSprite,
                    tokens,
                    callData with
                    {
                        UsesResult = true,
                        ResultTargetExpression = string.IsNullOrEmpty(targetExpression)
                            ? callData.ResultTargetExpression
                            : targetExpression,
                    });
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
                    TryParseCall(valueTokens, symbols, out var callData))
                {
                    var targetTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, intoIndex + 1, tokens.Count - (intoIndex + 1));
                    var targetExpression = BlLegacyExpressionConverter.Convert(targetTokens);
                    block = new BlLingoHandlerCodeBlock(
                        BlLingoHandlerCodeBlockKind.SendSprite,
                        tokens,
                        callData with
                        {
                            UsesResult = true,
                            ResultTargetExpression = string.IsNullOrEmpty(targetExpression)
                                ? callData.ResultTargetExpression
                                : targetExpression,
                        });
                    return true;
                }
            }
        }

        return false;
    }

    private static bool TryParseCall(
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
        if (commaIndex < 0 || commaIndex + 1 >= tokens.Count)
        {
            return false;
        }

        var channelTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 1, commaIndex - 1);
        var handlerToken = tokens[commaIndex + 1];
        if (handlerToken.Kind != BlSyntaxKind.SymbolToken)
        {
            return false;
        }

        var channelExpression = BlLegacyExpressionConverter.Convert(channelTokens);
        var handlerName = BlCSharpName.SanitizeIdentifier(handlerToken.ValueText);
        if (string.IsNullOrEmpty(handlerName))
        {
            return false;
        }

        var parameterName = "sprite";
        string? scriptName = null;

        if (channelTokens.Count == 1 &&
            channelTokens[0].Kind == BlSyntaxKind.NumberToken &&
            int.TryParse(channelTokens[0].ValueText, out var channelNumber))
        {
            scriptName = $"B{channelNumber}";
            parameterName = DeriveParameterName(scriptName);
        }

        var behaviorName = HandlerBlockClassifierUtilities.FindBehaviorForHandler(symbols, handlerName);
        if (!string.IsNullOrEmpty(behaviorName))
        {
            scriptName = behaviorName;
            parameterName = DeriveParameterName(behaviorName);
        }

        var argumentTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, commaIndex + 2, tokens.Count - (commaIndex + 2));
        var arguments = HandlerBlockClassifierUtilities.ParseArgumentExpressions(argumentTokens);

        data = new BlLingoSendSpriteBlockData(
            channelExpression,
            handlerName,
            parameterName,
            scriptName,
            arguments,
            UsesResult: false);
        return true;
    }

    private static string DeriveParameterName(string scriptName)
    {
        if (string.IsNullOrEmpty(scriptName))
        {
            return "sprite";
        }

        var sanitized = BlCSharpName.SanitizeIdentifier(scriptName + "Behavior");
        if (!string.IsNullOrEmpty(sanitized))
        {
            return sanitized.ToLowerInvariant();
        }

        return scriptName.ToLowerInvariant();
    }
}
