using System;
using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.Analysis;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.CodeGen;

public sealed class HandlerTokenEmitContext
{
    public HandlerTokenEmitContext(
        BlCSharpCodeWriter writer,
        IReadOnlyList<BlSyntaxToken> tokens,
        HandlerBlockManager blocks,
        BlLegacyClassGeneratorOptions options)
    {
        Writer = writer ?? throw new ArgumentNullException(nameof(writer));
        Tokens = tokens ?? Array.Empty<BlSyntaxToken>();
        Blocks = blocks ?? throw new ArgumentNullException(nameof(blocks));
        Options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public BlCSharpCodeWriter Writer { get; }

    public IReadOnlyList<BlSyntaxToken> Tokens { get; }

    public HandlerBlockManager Blocks { get; }

    public BlLegacyClassGeneratorOptions Options { get; }

    public string ComposeClassName(string scriptName, BlLingoScriptKind kind)
    {
        return BlCSharpName.ComposeClassName(scriptName, kind, Options);
    }

    public string ConvertExpression(IReadOnlyList<BlSyntaxToken> tokens)
    {
        return BlLegacyExpressionConverter.Convert(tokens);
    }
}
