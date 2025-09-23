using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.Analysis;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.CodeGen;

public sealed class SendSpriteVisitor : IHandlerTokenVisitor
{
    public bool TryHandle(HandlerTokenEmitContext context)
    {
        var tokens = context.Tokens;
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

        var channelExpression = context.ConvertExpression(channelTokens);
        var handlerName = BlCSharpName.SanitizeIdentifier(handlerToken.ValueText);
        string? typeName = null;
        string parameterName;

        if (channelTokens.Count == 1 && tokens[1].Kind == BlSyntaxKind.NumberToken &&
            int.TryParse(tokens[1].ValueText, out var channelNumber))
        {
            var scriptName = $"B{channelNumber}";
            var composed = context.ComposeClassName(scriptName, BlLingoScriptKind.Behavior);
            if (!string.IsNullOrEmpty(composed))
            {
                typeName = composed;
                var sanitized = BlCSharpName.SanitizeIdentifier(composed);
                parameterName = string.IsNullOrEmpty(sanitized)
                    ? $"b{channelNumber}"
                    : sanitized.ToLowerInvariant();
            }
            else
            {
                parameterName = $"b{channelNumber}";
            }
        }
        else
        {
            parameterName = "sprite";
        }

        var argumentTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, commaIndex + 2, tokens.Count - (commaIndex + 2));
        List<string>? arguments = null;
        if (argumentTokens.Count > 0)
        {
            var segments = BlLegacyHandlerTokenUtilities.SplitByComma(argumentTokens);
            foreach (var segment in segments)
            {
                var expression = context.ConvertExpression(segment);
                if (expression.Length == 0)
                {
                    continue;
                }

                arguments ??= new List<string>(segments.Count);
                arguments.Add(expression);
            }
        }

        var lambdaCall = arguments is { Count: > 0 }
            ? $"{parameterName}.{handlerName}({string.Join(", ", arguments)})"
            : $"{parameterName}.{handlerName}()";
        if (typeName is null)
        {
            context.Writer.WriteLine($"SendSprite({channelExpression}, {parameterName} => {lambdaCall});");
        }
        else
        {
            context.Writer.WriteLine($"SendSprite<{typeName}>({channelExpression}, {parameterName} => {lambdaCall});");
        }

        return true;
    }
}
