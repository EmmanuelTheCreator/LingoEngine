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
        Assert.Contains("public class MyBehaviorBehavior : BlingoSpriteBehavior", code);
        Assert.Contains("public MyBehaviorBehavior(IBlingoMovieEnvironment env) : base(env) { }", code);
    }

    [Fact]
    public void ParentScript_IncludesGlobalFieldAndConstructor()
    {
        var code = _generator.GenerateClass("MyParent", string.Empty, BlLingoScriptKind.Parent);
        Assert.Contains("private readonly GlobalVars _global;", code);
        Assert.Contains("public MyParentParent(IBlingoMovieEnvironment env, GlobalVars global) : base(env)", code);
        Assert.Contains("_global = global;", code);
    }

    [Fact]
    public void PropertyDescriptionList_AddsInterface()
    {
        const string source = "on getPropertyDescriptionList\nend";
        var code = _generator.GenerateClass("MyBehavior", source, BlLingoScriptKind.Behavior);
        Assert.Contains(
            "public class MyBehaviorBehavior : BlingoSpriteBehavior, IBlingoPropertyDescriptionList",
            code);
        Assert.Contains("public BehaviorPropertyDescriptionList? GetPropertyDescriptionList()", code);
    }

    [Fact]
    public void EventHandler_AddsEventInterface()
    {
        const string source = "on beginSprite me\nend";
        var code = _generator.GenerateClass("MyBehavior", source, BlLingoScriptKind.Behavior);
        Assert.Contains("IHasBeginSpriteEvent", code);
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
        Assert.Contains("public class MyBehaviorBeh : BlingoSpriteBehavior", code);
    }

    [Fact]
    public void Handler_IsConvertedToMethod()
    {
        const string source = "on mouseUp me, btn\n  put 1 into value\nend";
        var code = _generator.GenerateClass("MyBehavior", source, BlLingoScriptKind.Behavior);

        Assert.Contains("public void MouseUp(object? btn)", code);
        Assert.DoesNotContain("object? me", code);
        Assert.Contains("value = 1;", code);
    }

}
