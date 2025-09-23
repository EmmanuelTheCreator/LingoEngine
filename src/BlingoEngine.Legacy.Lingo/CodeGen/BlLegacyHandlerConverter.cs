using System;
using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.Analysis;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.CodeGen;

/// <summary>
/// Translates legacy Lingo handler bodies into C# statements following the
/// conversion tables outlined in docs/design/Blingo_vs_CSharp.md.
/// </summary>
public sealed class BlLegacyHandlerConverter
{
    private readonly IReadOnlyList<BlSyntaxToken> _tokens;
    private readonly BlLegacyClassGeneratorOptions _options;

    public BlLegacyHandlerConverter(string source, IReadOnlyList<BlSyntaxToken> tokens, BlLegacyClassGeneratorOptions options)
    {
        _ = source;
        _tokens = tokens ?? Array.Empty<BlSyntaxToken>();
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Writes the translated body for the supplied handler.
    /// </summary>
    public void WriteHandlerBody(BlCSharpCodeWriter writer, BlLingoHandlerSymbolTable handler)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(handler);

        var body = ExtractHandlerBody(handler);
        if (body.Tokens.Count == 0 && body.EndLeadingTrivia.Count == 0)
        {
            return;
        }

        var emitter = new BlLegacyHandlerBodyEmitter(writer, body.Tokens, body.EndLeadingTrivia, _options);
        emitter.Emit();
    }

    private HandlerBody ExtractHandlerBody(BlLingoHandlerSymbolTable handler)
    {
        if (handler.Symbol.Declarations.Count == 0)
        {
            return HandlerBody.Empty;
        }

        var declaration = handler.Symbol.Declarations[0];
        var declarationIndex = IndexOfToken(declaration);
        if (declarationIndex < 0)
        {
            return HandlerBody.Empty;
        }

        var bodyStartIndex = declarationIndex + 1;
        while (bodyStartIndex < _tokens.Count)
        {
            var token = _tokens[bodyStartIndex];
            if (ContainsNewLine(token.LeadingTrivia))
            {
                break;
            }

            bodyStartIndex++;
        }

        if (bodyStartIndex >= _tokens.Count)
        {
            return HandlerBody.Empty;
        }

        var endIndex = FindHandlerEndIndex(bodyStartIndex);
        if (endIndex < 0 || endIndex <= bodyStartIndex)
        {
            return HandlerBody.Empty;
        }

        var bodyTokens = new List<BlSyntaxToken>(endIndex - bodyStartIndex);
        for (var index = bodyStartIndex; index < endIndex; index++)
        {
            bodyTokens.Add(_tokens[index]);
        }

        return new HandlerBody(bodyTokens, _tokens[endIndex].LeadingTrivia);
    }

    private int IndexOfToken(BlSyntaxToken target)
    {
        for (var index = 0; index < _tokens.Count; index++)
        {
            if (ReferenceEquals(_tokens[index], target))
            {
                return index;
            }
        }

        return -1;
    }

    private int FindHandlerEndIndex(int startIndex)
    {
        for (var index = Math.Max(startIndex, 0); index < _tokens.Count; index++)
        {
            var token = _tokens[index];
            if (token.Kind != BlSyntaxKind.KeywordToken)
            {
                continue;
            }

            if (!string.Equals(token.ValueText, "end", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (IsHandlerTerminator(index))
            {
                return index;
            }
        }

        return -1;
    }

    private bool IsHandlerTerminator(int index)
    {
        var nextIndex = index + 1;
        if (nextIndex >= _tokens.Count)
        {
            return true;
        }

        var next = _tokens[nextIndex];
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

    private readonly record struct HandlerBody(
        IReadOnlyList<BlSyntaxToken> Tokens,
        IReadOnlyList<BlSyntaxTrivia> EndLeadingTrivia)
    {
        public static HandlerBody Empty { get; } = new(Array.Empty<BlSyntaxToken>(), Array.Empty<BlSyntaxTrivia>());
    }
}
