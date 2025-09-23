using System.Collections.Generic;
using System.Text;
using Xunit;

namespace BlingoEngine.Legacy.Lingo.Tests;

public sealed class LegacyHandlerPlayerPropertyTests : LegacyHandlerTestBase
{
    public static IEnumerable<object[]> PlayerWritablePropertyCases()
    {
        yield return new object[] { "mediaRequiresAsyncPreload", "MediaRequiresAsyncPreload" };
        yield return new object[] { "safePlayer", "SafePlayer" };
        yield return new object[] { "organizationName", "OrganizationName" };
        yield return new object[] { "applicationName", "ApplicationName" };
        yield return new object[] { "applicationPath", "ApplicationPath" };
        yield return new object[] { "productName", "ProductName" };
        yield return new object[] { "productVersion", "ProductVersion" };
        yield return new object[] { "alertHook", "AlertHook" };
    }

    public static IEnumerable<object[]> PlayerReadablePropertyCases()
    {
        foreach (var writable in PlayerWritablePropertyCases())
        {
            yield return writable;
        }

        yield return new object[] { "activeCastLib", "ActiveCastLib" };
        yield return new object[] { "activeMovie", "ActiveMovie" };
        yield return new object[] { "sound", "Sound" };
        yield return new object[] { "currentSpriteNum", "CurrentSpriteNum" };
        yield return new object[] { "netPreset", "NetPreset" };
        yield return new object[] { "activeWindow", "ActiveWindow" };
        yield return new object[] { "lastClick", "LastClick" };
        yield return new object[] { "lastEvent", "LastEvent" };
        yield return new object[] { "lastKey", "LastKey" };
        yield return new object[] { "castLibs", "CastLibs" };
        yield return new object[] { "stage", "Stage" };
    }

    [Theory]
    [MemberData(nameof(PlayerWritablePropertyCases))]
    public void PlayerPropertyAssignment_UsesPlayerProperty(string lingoProperty, string expectedProperty)
    {
        const string valueExpression = "valueExpr";
        var source = $"""
on test
  put {valueExpression} into the {lingoProperty}
end
""";

        var code = GenerateBehavior(source);
        var expected = CreateBehavior($"_Player.{expectedProperty} = {valueExpression};");
        LegacyCodeAssert.AreEqual(expected, code);
    }

    [Theory]
    [MemberData(nameof(PlayerReadablePropertyCases))]
    public void PlayerPropertyGetter_UsesPlayerProperty(string lingoProperty, string expectedProperty)
    {
        var source = $"""
on test
  put the {lingoProperty} into result
end
""";

        var code = GenerateBehavior(source);
        var expected = CreateBehavior($"result = _Player.{expectedProperty};");
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
