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
        Assert.Contains("answer = 42;", code);
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
        Assert.Contains("PutTextIntoField(\"Greeting\", \"Hi\");", code);
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
        Assert.Contains("Sprite(3).LocH = 100;", code);
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
        Assert.Contains("myList.SetAt(2, 7);", code);
    }
}
