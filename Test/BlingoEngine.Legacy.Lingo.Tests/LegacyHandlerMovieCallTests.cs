using BlingoEngine.Legacy.Lingo.Analysis;
using BlingoEngine.Legacy.Lingo.CodeGen;
using Xunit;

namespace BlingoEngine.Legacy.Lingo.Tests;

public sealed class LegacyHandlerMovieCallTests
{
    [Fact]
    public void UnknownMovieHandler_CallsMovieScript()
    {
        const string movieSource = """
on myMovieHandler
  return 42
end
""";
        const string behaviorSource = """
on beginSprite
  myMovieHandler
end
""";

        var generator = new BlLegacyClassGenerator();
        var movieCode = generator.GenerateClass("M1", movieSource, BlLingoScriptKind.Movie);
        var behaviorCode = generator.GenerateClass("P1", behaviorSource, BlLingoScriptKind.Behavior);

        const string expectedBehavior = """
public class P1Behavior : BlingoSpriteBehavior, IHasBeginSpriteEvent
{
    public P1Behavior(IBlingoMovieEnvironment env) : base(env) { }
    public void BeginSprite()
    {
        CallMovieScript<MyMovieMovieScript>(movie => movie.myMovieHandler());
    }
}
""";

        const string expectedMovie = """
public class M1MovieScript : BlingoMovieScript
{
    private readonly GlobalVars _global;

    public M1MovieScript(IBlingoMovieEnvironment env, GlobalVars global) : base(env)
    {
        _global = global;
    }
    public int MyMovieHandler()
    {
        return 42;
    }
}
""";

        LegacyCodeAssert.AreEqual(expectedBehavior, behaviorCode);
        LegacyCodeAssert.AreEqual(expectedMovie, movieCode);
    }

    [Fact]
    public void UnknownMovieHandler_ReturnsValueFromMovieScript()
    {
        const string movieSource = """
on myMovieHandler
  return 42
end
""";
        const string behaviorSource = """
on beginSprite
  put myMovieHandler() into result
end
""";

        var generator = new BlLegacyClassGenerator();
        var movieCode = generator.GenerateClass("M1", movieSource, BlLingoScriptKind.Movie);
        var behaviorCode = generator.GenerateClass("P1", behaviorSource, BlLingoScriptKind.Behavior);

        const string expectedBehavior = """
public class P1Behavior : BlingoSpriteBehavior, IHasBeginSpriteEvent
{
    public P1Behavior(IBlingoMovieEnvironment env) : base(env) { }
    public void BeginSprite()
    {
        result = CallMovieScript<MyMovieMovieScript, int>(movie => movie.myMovieHandler());
    }
}
""";

        const string expectedMovie = """
public class M1MovieScript : BlingoMovieScript
{
    private readonly GlobalVars _global;

    public M1MovieScript(IBlingoMovieEnvironment env, GlobalVars global) : base(env)
    {
        _global = global;
    }
    public int MyMovieHandler()
    {
        return 42;
    }
}
""";

        LegacyCodeAssert.AreEqual(expectedBehavior, behaviorCode);
        LegacyCodeAssert.AreEqual(expectedMovie, movieCode);
    }

    [Fact]
    public void UnknownMovieHandler_AssignmentUsesCallResult()
    {
        const string movieSource = """
on myMovieHandler
  return 42
end
""";
        const string behaviorSource = """
on beginSprite
  result = myMovieHandler()
end
""";

        var generator = new BlLegacyClassGenerator();
        var movieCode = generator.GenerateClass("M1", movieSource, BlLingoScriptKind.Movie);
        var behaviorCode = generator.GenerateClass("P1", behaviorSource, BlLingoScriptKind.Behavior);

        const string expectedBehavior = """
public class P1Behavior : BlingoSpriteBehavior, IHasBeginSpriteEvent
{
    public P1Behavior(IBlingoMovieEnvironment env) : base(env) { }
    public void BeginSprite()
    {
        result = CallMovieScript<MyMovieMovieScript, int>(movie => movie.myMovieHandler());
    }
}
""";

        const string expectedMovie = """
public class M1MovieScript : BlingoMovieScript
{
    private readonly GlobalVars _global;

    public M1MovieScript(IBlingoMovieEnvironment env, GlobalVars global) : base(env)
    {
        _global = global;
    }
    public int MyMovieHandler()
    {
        return 42;
    }
}
""";

        LegacyCodeAssert.AreEqual(expectedBehavior, behaviorCode);
        LegacyCodeAssert.AreEqual(expectedMovie, movieCode);
    }

    [Fact]
    public void MovieHandlerFallback_DerivesTypeFromHandlerName()
    {
        const string behaviorSource = """
on beginSprite
  unknownHandler()
end
""";

        var generator = new BlLegacyClassGenerator();
        var behaviorCode = generator.GenerateClass("P1", behaviorSource, BlLingoScriptKind.Behavior);

        const string expectedBehavior = """
public class P1Behavior : BlingoSpriteBehavior, IHasBeginSpriteEvent
{
    public P1Behavior(IBlingoMovieEnvironment env) : base(env) { }
    public void BeginSprite()
    {
        CallMovieScript<UnknownMovieScript>(movie => movie.unknownHandler());
    }
}
""";

        LegacyCodeAssert.AreEqual(expectedBehavior, behaviorCode);
    }
}
