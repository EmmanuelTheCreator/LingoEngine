using Xunit;

namespace BlingoEngine.Legacy.Lingo.Tests;

public sealed class LegacyHandlerLoopTests : LegacyHandlerTestBase
{
    [Fact]
    public void RepeatWhile_TranslatesToWhileLoop()
    {
        const string source = """
on test
  repeat while cond
end repeat
end
""";

        var code = GenerateBehavior(source);
        Assert.Contains("while (cond)", code);
    }

    [Fact]
    public void RepeatUntil_TranslatesToDoWhileLoop()
    {
        const string source = """
on test
  repeat until done
end repeat
end
""";

        var code = GenerateBehavior(source);
        Assert.Contains("do", code);
        Assert.Contains("} while (!(done));", code);
    }

    [Fact]
    public void RepeatWithIn_TranslatesToForeachLoop()
    {
        const string source = """
on test
  repeat with item in list
end repeat
end
""";

        var code = GenerateBehavior(source);
        Assert.Contains("foreach (var item in list)", code);
    }

    [Fact]
    public void RepeatWithRange_TranslatesToForLoop()
    {
        const string source = """
on test
  repeat with i = 1 to count
end repeat
end
""";

        var code = GenerateBehavior(source);
        Assert.Contains("for (int i = 1; i <= count; i++)", code);
    }
}
