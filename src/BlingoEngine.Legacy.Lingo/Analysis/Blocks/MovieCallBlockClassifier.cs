using System;
using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.CodeGen;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.Analysis.Blocks;

internal sealed class MovieCallBlockClassifier : IHandlerBlockClassifier
{
    public static MovieCallBlockClassifier Instance { get; } = new();

    private MovieCallBlockClassifier()
    {
    }

    public bool TryCreate(IReadOnlyList<BlSyntaxToken> tokens, BlLingoSymbolTable symbols, out BlLingoHandlerCodeBlock block)
    {
        block = null!;
        if (tokens.Count == 0)
        {
            return false;
        }

        if (TryParseCall(tokens, symbols, out var data))
        {
            block = new BlLingoHandlerCodeBlock(BlLingoHandlerCodeBlockKind.MovieCall, tokens, data);
            return true;
        }

        var equalsIndex = BlLegacyHandlerTokenUtilities.FindOperator(tokens, "=");
        if (equalsIndex > 0 && equalsIndex + 1 < tokens.Count)
        {
            var callTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, equalsIndex + 1, tokens.Count - (equalsIndex + 1));
            if (TryParseCall(callTokens, symbols, out var callData))
            {
                var targetTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 0, equalsIndex);
                var targetExpression = BlLegacyExpressionConverter.Convert(targetTokens);
                block = new BlLingoHandlerCodeBlock(
                    BlLingoHandlerCodeBlockKind.MovieCall,
                    tokens,
                    callData with
                    {
                        UsesResult = true,
                        ResultTargetExpression = string.IsNullOrEmpty(targetExpression)
                            ? callData.ResultTargetExpression
                            : targetExpression,
                    });
                return true;
            }
        }

        if (BlLegacyHandlerTokenUtilities.IsKeyword(tokens[0], "put"))
        {
            var intoIndex = BlLegacyHandlerTokenUtilities.FindKeyword(tokens, "into");
            if (intoIndex > 0)
            {
                var valueTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 1, intoIndex - 1);
                if (TryParseCall(valueTokens, symbols, out var callData))
                {
                    var targetTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, intoIndex + 1, tokens.Count - (intoIndex + 1));
                    var targetExpression = BlLegacyExpressionConverter.Convert(targetTokens);
                    block = new BlLingoHandlerCodeBlock(
                        BlLingoHandlerCodeBlockKind.MovieCall,
                        tokens,
                        callData with
                        {
                            UsesResult = true,
                            ResultTargetExpression = string.IsNullOrEmpty(targetExpression)
                                ? callData.ResultTargetExpression
                                : targetExpression,
                        });
                    return true;
                }
            }
        }

        return false;
    }

    private static bool TryParseCall(
        IReadOnlyList<BlSyntaxToken> tokens,
        BlLingoSymbolTable symbols,
        out BlLingoMovieCallBlockData data)
    {
        data = null!;
        if (tokens.Count == 0)
        {
            return false;
        }

        var handlerToken = tokens[0];
        if (handlerToken.Kind is not (BlSyntaxKind.IdentifierToken or BlSyntaxKind.SymbolToken))
        {
            return false;
        }

        var handlerName = BlCSharpName.SanitizeIdentifier(handlerToken.ValueText);
        if (string.IsNullOrEmpty(handlerName))
        {
            return false;
        }

        var arguments = new List<string>();
        if (tokens.Count == 1)
        {
            // No arguments.
        }
        else if (tokens.Count >= 2 && tokens[1].Kind == BlSyntaxKind.LeftParenthesisToken)
        {
            var closeIndex = BlLegacyHandlerTokenUtilities.FindMatchingToken(tokens, 1, BlSyntaxKind.LeftParenthesisToken, BlSyntaxKind.RightParenthesisToken);
            if (closeIndex != tokens.Count - 1)
            {
                return false;
            }

            var argumentTokens = BlLegacyHandlerTokenUtilities.SliceTokens(tokens, 2, closeIndex - 2);
            arguments = HandlerBlockClassifierUtilities.ParseArgumentExpressions(argumentTokens);
        }
        else
        {
            return false;
        }

        var targetScript = HandlerBlockClassifierUtilities.FindMovieHandlerScope(symbols, handlerName);
        if (string.IsNullOrEmpty(targetScript) &&
            !handlerName.EndsWith("Handler", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        data = new BlLingoMovieCallBlockData(handlerName, targetScript, UsesResult: false, arguments, ParameterName: "movie");
        return true;
    }
}
