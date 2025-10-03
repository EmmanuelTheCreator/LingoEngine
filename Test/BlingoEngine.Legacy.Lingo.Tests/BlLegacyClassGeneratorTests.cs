using System;
using BlingoEngine.Legacy.Lingo.Analysis;
using BlingoEngine.Legacy.Lingo.CodeGen;
using Xunit;

namespace BlingoEngine.Legacy.Lingo.Tests;

public sealed class BlLegacyClassGeneratorTests
{
    private readonly BlLegacyClassGenerator _generator = new();

    [Fact]
    public void BehaviorScript_IncludesEnvironmentConstructor()
    {
        var code = _generator.GenerateClass("MyBehavior", string.Empty, BlLingoScriptKind.Behavior);
        const string expected = """
public class MyBehaviorBehavior : BlingoSpriteBehavior
{
    public MyBehaviorBehavior(IBlingoMovieEnvironment env) : base(env) { }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }

    [Fact]
    public void ParentScriptWithoutGlobals_UsesBasicConstructor()
    {
        var code = _generator.GenerateClass("MyParent", string.Empty, BlLingoScriptKind.Parent);
        const string expected = """
public class MyParentParent : BlingoParentScript
{
    public MyParentParent(IBlingoMovieEnvironment env) : base(env) { }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }

    [Fact]
    public void ParentScriptWithGlobals_IncludesGlobalField()
    {
        const string source = "global gValue\n" +
            "on startMovie\n" +
            "  gValue = 1\n" +
            "end";

        var code = _generator.GenerateClass("MyParent", source, BlLingoScriptKind.Parent);
        const string expected = """
public class MyParentParent : BlingoParentScript
{
    private readonly GlobalVars _global;

    public MyParentParent(IBlingoMovieEnvironment env, GlobalVars global) : base(env)
    {
        _global = global;
    }
    public void StartMovie()
    {
        gValue = 1;
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }

    [Fact]
    public void Properties_AreDeclaredWithInferredTypesAndComments()
    {
        const string source = "property myStartX -- start X\n" +
            "property myStartY\n" +
            "on new me\n" +
            "  myStartX = 250\n" +
            "  myStartY = 45\n" +
            "end";

        var code = _generator.GenerateClass("Mover", source, BlLingoScriptKind.Behavior);

        Assert.Contains("public int myStartX { get; set; } // start X", code);
        Assert.Contains("public int myStartY { get; set; }", code);

        var propertyIndex = code.IndexOf("public int myStartX { get; set; } // start X", StringComparison.Ordinal);
        var constructorIndex = code.IndexOf("public MoverBehavior(IBlingoMovieEnvironment env) : base(env)", StringComparison.Ordinal);

        Assert.True(propertyIndex >= 0, "Properties were not generated");
        Assert.True(constructorIndex > propertyIndex, "Constructor should appear after properties");
        Assert.DoesNotContain("public void New()", code);
        Assert.Contains("myStartX = 250;", code);
        Assert.Contains("myStartY = 45;", code);
    }

    [Fact]
    public void Constructor_IncludesNewHandlerParameters()
    {
        const string source = "on new me, _Gfx, ChosenType\nend";
        var code = _generator.GenerateClass("MyParent", source, BlLingoScriptKind.Parent);

        Assert.Contains(
            "public MyParentParent(IBlingoMovieEnvironment env, object _Gfx, object ChosenType) : base(env)",
            code);
        Assert.DoesNotContain("public void New(object _Gfx, object ChosenType)", code);
    }

    [Fact]
    public void ExitFrameWithMeParameter_UsesCurrentFrame()
    {
        const string source = "on exitFrame me\n  go to the frame\nend";
        var code = _generator.GenerateClass("Stopper", source, BlLingoScriptKind.Behavior);

        Assert.Contains("_movie.GoTo(_movie.CurrentFrame);", code);
    }

    [Fact]
    public void GoToLabel_UsesLowercaseMovieField()
    {
        const string source = "on exitFrame\n  go to \"Game\"\nend";
        var code = _generator.GenerateClass("Navigator", source, BlLingoScriptKind.Behavior);

        Assert.Contains("_movie.GoTo(\"Game\");", code);
    }

    [Fact]
    public void ReturnKeywordInExpression_TranslatesToEnvironmentNewLine()
    {
        const string source = """
on updateScores i
  member("T_InternetScoresNames").text = member("T_InternetScoresNames").text & i[1] & return
end
""";

        var code = _generator.GenerateClass("ScoreKeeper", source, BlLingoScriptKind.Parent);

        Assert.Contains("Environment.NewLine", code);
        Assert.Contains(
            "Member<IBlingoMemberTextBase>(\"T_InternetScoresNames\").Text = Member<IBlingoMemberTextBase>(\"T_InternetScoresNames\").Text + i[1] + Environment.NewLine;",
            code);
    }

    [Fact]
    public void Properties_InferTypesFromMemberAccess()
    {
        const string source = """
property key1
property key2
property key3
on beginSprite
  key1 = member("parameters").line[21]
  key2 = member("parameters").line[21].word[2]
  key3 = value(member("parameters").line[21].word[2])
end
""";

        var code = _generator.GenerateClass("KeyReader", source, BlLingoScriptKind.Behavior);

        Assert.Contains("public string key1 { get; set; }", code);
        Assert.Contains("public string key2 { get; set; }", code);
        Assert.Contains("public int key3 { get; set; }", code);
        Assert.Contains(
            "key3 = Convert.ToInt32(Member<IBlingoMemberTextBase>(\"parameters\").Line[21].Word[2]);",
            code);
    }

    [Fact]
    public void Handlers_AppearInSourceOrder()
    {
        const string source = "on startMovie\nend\n" +
            "on stopMovie\nend";

        var code = _generator.GenerateClass("Order", source, BlLingoScriptKind.Movie);
        var startIndex = code.IndexOf("public void StartMovie", StringComparison.Ordinal);
        var stopIndex = code.IndexOf("public void StopMovie", StringComparison.Ordinal);

        Assert.True(startIndex >= 0, "StartMovie handler not found");
        Assert.True(stopIndex >= 0, "StopMovie handler not found");
        Assert.True(startIndex < stopIndex, "Handlers are not in source order");
    }

    [Fact]
    public void PropertyDescriptionList_AddsInterface()
    {
        const string source = "on getPropertyDescriptionList\nend";
        var code = _generator.GenerateClass("MyBehavior", source, BlLingoScriptKind.Behavior);
        const string expected = """
public class MyBehaviorBehavior : BlingoSpriteBehavior, IBlingoPropertyDescriptionList
{
    public MyBehaviorBehavior(IBlingoMovieEnvironment env) : base(env) { }

    public BehaviorPropertyDescriptionList? GetPropertyDescriptionList()
    {
        return null;
    }

    public string? GetBehaviorDescription() => null;

    public string? GetBehaviorTooltip() => null;

    public bool IsOKToAttach(BlingoSymbol spriteType, int spriteNum) => true;
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }

    [Fact]
    public void EventHandler_AddsEventInterface()
    {
        const string source = "on beginSprite me\nend";
        var code = _generator.GenerateClass("MyBehavior", source, BlLingoScriptKind.Behavior);
        const string expected = """
public class MyBehaviorBehavior : BlingoSpriteBehavior, IHasBeginSpriteEvent
{
    public MyBehaviorBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public void BeginSprite()
    {
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }

    [Fact]
    public void CustomSuffixes_AreApplied()
    {
        var options = new BlLegacyClassGeneratorOptions
        {
            BehaviorSuffix = "Beh",
            ParentSuffix = "Par",
            MovieScriptSuffix = "Movie",
            ScriptSuffix = "Script",
        };

        var generator = new BlLegacyClassGenerator(options);
        var code = generator.GenerateClass("MyBehavior", string.Empty, BlLingoScriptKind.Behavior);
        const string expected = """
public class MyBehaviorBeh : BlingoSpriteBehavior
{
    public MyBehaviorBeh(IBlingoMovieEnvironment env) : base(env) { }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }

    [Fact]
    public void Handler_IsConvertedToMethod()
    {
        const string source = "on mouseUp me, btn\n  put 1 into value\nend";
        var code = _generator.GenerateClass("MyBehavior", source, BlLingoScriptKind.Behavior);
        const string expected = """
public class MyBehaviorBehavior : BlingoSpriteBehavior, IHasMouseUpEvent
{
    public MyBehaviorBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public void MouseUp(BlingoMouseEvent btn)
    {
        value = 1;
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }

    [Fact]
    public void LeadingComments_ArePreservedBeforeClassDeclaration()
    {
        const string source = "-- Header comment\n-- Second line\non startMovie\nend";
        var code = _generator.GenerateClass("MyBehavior", source, BlLingoScriptKind.Behavior);

        const string expectedPrefix = """
// Header comment
// Second line

public class MyBehaviorBehavior : BlingoSpriteBehavior
""";

        Assert.StartsWith(expectedPrefix.ReplaceLineEndings(Environment.NewLine), code);
    }

    [Fact]
    public void CommentsBetweenHandlers_ArePreserved()
    {
        const string source = "-- Header\n" +
            "on mouseUp\n" +
            "end\n" +
            "\n" +
            "-- Between handlers\n" +
            "on mouseDown\n" +
            "end";

        var code = _generator.GenerateClass("MyBehavior", source, BlLingoScriptKind.Behavior);

        var commentSnippet = "    // Between handlers" + Environment.NewLine + Environment.NewLine + "    public void MouseDown";
        Assert.Contains(commentSnippet.ReplaceLineEndings(Environment.NewLine), code);
    }
}
