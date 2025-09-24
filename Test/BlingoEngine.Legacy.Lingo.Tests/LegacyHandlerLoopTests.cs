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
        const string expected = """
public class TestScriptBehavior : BlingoSpriteBehavior
{
    public TestScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public void Test()
    {
        while (cond)
        {
        }
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
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
        const string expected = """
public class TestScriptBehavior : BlingoSpriteBehavior
{
    public TestScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public void Test()
    {
        do
        {
        } while (!(done));
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
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
        const string expected = """
public class TestScriptBehavior : BlingoSpriteBehavior
{
    public TestScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public void Test()
    {
        foreach (var item in list)
        {
        }
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
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
        const string expected = """
public class TestScriptBehavior : BlingoSpriteBehavior
{
    public TestScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public void Test()
    {
        for (int i = 1; i <= count; i++)
        {
        }
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }

    [Fact]
    public void RepeatForever_TranslatesToInfiniteLoop()
    {
        const string source = """
on test
  repeat forever
    put 1 into counter
  end repeat
end
""";

        var code = GenerateBehavior(source);
        const string expected = """
public class TestScriptBehavior : BlingoSpriteBehavior
{
    public TestScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public void Test()
    {
        while (true)
        {
            counter = 1;
        }
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }

    [Fact]
    public void ExitRepeat_EmitsBreakStatement()
    {
        const string source = """
on test
  repeat while keepRunning
    exit repeat
  end repeat
end
""";

        var code = GenerateBehavior(source);
        const string expected = """
public class TestScriptBehavior : BlingoSpriteBehavior
{
    public TestScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public void Test()
    {
        while (keepRunning)
        {
            break;
        }
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }

    [Fact]
    public void ExitRepeatIf_EmitsConditionalBreak()
    {
        const string source = """
on test
  repeat while keepRunning
    exit repeat if done
  end repeat
end
""";

        var code = GenerateBehavior(source);
        const string expected = """
public class TestScriptBehavior : BlingoSpriteBehavior
{
    public TestScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public void Test()
    {
        while (keepRunning)
        {
            if (done) break;
        }
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }

    [Fact]
    public void NextRepeat_EmitsContinueStatement()
    {
        const string source = """
on test
  repeat while keepRunning
    next repeat
  end repeat
end
""";

        var code = GenerateBehavior(source);
        const string expected = """
public class TestScriptBehavior : BlingoSpriteBehavior
{
    public TestScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public void Test()
    {
        while (keepRunning)
        {
            continue;
        }
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }

    [Fact]
    public void NextRepeatIf_EmitsConditionalContinue()
    {
        const string source = """
on test
  repeat while keepRunning
    next repeat if skip
  end repeat
end
""";

        var code = GenerateBehavior(source);
        const string expected = """
public class TestScriptBehavior : BlingoSpriteBehavior
{
    public TestScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public void Test()
    {
        while (keepRunning)
        {
            if (skip) continue;
        }
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }
}
