using System;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.CodeGen;

public sealed class PutTokenVisitor : IHandlerTokenVisitor
{
    public bool TryHandle(HandlerTokenEmitContext context)
    {
        var tokens = context.Tokens;
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
        var valueExpression = context.ConvertExpression(valueTokens);

        if (targetTokens.Count >= 2 && BlLegacyHandlerTokenUtilities.IsIdentifier(targetTokens[0], "field"))
        {
            var fieldTokens = BlLegacyHandlerTokenUtilities.SliceTokens(targetTokens, 1, targetTokens.Count - 1);
            var fieldName = context.ConvertExpression(fieldTokens);
            context.Writer.WriteLine($"PutTextIntoField({fieldName}, {valueExpression});");
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
                        var memberArgs = context.ConvertExpression(memberTokens);
                        context.Writer.WriteLine($"Sprite({spriteIndex}).SetMember({memberArgs});");
                        return true;
                    }
                }

                context.Writer.WriteLine($"Sprite({spriteIndex}).Member = {valueExpression};");
                return true;
            }

            context.Writer.WriteLine($"Sprite({spriteIndex}).{propertyName} = {valueExpression};");
            return true;
        }

        if (BlLegacyHandlerTokenUtilities.TryGetListTarget(targetTokens, out var listExpression, out var listIndex))
        {
            context.Writer.WriteLine($"{listExpression}.SetAt({listIndex}, {valueExpression});");
            return true;
        }

        var targetExpression = context.ConvertExpression(targetTokens);
        if (targetExpression.Length == 0)
        {
            return false;
        }

        context.Writer.WriteLine($"{targetExpression} = {valueExpression};");
        return true;
    }
}
