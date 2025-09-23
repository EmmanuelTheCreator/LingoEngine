using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.CodeGen;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.Analysis.Blocks;

internal sealed class ExpressionBlockClassifier : IHandlerBlockClassifier
{
    public static ExpressionBlockClassifier Instance { get; } = new();

    private ExpressionBlockClassifier()
    {
    }

    public bool TryCreate(IReadOnlyList<BlSyntaxToken> tokens, BlLingoSymbolTable symbols, out BlLingoHandlerCodeBlock block)
    {
        var expression = BlLegacyExpressionConverter.Convert(tokens);
        block = new BlLingoHandlerCodeBlock(
            BlLingoHandlerCodeBlockKind.Expression,
            tokens,
            new BlLingoExpressionBlockData(expression));
        return true;
    }
}
