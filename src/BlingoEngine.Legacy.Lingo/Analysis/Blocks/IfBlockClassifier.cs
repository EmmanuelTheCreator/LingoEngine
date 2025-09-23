using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.CodeGen;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.Analysis.Blocks;

internal sealed class IfBlockClassifier : IHandlerBlockClassifier
{
    public static IfBlockClassifier Instance { get; } = new();

    private IfBlockClassifier()
    {
    }

    public bool TryCreate(IReadOnlyList<BlSyntaxToken> tokens, BlLingoSymbolTable symbols, out BlLingoHandlerCodeBlock block)
    {
        block = null!;
        if (tokens.Count == 0 || !BlLegacyHandlerTokenUtilities.IsKeyword(tokens[0], "if"))
        {
            return false;
        }

        var thenIndex = BlLegacyHandlerTokenUtilities.FindKeyword(tokens, "then");
        if (thenIndex < 0)
        {
            return false;
        }

        var conditionTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 1, thenIndex - 1);
        var condition = BlLegacyExpressionConverter.Convert(conditionTokens);
        block = new BlLingoHandlerCodeBlock(
            BlLingoHandlerCodeBlockKind.If,
            tokens,
            new BlLingoIfBlockData(condition));
        return true;
    }
}
