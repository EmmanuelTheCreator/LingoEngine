using System;
using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.Analysis;

/// <summary>
/// Represents a classified unit of code or trivia discovered while analysing a handler body.
/// </summary>
public sealed class BlLingoHandlerCodeBlock
{
    public BlLingoHandlerCodeBlock(
        BlLingoHandlerCodeBlockKind kind,
        IReadOnlyList<BlSyntaxToken> tokens,
        object? data = null,
        string? commentText = null)
    {
        Kind = kind;
        Tokens = tokens ?? Array.Empty<BlSyntaxToken>();
        Data = data;
        CommentText = commentText;
    }

    /// <summary>
    /// Gets the logical kind of the block.
    /// </summary>
    public BlLingoHandlerCodeBlockKind Kind { get; }

    /// <summary>
    /// Gets the tokens that composed the block.
    /// </summary>
    public IReadOnlyList<BlSyntaxToken> Tokens { get; }

    /// <summary>
    /// Gets optional metadata attached to the block.
    /// </summary>
    public object? Data { get; }

    /// <summary>
    /// Gets the comment text when the block represents a comment.
    /// </summary>
    public string? CommentText { get; }
}
