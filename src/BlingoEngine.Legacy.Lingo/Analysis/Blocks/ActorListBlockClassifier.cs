using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.CodeGen;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.Analysis.Blocks;

internal sealed class ActorListBlockClassifier : IHandlerBlockClassifier
{
    public static ActorListBlockClassifier Instance { get; } = new();

    private ActorListBlockClassifier()
    {
    }

    public bool TryCreate(IReadOnlyList<BlSyntaxToken> tokens, BlLingoSymbolTable symbols, out BlLingoHandlerCodeBlock block)
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

        var isRemoval = BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[3], "deleteOne") ||
            BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[3], "delete");

        block = new BlLingoHandlerCodeBlock(
            isRemoval ? BlLingoHandlerCodeBlockKind.ActorListRemove : BlLingoHandlerCodeBlockKind.ActorListAppend,
            tokens,
            new BlLingoActorListMutationBlockData(argument, IsRemoval: isRemoval));
        return true;
    }
}
