using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.CodeGen;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.Analysis.Blocks;

internal sealed class RepeatBlockClassifier : IHandlerBlockClassifier
{
    public static RepeatBlockClassifier Instance { get; } = new();

    private RepeatBlockClassifier()
    {
    }

    public bool TryCreate(IReadOnlyList<BlSyntaxToken> tokens, BlLingoSymbolTable symbols, out BlLingoHandlerCodeBlock block)
    {
        block = null!;
        if (tokens.Count == 0 || !BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[0], "repeat"))
        {
            return false;
        }

        if (tokens.Count >= 4 && BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[1], "with"))
        {
            var variableToken = tokens[2];
            var variableName = BlCSharpName.SanitizeIdentifier(variableToken.ValueText);
            if (string.IsNullOrEmpty(variableName))
            {
                variableName = "item";
            }

            var inIndex = BlLegacyHandlerTokenUtilities.FindKeyword(tokens, "in");
            if (inIndex > 0)
            {
                var sourceTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, inIndex + 1, tokens.Count - (inIndex + 1));
                var source = BlLegacyExpressionConverter.Convert(sourceTokens);
                block = new BlLingoHandlerCodeBlock(
                    BlLingoHandlerCodeBlockKind.RepeatWithEach,
                    tokens,
                    new BlLingoRepeatWithEachBlockData(variableName, source));
                return true;
            }

            var toIndex = BlLegacyHandlerTokenUtilities.FindKeyword(tokens, "to");
            var equalsIndex = BlLegacyHandlerTokenUtilities.FindOperator(tokens, "=");
            if (equalsIndex > 0 && toIndex > equalsIndex)
            {
                var startTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, equalsIndex + 1, toIndex - equalsIndex - 1);
                var endTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, toIndex + 1, tokens.Count - (toIndex + 1));
                var start = BlLegacyExpressionConverter.Convert(startTokens);
                var end = BlLegacyExpressionConverter.Convert(endTokens);
                block = new BlLingoHandlerCodeBlock(
                    BlLingoHandlerCodeBlockKind.RepeatWithRange,
                    tokens,
                    new BlLingoRepeatWithRangeBlockData(variableName, start, end));
                return true;
            }

            return false;
        }

        if (tokens.Count >= 3 && BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[1], "while"))
        {
            var conditionTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 2, tokens.Count - 2);
            var condition = BlLegacyExpressionConverter.Convert(conditionTokens);
            block = new BlLingoHandlerCodeBlock(
                BlLingoHandlerCodeBlockKind.RepeatWhile,
                tokens,
                new BlLingoRepeatWhileBlockData(condition));
            return true;
        }

        if (tokens.Count >= 3 && BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[1], "until"))
        {
            var conditionTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 2, tokens.Count - 2);
            var condition = BlLegacyExpressionConverter.Convert(conditionTokens);
            block = new BlLingoHandlerCodeBlock(
                BlLingoHandlerCodeBlockKind.RepeatUntil,
                tokens,
                new BlLingoRepeatUntilBlockData(condition));
            return true;
        }

        if (tokens.Count >= 2 && BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[1], "forever"))
        {
            block = new BlLingoHandlerCodeBlock(BlLingoHandlerCodeBlockKind.RepeatForever, tokens);
            return true;
        }

        return false;
    }
}
