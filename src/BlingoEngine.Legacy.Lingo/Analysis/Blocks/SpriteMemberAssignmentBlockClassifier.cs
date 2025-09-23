using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.CodeGen;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.Analysis.Blocks;

internal sealed class SpriteMemberAssignmentBlockClassifier : IHandlerBlockClassifier
{
    public static SpriteMemberAssignmentBlockClassifier Instance { get; } = new();

    private SpriteMemberAssignmentBlockClassifier()
    {
    }

    public bool TryCreate(IReadOnlyList<BlSyntaxToken> tokens, BlLingoSymbolTable symbols, out BlLingoHandlerCodeBlock block)
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
}
