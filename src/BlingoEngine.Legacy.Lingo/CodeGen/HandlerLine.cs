using System;
using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.CodeGen;

public sealed class HandlerLine
{
    private HandlerLine(
        HandlerLineKind kind,
        IReadOnlyList<BlSyntaxToken> tokens,
        string? commentText,
        bool beginsInlineConditional = false,
        bool endsInlineConditional = false)
    {
        Kind = kind;
        Tokens = tokens;
        CommentText = commentText;
        BeginsInlineConditional = beginsInlineConditional;
        EndsInlineConditional = endsInlineConditional;
    }

    public HandlerLineKind Kind { get; }

    public IReadOnlyList<BlSyntaxToken> Tokens { get; }

    public string? CommentText { get; }

    public bool BeginsInlineConditional { get; }

    public bool EndsInlineConditional { get; }

    public static List<HandlerLine> Split(IReadOnlyList<BlSyntaxToken> tokens, IReadOnlyList<BlSyntaxTrivia> endTrivia)
    {
        var result = new List<HandlerLine>();
        var currentTokens = new List<BlSyntaxToken>();
        var lastWasBlank = false;
        var lineBreakPending = false;

        foreach (var token in tokens)
        {
            ProcessTrivia(token.LeadingTrivia, result, currentTokens, ref lastWasBlank, ref lineBreakPending);
            currentTokens.Add(token);
            lastWasBlank = false;
            lineBreakPending = false;
            ProcessTrivia(token.TrailingTrivia, result, currentTokens, ref lastWasBlank, ref lineBreakPending);
        }

        ProcessTrivia(endTrivia, result, currentTokens, ref lastWasBlank, ref lineBreakPending);
        AddTokenLine(result, currentTokens, ref lastWasBlank, ref lineBreakPending);

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
        ref bool lastWasBlank,
        ref bool lineBreakPending)
    {
        if (trivia is null)
        {
            return;
        }

        foreach (var item in trivia)
        {
            if (item.Kind == BlSyntaxKind.CommentTrivia)
            {
                AddTokenLine(result, currentTokens, ref lastWasBlank, ref lineBreakPending);
                AddCommentLine(result, ExtractCommentText(item), ref lastWasBlank);
                lineBreakPending = false;
                continue;
            }

            if (item.Kind == BlSyntaxKind.NewLineTrivia)
            {
                var hadPendingBreak = lineBreakPending;
                AddTokenLine(result, currentTokens, ref lastWasBlank, ref lineBreakPending);

                if (hadPendingBreak)
                {
                    AddBlankLine(result, ref lastWasBlank);
                }

                lineBreakPending = true;
            }
        }
    }

    private static void AddTokenLine(
        List<HandlerLine> result,
        List<BlSyntaxToken> currentTokens,
        ref bool lastWasBlank,
        ref bool lineBreakPending)
    {
        if (currentTokens.Count == 0)
        {
            return;
        }

        foreach (var segment in SplitInlineThenTokens(currentTokens))
        {
            if (segment.Tokens.Count == 0)
            {
                continue;
            }

            result.Add(new HandlerLine(
                HandlerLineKind.Tokens,
                segment.Tokens.ToArray(),
                null,
                segment.BeginsInline,
                segment.EndsInline));
        }

        currentTokens.Clear();
        lastWasBlank = false;
        lineBreakPending = false;
    }

    private static void AddBlankLine(List<HandlerLine> result, ref bool lastWasBlank)
    {
        if (lastWasBlank || result.Count == 0)
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

    private static List<InlineSegment> SplitInlineThenTokens(List<BlSyntaxToken> tokens)
    {
        var segments = new List<InlineSegment>();
        if (tokens.Count == 0)
        {
            return segments;
        }

        var splitIndex = FindInlineThenIndex(tokens);
        if (splitIndex < 0)
        {
            segments.Add(new InlineSegment(new List<BlSyntaxToken>(tokens), beginsInline: false, endsInline: false));
            return segments;
        }

        var head = new List<BlSyntaxToken>(tokens.GetRange(0, splitIndex + 1));
        var tail = new List<BlSyntaxToken>(tokens.GetRange(splitIndex + 1, tokens.Count - (splitIndex + 1)));
        segments.Add(new InlineSegment(head, beginsInline: true, endsInline: tail.Count == 0));
        if (tail.Count > 0)
        {
            segments.Add(new InlineSegment(tail, beginsInline: false, endsInline: true));
        }

        return segments;
    }

    private static int FindInlineThenIndex(List<BlSyntaxToken> tokens)
    {
        for (var index = 0; index < tokens.Count; index++)
        {
            var token = tokens[index];
            if (token.Kind != BlSyntaxKind.KeywordToken ||
                !string.Equals(token.ValueText, "then", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (index + 1 < tokens.Count)
            {
                return index;
            }
        }

        return -1;
    }

    private readonly struct InlineSegment
    {
        public InlineSegment(List<BlSyntaxToken> tokens, bool beginsInline, bool endsInline)
        {
            Tokens = tokens;
            BeginsInline = beginsInline;
            EndsInline = endsInline;
        }

        public List<BlSyntaxToken> Tokens { get; }

        public bool BeginsInline { get; }

        public bool EndsInline { get; }
    }
}
