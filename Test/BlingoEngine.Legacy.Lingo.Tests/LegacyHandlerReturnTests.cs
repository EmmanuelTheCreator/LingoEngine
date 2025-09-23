using BlingoEngine.Legacy.Lingo.Analysis;
using BlingoEngine.Legacy.Lingo.CodeGen;
using Xunit;

namespace BlingoEngine.Legacy.Lingo.Tests;

public sealed class LegacyHandlerReturnTests
{
    [Fact]
    public void ExitStatement_EmitsReturn()
    {
        const string source = """
on exitHandler
  exit
end
""";

        var generator = new BlLegacyClassGenerator();
        var code = generator.GenerateClass("ExitScript", source, BlLingoScriptKind.Behavior);
        const string expected = """
public class ExitScriptBehavior : BlingoSpriteBehavior
{
    public ExitScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public void ExitHandler()
    {
        return;
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }

    [Fact]
    public void ReturnStatementWithoutValue_EmitsReturn()
    {
        const string source = """
on returnVoid
  return
end
""";

        var generator = new BlLegacyClassGenerator();
        var code = generator.GenerateClass("ReturnVoidScript", source, BlLingoScriptKind.Behavior);
        const string expected = """
public class ReturnVoidScriptBehavior : BlingoSpriteBehavior
{
    public ReturnVoidScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public void ReturnVoid()
    {
        return;
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }

    [Fact]
    public void ReturnStatementWithValue_InferReturnType()
    {
        const string source = """
on computeValue
  return 5
end
""";

        var generator = new BlLegacyClassGenerator();
        var code = generator.GenerateClass("ReturnValueScript", source, BlLingoScriptKind.Behavior);
        const string expected = """
public class ReturnValueScriptBehavior : BlingoSpriteBehavior
{
    public ReturnValueScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public int ComputeValue()
    {
        return 5;
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }
}
