using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.CodeGen;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.Analysis.Blocks;

internal sealed class ElseBlockClassifier : IHandlerBlockClassifier
{
    public static ElseBlockClassifier Instance { get; } = new();

    private ElseBlockClassifier()
    {
    }

    public bool TryCreate(IReadOnlyList<BlSyntaxToken> tokens, BlLingoSymbolTable symbols, out BlLingoHandlerCodeBlock block)
    {
        block = null!;
        if (tokens.Count == 0 || !BlLegacyHandlerTokenUtilities.IsKeyword(tokens[0], "else"))
        {
            return false;
        }

        if (tokens.Count > 1 && BlLegacyHandlerTokenUtilities.IsKeyword(tokens[1], "if"))
        {
            var thenIndex = BlLegacyHandlerTokenUtilities.FindKeyword(tokens, "then");
            if (thenIndex < 0)
            {
                return false;
            }

            var conditionTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 2, thenIndex - 2);
            var condition = BlLegacyExpressionConverter.Convert(conditionTokens);
            block = new BlLingoHandlerCodeBlock(
                BlLingoHandlerCodeBlockKind.ElseIf,
                tokens,
                new BlLingoElseIfBlockData(condition));
            return true;
        }

        block = new BlLingoHandlerCodeBlock(BlLingoHandlerCodeBlockKind.Else, tokens);
        return true;
    }
}
