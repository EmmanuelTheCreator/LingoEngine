using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.Analysis;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.CodeGen;

internal static class BlLingoHandlerFallbackBuilder
{
    public static IReadOnlyList<BlLingoHandlerCodeBlock> Build(
        IReadOnlyList<BlSyntaxToken> tokens,
        IReadOnlyList<BlSyntaxTrivia> endTrivia,
        BlLingoSymbolTable symbols)
    {
        return BlLegacyHandlerCodeBlockClassifier.BuildBlocks(tokens, endTrivia, symbols);
    }
}
