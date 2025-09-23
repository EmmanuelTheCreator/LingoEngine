using BlingoEngine.Legacy.Lingo.Analysis;
using BlingoEngine.Legacy.Lingo.CodeGen;
using Xunit;

namespace BlingoEngine.Legacy.Lingo.Tests;

public sealed class LegacyHandlerSendSpriteTests : LegacyHandlerTestBase
{
    [Fact]
    public void SendSpriteWithoutArguments_UsesBehaviorType()
    {
        const string callerSource = """
on beginSprite
  sendSprite 2, #doIt
end
""";
        const string calleeSource = """
on doIt
end
""";

        var generator = new BlLegacyClassGenerator();
        var callerCode = generator.GenerateClass("B1", callerSource, BlLingoScriptKind.Behavior);
        var calleeCode = generator.GenerateClass("B2", calleeSource, BlLingoScriptKind.Behavior);

        const string expectedCaller = """
public class B1Behavior : BlingoSpriteBehavior, IHasBeginSpriteEvent
{
    public B1Behavior(IBlingoMovieEnvironment env) : base(env) { }
    public void BeginSprite()
    {
        SendSprite<B2Behavior>(2, b2behavior => b2behavior.doIt());
    }
}
""";

        const string expectedCallee = """
public class B2Behavior : BlingoSpriteBehavior
{
    public B2Behavior(IBlingoMovieEnvironment env) : base(env) { }
    public void DoIt()
    {
    }
}
""";

        LegacyCodeAssert.AreEqual(expectedCaller, callerCode);
        LegacyCodeAssert.AreEqual(expectedCallee, calleeCode);
    }

    [Fact]
    public void SendSpriteWithArgument_PropagatesValue()
    {
        const string callerSource = """
on beginSprite
  sendSprite 2, #doIt, 42
end
""";
        const string calleeSource = """
on doIt me, value
end
""";

        var generator = new BlLegacyClassGenerator();
        var callerCode = generator.GenerateClass("B1", callerSource, BlLingoScriptKind.Behavior);
        var calleeCode = generator.GenerateClass("B2", calleeSource, BlLingoScriptKind.Behavior);

        const string expectedCaller = """
public class B1Behavior : BlingoSpriteBehavior, IHasBeginSpriteEvent
{
    public B1Behavior(IBlingoMovieEnvironment env) : base(env) { }
    public void BeginSprite()
    {
        SendSprite<B2Behavior>(2, b2behavior => b2behavior.doIt(42));
    }
}
""";

        const string expectedCallee = """
public class B2Behavior : BlingoSpriteBehavior
{
    public B2Behavior(IBlingoMovieEnvironment env) : base(env) { }
    public void DoIt(object? value)
    {
    }
}
""";

        LegacyCodeAssert.AreEqual(expectedCaller, callerCode);
        LegacyCodeAssert.AreEqual(expectedCallee, calleeCode);
    }

    [Fact]
    public void SendSpriteWithoutResolvedScript_UsesHandlerFallback()
    {
        const string callerSource = """
on beginSprite
  sendSprite targetSprite, #doIt
end
""";

        var generator = new BlLegacyClassGenerator();
        var callerCode = generator.GenerateClass("B1", callerSource, BlLingoScriptKind.Behavior);

        const string expectedCaller = """
public class B1Behavior : BlingoSpriteBehavior, IHasBeginSpriteEvent
{
    public B1Behavior(IBlingoMovieEnvironment env) : base(env) { }
    public void BeginSprite()
    {
        SendSprite<DoItBehavior>(targetSprite, sprite => sprite.doIt());
    }
}
""";

        LegacyCodeAssert.AreEqual(expectedCaller, callerCode);
    }

    [Fact]
    public void SendSpriteReturningValue_AssignsResult()
    {
        const string callerSource = """
on beginSprite
  put sendSprite 2, #getValue into result
end
""";
        const string calleeSource = """
on getValue
  return 5
end
""";

        var generator = new BlLegacyClassGenerator();
        var callerCode = generator.GenerateClass("B1", callerSource, BlLingoScriptKind.Behavior);
        var calleeCode = generator.GenerateClass("B2", calleeSource, BlLingoScriptKind.Behavior);

        const string expectedCaller = """
public class B1Behavior : BlingoSpriteBehavior, IHasBeginSpriteEvent
{
    public B1Behavior(IBlingoMovieEnvironment env) : base(env) { }
    public void BeginSprite()
    {
        result = SendSprite<B2Behavior, object?>(2, b2behavior => b2behavior.getValue());
    }
}
""";

        const string expectedCallee = """
public class B2Behavior : BlingoSpriteBehavior
{
    public B2Behavior(IBlingoMovieEnvironment env) : base(env) { }
    public void GetValue()
    {
        return 5;
    }
}
""";

        LegacyCodeAssert.AreEqual(expectedCaller, callerCode);
        LegacyCodeAssert.AreEqual(expectedCallee, calleeCode);
    }
}
