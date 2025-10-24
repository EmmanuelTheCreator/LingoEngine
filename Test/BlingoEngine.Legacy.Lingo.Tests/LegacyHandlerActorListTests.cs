using Xunit;

namespace BlingoEngine.Legacy.Lingo.Tests;

public sealed class LegacyHandlerActorListTests : LegacyHandlerTestBase
{
    [Fact]
    public void ActorListAppend_AddsToMovieList()
    {
        const string source = """
on test
  the actorList.append(me)
end
""";

        var code = GenerateBehavior(source);
        const string expected = """
public class TestScriptBehavior : BlingoSpriteBehavior
{
    public TestScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public void Test()
    {
        _Movie.ActorList.Append(this);
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }

    [Fact]
    public void ActorListAppendCommand_AddsToMovieList()
    {
        const string source = """
on test
  append the actorList(me)
end
""";

        var code = GenerateBehavior(source);
        const string expected = """
public class TestScriptBehavior : BlingoSpriteBehavior
{
    public TestScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public void Test()
    {
        _Movie.ActorList.Append(this);
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }

    [Fact]
    public void ActorListDeleteOne_RemovesFromMovieList()
    {
        const string source = """
on test
  the actorList.deleteOne(me)
end
""";

        var code = GenerateBehavior(source);
        const string expected = """
public class TestScriptBehavior : BlingoSpriteBehavior
{
    public TestScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public void Test()
    {
        _Movie.ActorList.DeleteOne(this);
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }

    [Fact]
    public void ActorListDeleteOneCommand_RemovesFromMovieList()
    {
        const string source = """
on test
  deleteOne the actorList(me)
end
""";

        var code = GenerateBehavior(source);
        const string expected = """
public class TestScriptBehavior : BlingoSpriteBehavior
{
    public TestScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public void Test()
    {
        _Movie.ActorList.DeleteOne(this);
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }
}
