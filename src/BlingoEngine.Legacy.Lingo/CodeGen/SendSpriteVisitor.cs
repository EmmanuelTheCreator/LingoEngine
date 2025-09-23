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
            typeName = $"B{channelNumber}";
            parameterName = $"b{channelNumber}";
        }
        else
        {
            parameterName = "sprite";
        }

        var lambdaCall = $"{parameterName}.{handlerName}()";
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
