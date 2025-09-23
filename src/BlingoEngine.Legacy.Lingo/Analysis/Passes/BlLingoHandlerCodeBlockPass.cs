using System;
using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.CodeGen;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.Analysis.Passes;

/// <summary>
/// Classifies handler body statements into high level code blocks so code generation can operate on pre-analysed structures.
/// </summary>
public sealed class BlLingoHandlerCodeBlockPass : BlLingoAnalysisPass
{
    public const string HandlerCodeBlocksKey = "Legacy.HandlerCodeBlocks";

    public BlLingoHandlerCodeBlockPass()
        : base("HandlerCodeBlocks")
    {
    }

    public override void Execute(BlLingoAnalysisContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var tokens = context.Tokens;
        var map = new Dictionary<BlLingoHandlerSymbolTable, IReadOnlyList<BlLingoHandlerCodeBlock>>();

        foreach (var classScope in EnumerateClasses(context.Symbols))
        {
            foreach (var handler in classScope.Handlers.Values)
            {
                if (handler is null)
                {
                    continue;
                }

                var body = ExtractHandlerBody(tokens, handler);
                if (body.Tokens.Count == 0 && body.EndTrivia.Count == 0)
                {
                    map[handler] = Array.Empty<BlLingoHandlerCodeBlock>();
                    continue;
                }

                var blocks = BlLegacyHandlerCodeBlockClassifier.BuildBlocks(body.Tokens, body.EndTrivia, context.Symbols);
                map[handler] = blocks;
            }
        }

        context.SetData(HandlerCodeBlocksKey, map);
    }

    private static IEnumerable<BlLingoClassSymbolTable> EnumerateClasses(BlLingoSymbolTable symbols)
    {
        yield return symbols.MovieScript;
        foreach (var scope in symbols.ClassScopes.Values)
        {
            yield return scope;
        }
    }

    private static HandlerBody ExtractHandlerBody(IReadOnlyList<BlSyntaxToken> tokens, BlLingoHandlerSymbolTable handler)
    {
        if (handler.Symbol.Declarations.Count == 0)
        {
            return HandlerBody.Empty;
        }

        var declaration = handler.Symbol.Declarations[0];
        var declarationIndex = IndexOfToken(tokens, declaration);
        if (declarationIndex < 0)
        {
            return HandlerBody.Empty;
        }

        var bodyStartIndex = declarationIndex + 1;
        while (bodyStartIndex < tokens.Count)
        {
            var token = tokens[bodyStartIndex];
            if (ContainsNewLine(token.LeadingTrivia))
            {
                break;
            }

            bodyStartIndex++;
        }

        if (bodyStartIndex >= tokens.Count)
        {
            return HandlerBody.Empty;
        }

        var endIndex = FindHandlerEndIndex(tokens, bodyStartIndex);
        if (endIndex < 0 || endIndex <= bodyStartIndex)
        {
            return HandlerBody.Empty;
        }

        var bodyTokens = new List<BlSyntaxToken>(endIndex - bodyStartIndex);
        for (var index = bodyStartIndex; index < endIndex; index++)
        {
            bodyTokens.Add(tokens[index]);
        }

        return new HandlerBody(bodyTokens, tokens[endIndex].LeadingTrivia ?? Array.Empty<BlSyntaxTrivia>());
    }

    private static int IndexOfToken(IReadOnlyList<BlSyntaxToken> tokens, BlSyntaxToken target)
    {
        for (var index = 0; index < tokens.Count; index++)
        {
            if (ReferenceEquals(tokens[index], target))
            {
                return index;
            }
        }

        return -1;
    }

    private static int FindHandlerEndIndex(IReadOnlyList<BlSyntaxToken> tokens, int startIndex)
    {
        for (var index = Math.Max(startIndex, 0); index < tokens.Count; index++)
        {
            var token = tokens[index];
            if (token.Kind != BlSyntaxKind.KeywordToken)
            {
                continue;
            }

            if (!string.Equals(token.ValueText, "end", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (IsHandlerTerminator(tokens, index))
            {
                return index;
            }
        }

        return -1;
    }

    private static bool IsHandlerTerminator(IReadOnlyList<BlSyntaxToken> tokens, int index)
    {
        var nextIndex = index + 1;
        if (nextIndex >= tokens.Count)
        {
            return true;
        }

        var next = tokens[nextIndex];
        if (next.Kind == BlSyntaxKind.EndOfFileToken)
        {
            return true;
        }

        return ContainsNewLine(next.LeadingTrivia);
    }

    private static bool ContainsNewLine(IReadOnlyList<BlSyntaxTrivia> trivia)
    {
        if (trivia is null)
        {
            return false;
        }

        for (var index = 0; index < trivia.Count; index++)
        {
            if (trivia[index].Kind == BlSyntaxKind.NewLineTrivia)
            {
                return true;
            }
        }

        return false;
    }


    private sealed class HandlerBody
    {
        public static HandlerBody Empty { get; } = new(Array.Empty<BlSyntaxToken>(), Array.Empty<BlSyntaxTrivia>());

        public HandlerBody(IReadOnlyList<BlSyntaxToken> tokens, IReadOnlyList<BlSyntaxTrivia> endTrivia)
        {
            Tokens = tokens ?? Array.Empty<BlSyntaxToken>();
            EndTrivia = endTrivia ?? Array.Empty<BlSyntaxTrivia>();
        }

        public IReadOnlyList<BlSyntaxToken> Tokens { get; }

        public IReadOnlyList<BlSyntaxTrivia> EndTrivia { get; }
    }
}
