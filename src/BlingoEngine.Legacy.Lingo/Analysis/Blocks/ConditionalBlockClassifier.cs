using System;
using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.CodeGen;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.Analysis.Blocks;

/// <summary>
/// Classifies conditional handler blocks (if/else if/else) so that all conditional
/// parsing logic lives in a single place.
/// </summary>
internal sealed class ConditionalBlockClassifier : IHandlerBlockClassifier
{
    public static ConditionalBlockClassifier Instance { get; } = new();

    private ConditionalBlockClassifier()
    {
    }

    public bool TryCreate(IReadOnlyList<BlSyntaxToken> tokens, BlLingoSymbolTable symbols, out BlLingoHandlerCodeBlock block)
    {
        block = null!;
        if (tokens.Count == 0)
        {
            return false;
        }

        if (BlLegacyHandlerTokenUtilities.IsKeyword(tokens[0], "if"))
        {
            return TryCreateConditionalBlock(
                tokens,
                conditionStartIndex: 1,
                condition => new BlLingoHandlerCodeBlock(
                    BlLingoHandlerCodeBlockKind.If,
                    tokens,
                    new BlLingoIfBlockData(condition)),
                out block);
        }

        if (BlLegacyHandlerTokenUtilities.IsKeyword(tokens[0], "else"))
        {
            if (tokens.Count > 1 && BlLegacyHandlerTokenUtilities.IsKeyword(tokens[1], "if"))
            {
                return TryCreateConditionalBlock(
                    tokens,
                    conditionStartIndex: 2,
                    condition => new BlLingoHandlerCodeBlock(
                        BlLingoHandlerCodeBlockKind.ElseIf,
                        tokens,
                        new BlLingoElseIfBlockData(condition)),
                    out block);
            }

            block = new BlLingoHandlerCodeBlock(BlLingoHandlerCodeBlockKind.Else, tokens);
            return true;
        }

        return false;
    }

    private static bool TryCreateConditionalBlock(
        IReadOnlyList<BlSyntaxToken> tokens,
        int conditionStartIndex,
        Func<string, BlLingoHandlerCodeBlock> blockFactory,
        out BlLingoHandlerCodeBlock block)
    {
        block = null!;
        ArgumentNullException.ThrowIfNull(blockFactory);
        var thenIndex = BlLegacyHandlerTokenUtilities.FindKeyword(tokens, "then");
        if (thenIndex < 0 || thenIndex < conditionStartIndex)
        {
            return false;
        }

        var conditionTokens = BlLegacyHandlerTokenUtilities.SliceTokens(
            tokens,
            conditionStartIndex,
            thenIndex - conditionStartIndex);
        var condition = BlLegacyExpressionConverter.Convert(conditionTokens);
        block = blockFactory(condition);
        return true;
    }
}
