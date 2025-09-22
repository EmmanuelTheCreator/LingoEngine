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
property pSprite, myPropertyForEnemy

on beginSprite me
  myPropertyForEnemy = script("myEnemyScript").new()
end

on mouseDown me, theStage
  gScore = gScore + gLives
end
""";

        var tokens = _tokenizer.Tokenize(script);
        var analyzer = BlLingoAnalyzer.Create(tokens);
        var result = analyzer.Run();

        result.Symbols.Globals.Keys.Should().Contain(new[] { "gScore", "gLives" });
        result.Symbols.Classes.Should().BeEmpty();

        var movieScript = result.Symbols.MovieScript;
        movieScript.Properties.Keys.Should().Contain(new[] { "pSprite", "myPropertyForEnemy" });
        movieScript.ScriptKind.Should().Be(BlLingoScriptKind.Movie);
        var enemyProperty = movieScript.Properties["myPropertyForEnemy"];
        enemyProperty.TypeCode.Should().Be("myEnemyScript");
        enemyProperty.ResolvedTypeName.Should().Be("myEnemyScript");
        movieScript.Handlers.Should().ContainKey("beginSprite");
        var beginSprite = movieScript.Handlers["beginSprite"];
        beginSprite.Symbol.Name.Should().Be("BeginSprite");
        beginSprite.HandlerKind.Should().Be(BlLingoHandlerKind.Behavior);
        beginSprite.ImpliedScriptKind.Should().Be(BlLingoScriptKind.Behavior);
        beginSprite.HasLeadingMeParameter.Should().BeTrue();

        movieScript.Handlers.Should().ContainKey("mouseDown");
        var mouseDown = movieScript.Handlers["mouseDown"];
        mouseDown.Symbol.Name.Should().Be("MouseDown");
        mouseDown.HandlerKind.Should().Be(BlLingoHandlerKind.Behavior);
        mouseDown.Parameters.Keys.Should().Contain(new[] { "me", "theStage" });
        mouseDown.HasLeadingMeParameter.Should().BeTrue();

        result.Data.Should().ContainKey(BlLingoTypeLinkPass.PendingTypeSymbolsKey);
        result.Data[BlLingoTypeLinkPass.PendingTypeSymbolsKey]
            .Should()
            .BeAssignableTo<IEnumerable<BlCodeSymbol>>()
            .Which.Should().Contain(symbol => symbol.Name == "me");

        result.Data.Should().ContainKey(BlLingoClassLinkPass.KnownClassesKey);
        result.Data[BlLingoClassLinkPass.KnownClassesKey]
            .Should()
            .BeAssignableTo<IEnumerable<string>>()
            .Which.Should().BeEmpty();

        result.Symbols.MovieScript.ScriptKind.Should().Be(BlLingoScriptKind.Movie);
    }

    [Fact]
    public void Analyzer_DetectsParentScript()
    {
        const string script = """
script "Spawner"
property spriteNum

on new me, aSpriteNum
  spriteNum = aSpriteNum
  return me
end

on accelerate me
  -- custom logic
end
""";

        var tokens = _tokenizer.Tokenize(script);
        var analyzer = BlLingoAnalyzer.Create(tokens);
        var result = analyzer.Run();

        result.Symbols.ClassScopes.Should().ContainKey("Spawner");
        var spawner = result.Symbols.ClassScopes["Spawner"];
        spawner.ScriptKind.Should().Be(BlLingoScriptKind.Parent);

        spawner.Handlers.Should().ContainKey("new");
        var constructor = spawner.Handlers["new"];
        constructor.Symbol.Name.Should().Be("New");
        constructor.HandlerKind.Should().Be(BlLingoHandlerKind.Custom);
        constructor.HasLeadingMeParameter.Should().BeTrue();
        constructor.ImpliedScriptKind.Should().Be(BlLingoScriptKind.Parent);

        spawner.Handlers.Should().ContainKey("accelerate");
        var accelerate = spawner.Handlers["accelerate"];
        accelerate.HandlerKind.Should().Be(BlLingoHandlerKind.Custom);
        accelerate.ImpliedScriptKind.Should().Be(BlLingoScriptKind.Unknown);
    }
}
