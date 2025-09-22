using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.CodeGen;

public sealed class SpriteMemberAssignmentVisitor : IHandlerTokenVisitor
{
    public bool TryHandle(HandlerTokenEmitContext context)
    {
        var tokens = context.Tokens;
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

        if (tokens[closeIndex + 3].Kind != BlSyntaxKind.OperatorToken ||
            tokens[closeIndex + 3].ValueText != "=")
        {
            return false;
        }

        var valueIndex = closeIndex + 4;
        if (valueIndex >= tokens.Count ||
            !BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[valueIndex], "member"))
        {
            return false;
        }

        if (valueIndex + 1 >= tokens.Count ||
            tokens[valueIndex + 1].Kind != BlSyntaxKind.LeftParenthesisToken)
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
        var indexExpression = context.ConvertExpression(indexTokens);
        var memberExpression = context.ConvertExpression(memberTokens);
        context.Writer.WriteLine($"Sprite({indexExpression}).SetMember({memberExpression});");
        return true;
    }
}
