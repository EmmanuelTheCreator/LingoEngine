using System;
using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.CodeGen;

public sealed class HandlerTokenEmitContext
{
    public HandlerTokenEmitContext(BlCSharpCodeWriter writer, IReadOnlyList<BlSyntaxToken> tokens, HandlerBlockManager blocks)
    {
        Writer = writer ?? throw new ArgumentNullException(nameof(writer));
        Tokens = tokens ?? Array.Empty<BlSyntaxToken>();
        Blocks = blocks ?? throw new ArgumentNullException(nameof(blocks));
    }

    public BlCSharpCodeWriter Writer { get; }

    public IReadOnlyList<BlSyntaxToken> Tokens { get; }

    public HandlerBlockManager Blocks { get; }

    public string ConvertExpression(IReadOnlyList<BlSyntaxToken> tokens)
    {
        return BlLegacyExpressionConverter.Convert(tokens);
    }
}
