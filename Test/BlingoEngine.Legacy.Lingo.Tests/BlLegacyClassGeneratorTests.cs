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
    public void ParentScript_IncludesGlobalFieldAndConstructor()
    {
        var code = _generator.GenerateClass("MyParent", string.Empty, BlLingoScriptKind.Parent);
        const string expected = """
public class MyParentParent : BlingoParentScript
{
    private readonly GlobalVars _global;

    public MyParentParent(IBlingoMovieEnvironment env, GlobalVars global) : base(env)
    {
        _global = global;
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
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
