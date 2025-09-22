using System;
using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.CodeGen;

public sealed class HandlerLine
{
    private HandlerLine(HandlerLineKind kind, IReadOnlyList<BlSyntaxToken> tokens, string? commentText)
    {
        Kind = kind;
        Tokens = tokens;
        CommentText = commentText;
    }

    public HandlerLineKind Kind { get; }

    public IReadOnlyList<BlSyntaxToken> Tokens { get; }

    public string? CommentText { get; }

    public static List<HandlerLine> Split(IReadOnlyList<BlSyntaxToken> tokens, IReadOnlyList<BlSyntaxTrivia> endTrivia)
    {
        var result = new List<HandlerLine>();
        var currentTokens = new List<BlSyntaxToken>();
        var lastWasBlank = false;

        foreach (var token in tokens)
        {
            ProcessTrivia(token.LeadingTrivia, result, currentTokens, ref lastWasBlank);
            currentTokens.Add(token);
            lastWasBlank = false;
            ProcessTrivia(token.TrailingTrivia, result, currentTokens, ref lastWasBlank);
        }

        ProcessTrivia(endTrivia, result, currentTokens, ref lastWasBlank);
        AddTokenLine(result, currentTokens, ref lastWasBlank);

        for (var index = result.Count - 1; index >= 0; index--)
        {
            if (result[index].Kind == HandlerLineKind.Blank)
            {
                result.RemoveAt(index);
            }
            else
            {
                break;
            }
        }

        return result;
    }

    private static void ProcessTrivia(
        IReadOnlyList<BlSyntaxTrivia> trivia,
        List<HandlerLine> result,
        List<BlSyntaxToken> currentTokens,
        ref bool lastWasBlank)
    {
        if (trivia is null)
        {
            return;
        }

        foreach (var item in trivia)
        {
            if (item.Kind == BlSyntaxKind.CommentTrivia)
            {
                AddTokenLine(result, currentTokens, ref lastWasBlank);
                AddCommentLine(result, ExtractCommentText(item), ref lastWasBlank);
                continue;
            }

            if (item.Kind == BlSyntaxKind.NewLineTrivia)
            {
                AddTokenLine(result, currentTokens, ref lastWasBlank);
                AddBlankLine(result, ref lastWasBlank);
            }
        }
    }

    private static void AddTokenLine(
        List<HandlerLine> result,
        List<BlSyntaxToken> currentTokens,
        ref bool lastWasBlank)
    {
        if (currentTokens.Count == 0)
        {
            return;
        }

        result.Add(new HandlerLine(HandlerLineKind.Tokens, currentTokens.ToArray(), null));
        currentTokens.Clear();
        lastWasBlank = false;
    }

    private static void AddBlankLine(List<HandlerLine> result, ref bool lastWasBlank)
    {
        if (lastWasBlank)
        {
            return;
        }

        result.Add(new HandlerLine(HandlerLineKind.Blank, Array.Empty<BlSyntaxToken>(), null));
        lastWasBlank = true;
    }

    private static void AddCommentLine(List<HandlerLine> result, string text, ref bool lastWasBlank)
    {
        result.Add(new HandlerLine(HandlerLineKind.Comment, Array.Empty<BlSyntaxToken>(), text));
        lastWasBlank = false;
    }

    private static string ExtractCommentText(BlSyntaxTrivia trivia)
    {
        var text = trivia.Text.Trim();
        if (text.StartsWith("--", StringComparison.Ordinal))
        {
            text = text[2..];
        }

        return text.Trim();
    }
}
