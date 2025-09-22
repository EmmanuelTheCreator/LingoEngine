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
        Assert.Contains("if (a != b)", code);
        Assert.Contains("value = 1;", code);
        Assert.Contains("value = 0;", code);
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
        Assert.Contains("if (a > 10)", code);
        Assert.Contains("else if (a <5)", code);
        Assert.Contains("value = 3;", code);
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
        Assert.Contains("switch (state)", code);
        Assert.Contains("case 1:", code);
        Assert.Contains("case 2:", code);
        Assert.Contains("default:", code);
        Assert.Contains("value = null;", code);
    }
}
