using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.CodeGen;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.Analysis.Blocks;

internal sealed class ExitNextBlockClassifier : IHandlerBlockClassifier
{
    public static ExitNextBlockClassifier Instance { get; } = new();

    private ExitNextBlockClassifier()
    {
    }

    public bool TryCreate(IReadOnlyList<BlSyntaxToken> tokens, BlLingoSymbolTable symbols, out BlLingoHandlerCodeBlock block)
    {
        block = null!;
        if (tokens.Count == 0)
        {
            return false;
        }

        if (BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[0], "exit"))
        {
            if (tokens.Count >= 3 &&
                BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[1], "repeat") &&
                BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[2], "if"))
            {
                var conditionTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 3, tokens.Count - 3);
                var condition = BlLegacyExpressionConverter.Convert(conditionTokens);
                block = new BlLingoHandlerCodeBlock(
                    BlLingoHandlerCodeBlockKind.ExitRepeatIf,
                    tokens,
                    new BlLingoExitRepeatIfBlockData(condition));
                return true;
            }

            if (tokens.Count >= 2 && BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[1], "repeat"))
            {
                block = new BlLingoHandlerCodeBlock(BlLingoHandlerCodeBlockKind.ExitRepeat, tokens);
                return true;
            }

            if (tokens.Count == 1)
            {
                block = new BlLingoHandlerCodeBlock(BlLingoHandlerCodeBlockKind.Return, tokens);
                return true;
            }
        }

        if (BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[0], "next") &&
            tokens.Count >= 2 &&
            BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[1], "repeat"))
        {
            if (tokens.Count >= 3 && BlLegacyHandlerTokenUtilities.IsIdentifier(tokens[2], "if"))
            {
                var conditionTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 3, tokens.Count - 3);
                var condition = BlLegacyExpressionConverter.Convert(conditionTokens);
                block = new BlLingoHandlerCodeBlock(
                    BlLingoHandlerCodeBlockKind.NextRepeatIf,
                    tokens,
                    new BlLingoExitRepeatIfBlockData(condition, UseContinue: true));
                return true;
            }

            block = new BlLingoHandlerCodeBlock(BlLingoHandlerCodeBlockKind.NextRepeat, tokens);
            return true;
        }

        return false;
    }
}
