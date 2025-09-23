using System;
using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.Analysis.Passes;
using BlingoEngine.Legacy.Lingo.Syntax;

namespace BlingoEngine.Legacy.Lingo.Analysis;

/// <summary>
/// Coordinates execution of analysis passes over a sequence of tokens.
/// </summary>
public sealed class BlLingoAnalyzer
{
    private readonly List<BlLingoAnalysisPass> _passes = new();
    private readonly IReadOnlyList<BlSyntaxToken> _tokens;

    private BlLingoAnalyzer(IReadOnlyList<BlSyntaxToken> tokens)
    {
        _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
    }

    /// <summary>
    /// Creates an analyzer configured with the default passes for declaration discovery, pending type tracking, and class linking.
    /// </summary>
    public static BlLingoAnalyzer Create(IReadOnlyList<BlSyntaxToken> tokens)
    {
        var analyzer = new BlLingoAnalyzer(tokens);
        // Pass order is important: discovery populates the symbol table, type linking records unresolved symbols, and class
        // linking collects class names for later reference resolution.
        analyzer.AddPass(new BlLingoDeclarationPass());
        analyzer.AddPass(new BlLingoTypeLinkPass());
        analyzer.AddPass(new BlLingoClassLinkPass());
        analyzer.AddPass(new BlLingoHandlerCodeBlockPass());
        return analyzer;
    }

    /// <summary>
    /// Adds a pass to the analysis pipeline.
    /// </summary>
    public BlLingoAnalyzer AddPass(BlLingoAnalysisPass pass)
    {
        ArgumentNullException.ThrowIfNull(pass);
        _passes.Add(pass);
        return this;
    }

    /// <summary>
    /// Executes all configured passes and returns the result.
    /// </summary>
    public BlLingoAnalysisResult Run()
    {
        var context = new BlLingoAnalysisContext(_tokens, new BlLingoSymbolTable());
        foreach (var pass in _passes)
        {
            pass.Execute(context);
        }

        return new BlLingoAnalysisResult(_tokens, context.Symbols, context.GetDataSnapshot());
    }
}
