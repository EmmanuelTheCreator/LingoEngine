using Xunit;

namespace BlingoEngine.Legacy.Lingo.Tests;

public sealed class LegacyHandlerMiscTests : LegacyHandlerTestBase
{
    [Fact]
    public void CommentLine_IsEmittedAsComment()
    {
        const string source = """
on test
  put 1 into value -- this is a comment
end
""";

        var code = GenerateBehavior(source);

        const string expected = """
public class TestScriptBehavior : BlingoSpriteBehavior
{
    public TestScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public void Test()
    {
        value = 1;
        // this is a comment
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }

    [Fact]
    public void ExpressionLine_IsEmittedAsStatement()
    {
        const string source = """
on test
  doStuff()
end
""";

        var code = GenerateBehavior(source);

        const string expected = """
public class TestScriptBehavior : BlingoSpriteBehavior
{
    public TestScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public void Test()
    {
        doStuff();
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }
}
