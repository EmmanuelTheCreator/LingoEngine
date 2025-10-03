using System.Collections.Generic;
using BlingoEngine.Legacy.Lingo.Analysis;
using BlingoEngine.Legacy.Lingo.Analysis.Passes;
using BlingoEngine.Legacy.Lingo.Syntax;
using Xunit;

namespace BlingoEngine.Legacy.Lingo.Tests;

public sealed class LegacyTypeAnalysisLinkingTests
{
    [Fact]
    public void PropertyAssignedFromSelfProperty_AdoptsResolvedType()
    {
        const string Source = """
property sourceProp, destProp

on startMovie
  sourceProp = 42
  destProp = me.sourceProp
end
""";

        var analysis = Analyze(Source);
        var movieScope = analysis.Symbols.MovieScript;

        Assert.True(movieScope.Properties.TryGetValue("sourceProp", out var sourceProperty));
        Assert.Equal("int", sourceProperty.ResolvedTypeName);
        Assert.True(movieScope.Properties.TryGetValue("destProp", out var destProperty));
        Assert.Equal("int", destProperty.ResolvedTypeName);
    }

    [Fact]
    public void PropertyAssignedFromParameter_AdoptsParameterHints()
    {
        const string Source = """
property destProp

on foo value
  destProp = value
end

on startMovie
  foo(5)
end
""";

        var analysis = Analyze(Source);
        var movieScope = analysis.Symbols.MovieScript;

        Assert.True(analysis.TryGetData(BlLingoHandlerCodeBlockPass.HandlerCodeBlocksKey,
            out IReadOnlyDictionary<BlLingoHandlerSymbolTable, IReadOnlyList<BlLingoHandlerCodeBlock>>? blockMap));
        Assert.NotNull(blockMap);
        var startMovieSymbol = movieScope.Handlers["startMovie"];
        var startBlocks = blockMap![startMovieSymbol];
        Assert.Contains(startBlocks, block => block.Kind == BlLingoHandlerCodeBlockKind.MovieCall);

        Assert.True(movieScope.Properties.TryGetValue("destProp", out var destProperty));
        Assert.Equal("int", destProperty.ResolvedTypeName);

        Assert.True(movieScope.Handlers.TryGetValue("foo", out var fooHandler));
        Assert.True(fooHandler.Parameters.TryGetValue("value", out var valueParameter));
        Assert.Equal("int", valueParameter.ResolvedTypeName);
    }

    private static BlLingoAnalysisResult Analyze(string source)
    {
        var tokenizer = new BlLingoTokenizer();
        var tokens = tokenizer.Tokenize(source);
        return BlLingoAnalyzer.Create(tokens).Run();
    }
}
