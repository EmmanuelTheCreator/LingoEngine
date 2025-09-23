using Xunit;

namespace BlingoEngine.Legacy.Lingo.Tests;

public sealed class LegacyHandlerSendSpriteTests : LegacyHandlerTestBase
{
    [Fact]
    public void SendSpriteWithoutArguments_UsesBehaviorType()
    {
        const string source = """
on beginSprite
  sendSprite 2, #doIt
end
""";

        var code = GenerateBehavior(source);
        const string expected = """
public class TestScriptBehavior : BlingoSpriteBehavior, IHasBeginSpriteEvent
{
    public TestScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public void BeginSprite()
    {
        SendSprite<B2Behavior>(2, b2behavior => b2behavior.doIt());
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }

    [Fact]
    public void SendSpriteWithArgument_PropagatesValue()
    {
        const string source = """
on beginSprite
  sendSprite 2, #doIt, 42
end
""";

        var code = GenerateBehavior(source);
        const string expected = """
public class TestScriptBehavior : BlingoSpriteBehavior, IHasBeginSpriteEvent
{
    public TestScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public void BeginSprite()
    {
        SendSprite<B2Behavior>(2, b2behavior => b2behavior.doIt(42));
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }
}
