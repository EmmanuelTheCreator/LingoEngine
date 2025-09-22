using Xunit;

namespace BlingoEngine.Legacy.Lingo.Tests;

public sealed class LegacyHandlerPutTests : LegacyHandlerTestBase
{
    [Fact]
    public void PutIntoVariable_AssignsValue()
    {
        const string source = """
on test
  put 42 into answer
end
""";

        var code = GenerateBehavior(source);
        const string expected = """
public class TestScriptBehavior : BlingoSpriteBehavior
{
    public TestScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public void Test()
    {

        answer = 42;
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }

    [Fact]
    public void PutIntoField_UsesHelper()
    {
        const string source = """
on test
  put "Hi" into field "Greeting"
end
""";

        var code = GenerateBehavior(source);
        const string expected = """
public class TestScriptBehavior : BlingoSpriteBehavior
{
    public TestScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public void Test()
    {

        PutTextIntoField("Greeting", "Hi");
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }

    [Fact]
    public void PutIntoSpriteProperty_AssignsProperty()
    {
        const string source = """
on test
  put 100 into sprite(3).locH
end
""";

        var code = GenerateBehavior(source);
        const string expected = """
public class TestScriptBehavior : BlingoSpriteBehavior
{
    public TestScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public void Test()
    {

        Sprite(3).LocH = 100;
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }

    [Fact]
    public void PutIntoListIndex_UsesSetAt()
    {
        const string source = """
on test
  put 7 into myList[2]
end
""";

        var code = GenerateBehavior(source);
        const string expected = """
public class TestScriptBehavior : BlingoSpriteBehavior
{
    public TestScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public void Test()
    {

        myList.SetAt(2, 7);
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }
}
