using System.Collections.Generic;
using System.Linq;
using BlingoEngine.Legacy.Lingo.Analysis;
using BlingoEngine.Legacy.Lingo.Analysis.Passes;
using BlingoEngine.Legacy.Lingo.Syntax;
using FluentAssertions;

namespace BlingoEngine.Legacy.Lingo.Tests;

public sealed class BlLingoAnalyzerTests
{
    private readonly BlLingoTokenizer _tokenizer = new();

    [Fact]
    public void Analyzer_CollectsDeclarationsAcrossPasses()
    {
        const string script = """
global gScore, ¬
      gLives
script "Enemy"
property pSprite, myPropertyForEnemy

on mouseDown me, theStage
  myPropertyForEnemy = script("myEnemyScript").new()
  gScore = gScore + gLives
end
""";

        var tokens = _tokenizer.Tokenize(script);
        var analyzer = BlLingoAnalyzer.Create(tokens);
        var result = analyzer.Run();

        result.Symbols.Globals.Keys.Should().Contain(new[] { "gScore", "gLives" });
        result.Symbols.Classes.Keys.Should().Contain("Enemy");

        result.Symbols.ClassScopes.Should().ContainKey("Enemy");
        var enemyClass = result.Symbols.ClassScopes["Enemy"];
        enemyClass.Properties.Keys.Should().Contain(new[] { "pSprite", "myPropertyForEnemy" });
        var enemyProperty = enemyClass.Properties["myPropertyForEnemy"];
        enemyProperty.TypeCode.Should().Be("myEnemyScript");
        enemyProperty.ResolvedTypeName.Should().Be("myEnemyScript");
        enemyClass.Handlers.Should().ContainKey("mouseDown");
        var mouseDown = enemyClass.Handlers["mouseDown"];
        mouseDown.Parameters.Keys.Should().Contain(new[] { "me", "theStage" });

        result.Data.Should().ContainKey(BlLingoTypeLinkPass.PendingTypeSymbolsKey);
        result.Data[BlLingoTypeLinkPass.PendingTypeSymbolsKey]
            .Should()
            .BeAssignableTo<IEnumerable<BlCodeSymbol>>()
            .Which.Should().Contain(symbol => symbol.Name == "me");

        result.Data.Should().ContainKey(BlLingoClassLinkPass.KnownClassesKey);
        result.Data[BlLingoClassLinkPass.KnownClassesKey]
            .Should()
            .BeAssignableTo<IEnumerable<string>>()
            .Which.Should().Contain("Enemy");
    }
}
