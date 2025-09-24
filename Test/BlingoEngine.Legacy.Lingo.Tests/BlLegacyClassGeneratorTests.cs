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
        var constructorIndex = code.IndexOf("public MoverBehavior(IBlingoMovieEnvironment env) : base(env) { }", StringComparison.Ordinal);
        var handlerIndex = code.IndexOf("public void New()", StringComparison.Ordinal);

        Assert.True(propertyIndex >= 0, "Properties were not generated");
        Assert.True(constructorIndex > propertyIndex, "Constructor should appear after properties");
        Assert.True(handlerIndex > constructorIndex, "Handler should appear after constructor");
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
    public void MouseUp(object? btn)
    {
        value = 1;
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }

}
