using System;
using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.CodeGen;

public sealed class BlLegacyHandlerBodyEmitter
{
    private readonly BlCSharpCodeWriter _writer;
    private readonly IReadOnlyList<BlSyntaxToken> _tokens;
    private readonly IReadOnlyList<BlSyntaxTrivia> _endTrivia;
    private readonly BlLegacyClassGeneratorOptions _options;
    private readonly HandlerBlockManager _blocks;
    private readonly IReadOnlyList<IHandlerTokenVisitor> _visitors;

    public BlLegacyHandlerBodyEmitter(
        BlCSharpCodeWriter writer,
        IReadOnlyList<BlSyntaxToken> tokens,
        IReadOnlyList<BlSyntaxTrivia> endTrivia,
        BlLegacyClassGeneratorOptions options)
        : this(writer, tokens, endTrivia, options, null)
    {
    }

    public BlLegacyHandlerBodyEmitter(
        BlCSharpCodeWriter writer,
        IReadOnlyList<BlSyntaxToken> tokens,
        IReadOnlyList<BlSyntaxTrivia> endTrivia,
        BlLegacyClassGeneratorOptions options,
        IReadOnlyList<IHandlerTokenVisitor>? visitors)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        _tokens = tokens ?? Array.Empty<BlSyntaxToken>();
        _endTrivia = endTrivia ?? Array.Empty<BlSyntaxTrivia>();
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _blocks = new HandlerBlockManager(_writer);
        _visitors = visitors ?? CreateDefaultVisitors();
    }

    public void Emit()
    {
        var lines = HandlerLine.Split(_tokens, _endTrivia);
        if (lines.Count == 0)
        {
            return;
        }

        foreach (var line in lines)
        {
            switch (line.Kind)
            {
                case HandlerLineKind.Blank:
                    _writer.WriteLine();
                    break;
                case HandlerLineKind.Comment:
                    WriteComment(line.CommentText);
                    break;
                case HandlerLineKind.Tokens:
                    EmitTokens(line.Tokens);
                    break;
            }
        }

        _blocks.CloseAll();
    }

    private void EmitTokens(IReadOnlyList<BlSyntaxToken> tokens)
    {
        if (tokens.Count == 0)
        {
            return;
        }

        var context = new HandlerTokenEmitContext(_writer, tokens, _blocks, _options);
        foreach (var visitor in _visitors)
        {
            if (visitor.TryHandle(context))
            {
                return;
            }
        }
    }

    private void WriteComment(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            _writer.WriteLine("//");
        }
        else
        {
            _writer.WriteLine("// " + text);
        }
    }

    private static IReadOnlyList<IHandlerTokenVisitor> CreateDefaultVisitors()
    {
        return new IHandlerTokenVisitor[]
        {
            new EndTokenVisitor(),
            new ElseTokenVisitor(),
            new RepeatTokenVisitor(),
            new CaseTokenVisitor(),
            new PutTokenVisitor(),
            new SpriteMemberAssignmentVisitor(),
            new ActorListAppendVisitor(),
            new ActorListRemoveVisitor(),
            new SendSpriteVisitor(),
            new IfTokenVisitor(),
            new ExitNextTokenVisitor(),
            new MovieCallVisitor(),
            new ExpressionStatementVisitor(),
        };
    }
}
