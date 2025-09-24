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

        var operationToken = tokens[3];
        if (!BlLegacyHandlerTokenUtilities.IsIdentifier(operationToken, "append") &&
            !BlLegacyHandlerTokenUtilities.IsIdentifier(operationToken, "deleteOne") &&
            !BlLegacyHandlerTokenUtilities.IsIdentifier(operationToken, "delete"))
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

        var kind = BlLingoActorListMutationKind.Append;
        if (BlLegacyHandlerTokenUtilities.IsIdentifier(operationToken, "deleteOne"))
        {
            kind = BlLingoActorListMutationKind.DeleteOne;
        }
        else if (BlLegacyHandlerTokenUtilities.IsIdentifier(operationToken, "delete"))
        {
            kind = BlLingoActorListMutationKind.Delete;
        }

        block = new BlLingoHandlerCodeBlock(
            kind == BlLingoActorListMutationKind.Append ? BlLingoHandlerCodeBlockKind.ActorListAppend : BlLingoHandlerCodeBlockKind.ActorListRemove,
            tokens,
            new BlLingoActorListMutationBlockData(argument, kind));
        return true;
    }
}
