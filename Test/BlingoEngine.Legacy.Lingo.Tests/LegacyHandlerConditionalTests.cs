using Xunit;

namespace BlingoEngine.Legacy.Lingo.Tests;

public sealed class LegacyHandlerConditionalTests : LegacyHandlerTestBase
{
    [Fact]
    public void IfThenElse_TranslatesComparison()
    {
        const string source = """
on test
  if a <> b then
    put 1 into value
  else
    put 0 into value
  end if
end
""";

        var code = GenerateBehavior(source);
        const string expected = """
public class TestScriptBehavior : BlingoSpriteBehavior
{
    public TestScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public void Test()
    {
        if (a != b)
        {
            value = 1;
        }
        else
        {
            value = 0;
        }
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }

    [Fact]
    public void ElseIf_BecomesElseIf()
    {
        const string source = """
on test
  if a > 10 then
    put 1 into value
  else if a < 5 then
    put 2 into value
  else
    put 3 into value
  end if
end
""";

        var code = GenerateBehavior(source);
        const string expected = """
public class TestScriptBehavior : BlingoSpriteBehavior
{
    public TestScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public void Test()
    {
        if (a > 10)
        {
            value = 1;
        }
        else if (a <5)
        {
            value = 2;
        }
        else
        {
            value = 3;
        }
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }

    [Fact]
    public void CaseStatement_TranslatesToSwitch()
    {
        const string source = """
on test
  case state of
    when 1
      put 1 into value
    when 2
      put 2 into value
    otherwise
      put void into value
  end case
end
""";

        var code = GenerateBehavior(source);
        const string expected = """
public class TestScriptBehavior : BlingoSpriteBehavior
{
    public TestScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public void Test()
    {
        switch (state)
        {
            case 1:
                value = 1;
                break;
            case 2:
                value = 2;
                break;
            default:
                value = null;
                break;
        }
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }
}
