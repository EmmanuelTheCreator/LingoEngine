using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.CodeGen;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.Analysis.Blocks;

internal sealed class CaseBlockClassifier : IHandlerBlockClassifier
{
    public static CaseBlockClassifier Instance { get; } = new();

    private CaseBlockClassifier()
    {
    }

    public bool TryCreate(IReadOnlyList<BlSyntaxToken> tokens, BlLingoSymbolTable symbols, out BlLingoHandlerCodeBlock block)
    {
        block = null!;
        if (tokens.Count == 0)
        {
            return false;
        }

        if (BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[0], "case"))
        {
            var ofIndex = BlLegacyHandlerTokenUtilities.FindKeyword(tokens, "of");
            if (ofIndex < 0)
            {
                return false;
            }

            var expressionTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 1, ofIndex - 1);
            var expression = BlLegacyExpressionConverter.Convert(expressionTokens);
            block = new BlLingoHandlerCodeBlock(
                BlLingoHandlerCodeBlockKind.Case,
                tokens,
                new BlLingoCaseBlockData(expression));
            return true;
        }

        if (BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[0], "when"))
        {
            var expressionTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 1, tokens.Count - 1);
            var expression = BlLegacyExpressionConverter.Convert(expressionTokens);
            block = new BlLingoHandlerCodeBlock(
                BlLingoHandlerCodeBlockKind.CaseWhen,
                tokens,
                new BlLingoCaseWhenBlockData(expression));
            return true;
        }

        if (tokens.Count == 1 && BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[0], "otherwise"))
        {
            block = new BlLingoHandlerCodeBlock(BlLingoHandlerCodeBlockKind.CaseOtherwise, tokens);
            return true;
        }

        return false;
    }
}
