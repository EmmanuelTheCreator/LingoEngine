using System.Collections.Generic;
using System.Text;
using Xunit;

namespace BlingoEngine.Legacy.Lingo.Tests;

public sealed class LegacyHandlerMoviePropertyTests : LegacyHandlerTestBase
{
    public static IEnumerable<object[]> MovieWritablePropertyCases()
    {
        yield return new object[] { "tempo", "Tempo" };
        yield return new object[] { "about", "About" };
        yield return new object[] { "copyright", "Copyright" };
        yield return new object[] { "userName", "UserName" };
        yield return new object[] { "companyName", "CompanyName" };
        yield return new object[] { "maxSpriteChannelCount", "MaxSpriteChannelCount" };
    }

    public static IEnumerable<object[]> MovieReadablePropertyCases()
    {
        foreach (var writable in MovieWritablePropertyCases())
        {
            yield return writable;
        }

        yield return new object[] { "frame", "Frame" };
        yield return new object[] { "currentFrame", "CurrentFrame" };
        yield return new object[] { "frameCount", "FrameCount" };
        yield return new object[] { "isPlaying", "IsPlaying" };
        yield return new object[] { "timer", "Timer" };
        yield return new object[] { "spriteTotalCount", "SpriteTotalCount" };
        yield return new object[] { "spriteMaxNumber", "SpriteMaxNumber" };
        yield return new object[] { "lastChannel", "LastChannel" };
        yield return new object[] { "lastFrame", "LastFrame" };
        yield return new object[] { "markerList", "MarkerList" };
        yield return new object[] { "actorList", "ActorList" };
        yield return new object[] { "timeOutList", "TimeOutList" };
        yield return new object[] { "castLib", "CastLib" };
        yield return new object[] { "number of score", "Number" };
    }

    [Theory]
    [MemberData(nameof(MovieWritablePropertyCases))]
    public void MoviePropertyAssignment_UsesMovieProperty(string lingoProperty, string expectedProperty)
    {
        const string valueExpression = "valueExpr";
        var source = $"""
on test
  put {valueExpression} into the {lingoProperty}
end
""";

        var code = GenerateBehavior(source);
        var expected = CreateBehavior($"_Movie.{expectedProperty} = {valueExpression};");
        LegacyCodeAssert.AreEqual(expected, code);
    }

    [Theory]
    [MemberData(nameof(MovieReadablePropertyCases))]
    public void MoviePropertyGetter_UsesMovieProperty(string lingoProperty, string expectedProperty)
    {
        var source = $"""
on test
  put the {lingoProperty} into result
end
""";

        var code = GenerateBehavior(source);
        var expected = CreateBehavior($"result = _Movie.{expectedProperty};");
        LegacyCodeAssert.AreEqual(expected, code);
    }

    private static string CreateBehavior(string statement)
    {
        var builder = new StringBuilder();
        builder.AppendLine("public class TestScriptBehavior : BlingoSpriteBehavior");
        builder.AppendLine("{");
        builder.AppendLine("    public TestScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }");
        builder.AppendLine("    public void Test()");
        builder.AppendLine("    {");
        builder.Append("        ");
        builder.AppendLine(statement);
        builder.AppendLine("    }");
        builder.AppendLine("}");
        return builder.ToString();
    }
}
