using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.CodeGen;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.Analysis.Blocks;

internal sealed class EndBlockClassifier : IHandlerBlockClassifier
{
    public static EndBlockClassifier Instance { get; } = new();

    private EndBlockClassifier()
    {
    }

    public bool TryCreate(IReadOnlyList<BlSyntaxToken> tokens, BlLingoSymbolTable symbols, out BlLingoHandlerCodeBlock block)
    {
        block = null!;
        if (tokens.Count == 0)
        {
            return false;
        }

        if (!BlLegacyHandlerTokenUtilities.IsKeyword(tokens[0], "end"))
        {
            return false;
        }

        block = new BlLingoHandlerCodeBlock(BlLingoHandlerCodeBlockKind.End, tokens);
        return true;
    }
}
