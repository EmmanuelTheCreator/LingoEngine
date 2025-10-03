using System;
using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.Analysis.Blocks;
using BlingoEngine.Legacy.Lingo.CodeGen;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.Analysis;

internal static class BlLegacyHandlerCodeBlockClassifier
{
    private static readonly IHandlerBlockClassifier[] s_classifiers =
    {
        EndBlockClassifier.Instance,
        ConditionalBlockClassifier.Instance,
        RepeatBlockClassifier.Instance,
        CaseBlockClassifier.Instance,
        SendSpriteBlockClassifier.Instance,
        MovieCallBlockClassifier.Instance,
        PutBlockClassifier.Instance,
        SpriteMemberAssignmentBlockClassifier.Instance,
        ActorListBlockClassifier.Instance,
        ExitNextBlockClassifier.Instance,
        ExpressionBlockClassifier.Instance,
    };

    public static IReadOnlyList<BlLingoHandlerCodeBlock> BuildBlocks(
        IReadOnlyList<BlSyntaxToken> tokens,
        IReadOnlyList<BlSyntaxTrivia> endTrivia,
        BlLingoSymbolTable symbols)
    {
        var result = new List<BlLingoHandlerCodeBlock>();
        var lines = HandlerLine.Split(tokens, endTrivia);
        var inlineStack = new Stack<bool>();
        foreach (var line in lines)
        {
            switch (line.Kind)
            {
                case HandlerLineKind.Blank:
                    result.Add(new BlLingoHandlerCodeBlock(BlLingoHandlerCodeBlockKind.Blank, Array.Empty<BlSyntaxToken>()));
                    break;
                case HandlerLineKind.Comment:
                    result.Add(new BlLingoHandlerCodeBlock(BlLingoHandlerCodeBlockKind.Comment, Array.Empty<BlSyntaxToken>(), commentText: line.CommentText));
                    break;
                case HandlerLineKind.Tokens:
                    if (line.Tokens is { Count: > 0 })
                    {
                        var block = ClassifyTokens(line.Tokens, symbols);
                        result.Add(block);
                        if (line.BeginsInlineConditional && block.Kind == BlLingoHandlerCodeBlockKind.If)
                        {
                            inlineStack.Push(true);
                        }

                        if (line.EndsInlineConditional && inlineStack.Count > 0)
                        {
                            inlineStack.Pop();
                            result.Add(new BlLingoHandlerCodeBlock(BlLingoHandlerCodeBlockKind.End, Array.Empty<BlSyntaxToken>()));
                        }
                    }
                    else
                    {
                        result.Add(ExpressionBlockClassifier.Instance.TryCreate(line.Tokens, symbols, out var block)
                            ? block
                            : new BlLingoHandlerCodeBlock(BlLingoHandlerCodeBlockKind.Expression, line.Tokens));
                    }
                    break;
            }
        }

        return result;
    }

    private static BlLingoHandlerCodeBlock ClassifyTokens(
        IReadOnlyList<BlSyntaxToken> tokens,
        BlLingoSymbolTable symbols)
    {
        foreach (var classifier in s_classifiers)
        {
            if (classifier.TryCreate(tokens, symbols, out var block))
            {
                return block;
            }
        }

        var treatReturnAsStatement =
            tokens.Count > 0 && BlLegacyHandlerTokenUtilities.IsKeyword(tokens[0], "return");
        var expression = BlLegacyExpressionConverter.Convert(tokens, treatReturnAsStatement);
        return new BlLingoHandlerCodeBlock(
            BlLingoHandlerCodeBlockKind.Expression,
            tokens,
            new BlLingoExpressionBlockData(expression));
    }
}
