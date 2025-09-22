using System;
using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.Analysis;

/// <summary>
/// Represents the output produced after running the configured analysis passes.
/// </summary>
public sealed class BlLingoAnalysisResult
{
    internal BlLingoAnalysisResult(
        IReadOnlyList<BlSyntaxToken> tokens,
        BlLingoSymbolTable symbols,
        IReadOnlyDictionary<string, object?> data)
    {
        Tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
        Symbols = symbols ?? throw new ArgumentNullException(nameof(symbols));
        Data = data ?? throw new ArgumentNullException(nameof(data));
    }

    /// <summary>
    /// Gets the tokens that were analyzed.
    /// </summary>
    public IReadOnlyList<BlSyntaxToken> Tokens { get; }

    /// <summary>
    /// Gets the populated symbol table.
    /// </summary>
    public BlLingoSymbolTable Symbols { get; }

    /// <summary>
    /// Gets custom data exposed by the executed passes.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Data { get; }
}
