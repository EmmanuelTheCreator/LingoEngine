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
    public void IfVoidPThenAlert_TranslatesToNullCheck()
    {
        const string source = """
on test
  if VoidP(key1) then alert "wrong keys"
end
""";

        var code = GenerateBehavior(source);

        const string expected = """
public class TestScriptBehavior : BlingoSpriteBehavior
{
    public TestScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public void Test()
    {
        if (key1 == null)
        {
            _Player.Alert("wrong keys");
        }
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }

    [Fact]
    public void IfObjectPThenPut_TranslatesToObjectTypeCheck()
    {
        const string source = """
on test
  if objectP(target) then
    put 1 into value
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
        if (target is BlingoEngine.Core.IBlingoScriptBase or BlingoEngine.Xtras.IBlingoXtra or BlingoEngine.Core.IBlingoWindow)
        {
            value = 1;
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
    public void InlineIfWithSendSprite_TranslatesKeyPressedChecks()
    {
        const string source = """
on keyDown me
  if keyPressed(35) then sendSprite myTargetSprite, #PauseGame
  if keyPressed(49) then sendSprite myTargetSprite, #SpaceBar
end
""";

        var code = GenerateBehavior(source);

        const string expected = """
public class TestScriptBehavior : BlingoSpriteBehavior, IHasKeyDownEvent
{
    public TestScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public void KeyDown()
    {
        if (_Key.KeyPressed(35))
        {
            SendSprite<PauseGameBehavior>(myTargetSprite, sprite => sprite.PauseGame());
        }
        if (_Key.KeyPressed(49))
        {
            SendSprite<SpaceBarBehavior>(myTargetSprite, sprite => sprite.SpaceBar());
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

    [Fact]
    public void NestedConditionals_TranslateWithinLoops()
    {
        const string source = """
on test
  if flag then
    repeat with i = 1 to limit
      repeat with item in items
        if item > threshold then
          put item into total
        else
          put 0 into total
        end if
      end repeat
    end repeat
  else
    put 0 into total
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
        if (flag)
        {
            for (int i = 1; i <= limit; i++)
            {
                foreach (var item in items)
                {
                    if (item > threshold)
                    {
                        total = item;
                    }
                    else
                    {
                        total = 0;
                    }
                }
            }
        }
        else
        {
            total = 0;
        }
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }
}
