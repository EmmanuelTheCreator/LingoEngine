using System;
using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.CodeGen;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.Analysis.Blocks;

internal sealed class PutBlockClassifier : IHandlerBlockClassifier
{
    public static PutBlockClassifier Instance { get; } = new();

    private PutBlockClassifier()
    {
    }

    public bool TryCreate(IReadOnlyList<BlSyntaxToken> tokens, BlLingoSymbolTable symbols, out BlLingoHandlerCodeBlock block)
    {
        block = null!;
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
        var valueExpression = BlLegacyExpressionConverter.Convert(valueTokens);

        if (targetTokens.Count >= 2 && BlLegacyHandlerTokenUtilities.IsIdentifier(targetTokens[0], "field"))
        {
            var fieldTokens = BlLegacyHandlerTokenUtilities.SliceTokens(targetTokens, 1, targetTokens.Count - 1);
            var fieldNameExpression = BlLegacyExpressionConverter.Convert(fieldTokens);
            block = new BlLingoHandlerCodeBlock(
                BlLingoHandlerCodeBlockKind.Put,
                tokens,
                new BlLingoPutBlockData(
                    BlLingoPutAssignmentKind.Field,
                    valueExpression,
                    FieldName: fieldNameExpression));
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
                        var memberArgs = BlLegacyExpressionConverter.Convert(memberTokens);
                        block = new BlLingoHandlerCodeBlock(
                            BlLingoHandlerCodeBlockKind.Put,
                            tokens,
                            new BlLingoPutBlockData(
                                BlLingoPutAssignmentKind.SpriteMember,
                                memberArgs,
                                SpriteIndexExpression: spriteIndex,
                                SpriteMemberArguments: memberArgs));
                        return true;
                    }
                }

                block = new BlLingoHandlerCodeBlock(
                    BlLingoHandlerCodeBlockKind.Put,
                    tokens,
                    new BlLingoPutBlockData(
                        BlLingoPutAssignmentKind.SpriteProperty,
                        valueExpression,
                        SpriteIndexExpression: spriteIndex,
                        SpritePropertyName: propertyName));
                return true;
            }

            block = new BlLingoHandlerCodeBlock(
                BlLingoHandlerCodeBlockKind.Put,
                tokens,
                new BlLingoPutBlockData(
                    BlLingoPutAssignmentKind.SpriteProperty,
                    valueExpression,
                    SpriteIndexExpression: spriteIndex,
                    SpritePropertyName: propertyName));
            return true;
        }

        if (BlLegacyHandlerTokenUtilities.TryGetListTarget(targetTokens, out var listExpression, out var listIndex))
        {
            block = new BlLingoHandlerCodeBlock(
                BlLingoHandlerCodeBlockKind.Put,
                tokens,
                new BlLingoPutBlockData(
                    BlLingoPutAssignmentKind.ListElement,
                    valueExpression,
                    ListExpression: listExpression,
                    ListIndexExpression: listIndex));
            return true;
        }

        var targetExpression = BlLegacyExpressionConverter.Convert(targetTokens);
        if (targetExpression.Length == 0)
        {
            return false;
        }

        block = new BlLingoHandlerCodeBlock(
            BlLingoHandlerCodeBlockKind.Put,
            tokens,
            new BlLingoPutBlockData(
                BlLingoPutAssignmentKind.Direct,
                valueExpression,
                TargetExpression: targetExpression));
        return true;
    }
}
