using System.Collections.Generic;
using System.Text;
using Xunit;

namespace BlingoEngine.Legacy.Lingo.Tests;

public sealed class LegacyHandlerSprite2DPropertyTests : LegacyHandlerTestBase
{
    public static IEnumerable<object[]> SpritePropertyCases()
    {
        yield return new object[] { "margin", "Margin" };
        yield return new object[] { "name", "Name" };
        yield return new object[] { "memberNum", "MemberNum" };
        yield return new object[] { "displayMember", "DisplayMember" };
        yield return new object[] { "spritePropertiesOffset", "SpritePropertiesOffset" };
        yield return new object[] { "inkType", "InkType" };
        yield return new object[] { "ink", "Ink" };
        yield return new object[] { "visibility", "Visibility" };
        yield return new object[] { "hilite", "Hilite" };
        yield return new object[] { "blend", "Blend" };
        yield return new object[] { "locH", "LocH" };
        yield return new object[] { "locV", "LocV" };
        yield return new object[] { "locZ", "LocZ" };
        yield return new object[] { "loc", "Loc" };
        yield return new object[] { "rotation", "Rotation" };
        yield return new object[] { "skew", "Skew" };
        yield return new object[] { "flipH", "FlipH" };
        yield return new object[] { "flipV", "FlipV" };
        yield return new object[] { "top", "Top" };
        yield return new object[] { "bottom", "Bottom" };
        yield return new object[] { "left", "Left" };
        yield return new object[] { "right", "Right" };
        yield return new object[] { "cursor", "Cursor" };
        yield return new object[] { "constraint", "Constraint" };
        yield return new object[] { "directToStage", "DirectToStage" };
        yield return new object[] { "regPoint", "RegPoint" };
        yield return new object[] { "foreColor", "ForeColor" };
        yield return new object[] { "backColor", "BackColor" };
        yield return new object[] { "editable", "Editable" };
        yield return new object[] { "isDraggable", "IsDraggable" };
        yield return new object[] { "color", "Color" };
        yield return new object[] { "media", "Media" };
        yield return new object[] { "thumbnail", "Thumbnail" };
        yield return new object[] { "modifiedBy", "ModifiedBy" };
        yield return new object[] { "currentTime", "CurrentTime" };
        yield return new object[] { "width", "Width" };
        yield return new object[] { "height", "Height" };
    }

    [Theory]
    [MemberData(nameof(SpritePropertyCases))]
    public void SpritePropertyAssignment_UsesPascalCase(string lingoProperty, string expectedProperty)
    {
        const string valueExpression = "valueExpr";
        var source = $"""
on test
  sprite(5).{lingoProperty} = {valueExpression}
end
""";

        var code = GenerateBehavior(source);
        var expected = CreateBehavior($"Sprite(5).{expectedProperty} = {valueExpression};");
        LegacyCodeAssert.AreEqual(expected, code);
    }

    [Theory]
    [MemberData(nameof(SpritePropertyCases))]
    public void SpritePropertyGetter_UsesPascalCase(string lingoProperty, string expectedProperty)
    {
        var source = $"""
on test
  put sprite(5).{lingoProperty} into result
end
""";

        var code = GenerateBehavior(source);
        var expected = CreateBehavior($"result = Sprite(5).{expectedProperty};");
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
