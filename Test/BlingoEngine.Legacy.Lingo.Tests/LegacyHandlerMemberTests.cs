using Xunit;

namespace BlingoEngine.Legacy.Lingo.Tests;

public sealed class LegacyHandlerMemberTests : LegacyHandlerTestBase
{
    [Fact]
    public void SpriteMemberAssignment_UsesSetMember()
    {
        const string source = """
on test
  sprite(2).member = member("Name")
end
""";

        var code = GenerateBehavior(source);
        const string expected = """
public class TestScriptBehavior : BlingoSpriteBehavior
{
    public TestScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public void Test()
    {
        Sprite(2).SetMember("Name");
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }

    [Fact]
    public void SpriteMemberAssignment_WithCastLibKeepsArguments()
    {
        const string source = """
on test
  sprite(3).member = member("Title", "CastLib")
end
""";

        var code = GenerateBehavior(source);
        const string expected = """
public class TestScriptBehavior : BlingoSpriteBehavior
{
    public TestScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public void Test()
    {
        Sprite(3).SetMember("Title", "CastLib");
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }

    [Fact]
    public void PutMemberAssignment_UsesSetMember()
    {
        const string source = """
on test
  put member("Marker") into sprite(4).member
end
""";

        var code = GenerateBehavior(source);
        const string expected = """
public class TestScriptBehavior : BlingoSpriteBehavior
{
    public TestScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public void Test()
    {
        Sprite(4).SetMember("Marker");
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }
}
