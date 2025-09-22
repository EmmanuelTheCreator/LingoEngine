using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.CodeGen;

public sealed class ActorListRemoveVisitor : IHandlerTokenVisitor
{
    public bool TryHandle(HandlerTokenEmitContext context)
    {
        var tokens = context.Tokens;
        if (tokens.Count < 6)
        {
            return false;
        }

        if (!BlLegacyHandlerTokenUtilities.IsKeyword(tokens[0], "the") ||
            !BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[1], "actorList"))
        {
            return false;
        }

        if (tokens[2].Kind != BlSyntaxKind.PeriodToken)
        {
            return false;
        }

        if (!(BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[3], "deleteOne") ||
              BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[3], "delete")))
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
        var argument = context.ConvertExpression(argumentTokens);
        context.Writer.WriteLine($"_Movie.ActorList.Remove({argument});");
        return true;
    }
}
