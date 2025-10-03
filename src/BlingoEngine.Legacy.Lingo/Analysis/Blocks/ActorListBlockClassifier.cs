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

        if (!TryParseMemberInvocation(tokens, out var argumentExpression, out var kind) &&
            !TryParseCommandInvocation(tokens, out argumentExpression, out kind))
        {
            return false;
        }

        block = new BlLingoHandlerCodeBlock(
            kind == BlLingoActorListMutationKind.Append ? BlLingoHandlerCodeBlockKind.ActorListAppend : BlLingoHandlerCodeBlockKind.ActorListRemove,
            tokens,
            new BlLingoActorListMutationBlockData(argumentExpression, kind));
        return true;
    }

    private static bool TryParseMemberInvocation(
        IReadOnlyList<BlSyntaxToken> tokens,
        out string argumentExpression,
        out BlLingoActorListMutationKind kind)
    {
        argumentExpression = string.Empty;
        kind = BlLingoActorListMutationKind.Append;

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
        if (!TryGetMutationKind(operationToken, out kind))
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
        argumentExpression = BlLegacyExpressionConverter.Convert(argumentTokens);
        return argumentExpression.Length > 0;
    }

    private static bool TryParseCommandInvocation(
        IReadOnlyList<BlSyntaxToken> tokens,
        out string argumentExpression,
        out BlLingoActorListMutationKind kind)
    {
        argumentExpression = string.Empty;
        kind = BlLingoActorListMutationKind.Append;

        if (tokens.Count < 5)
        {
            return false;
        }

        var operationToken = tokens[0];
        if (!TryGetMutationKind(operationToken, out kind))
        {
            return false;
        }

        if (!BlLegacyHandlerTokenUtilities.IsKeyword(tokens[1], "the") ||
            !BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[2], "actorList") ||
            tokens[3].Kind != BlSyntaxKind.LeftParenthesisToken)
        {
            return false;
        }

        var closeIndex = BlLegacyHandlerTokenUtilities.FindMatchingToken(tokens, 3, BlSyntaxKind.LeftParenthesisToken, BlSyntaxKind.RightParenthesisToken);
        if (closeIndex < 0)
        {
            return false;
        }

        if (closeIndex + 1 != tokens.Count)
        {
            return false;
        }

        var argumentTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 4, closeIndex - 4);
        argumentExpression = BlLegacyExpressionConverter.Convert(argumentTokens);
        return argumentExpression.Length > 0;
    }

    private static bool TryGetMutationKind(BlSyntaxToken operationToken, out BlLingoActorListMutationKind kind)
    {
        kind = BlLingoActorListMutationKind.Append;

        if (BlLegacyHandlerTokenUtilities.IsIdentifier(operationToken, "append"))
        {
            kind = BlLingoActorListMutationKind.Append;
            return true;
        }

        if (BlLegacyHandlerTokenUtilities.IsIdentifier(operationToken, "deleteOne"))
        {
            kind = BlLingoActorListMutationKind.DeleteOne;
            return true;
        }

        if (BlLegacyHandlerTokenUtilities.IsIdentifier(operationToken, "delete"))
        {
            kind = BlLingoActorListMutationKind.Delete;
            return true;
        }

        return false;
    }
}
