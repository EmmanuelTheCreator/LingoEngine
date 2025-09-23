using System.Collections.Generic;
using System.Text;
using Xunit;

namespace BlingoEngine.Legacy.Lingo.Tests;

public sealed class LegacyHandlerMemberPropertyTests : LegacyHandlerTestBase
{
    public static IEnumerable<object[]> MemberPropertyCases()
    {
        yield return new object[] { "name", "Name", string.Empty, string.Empty, true };
        yield return new object[] { "purgePriority", "PurgePriority", string.Empty, string.Empty, true };
        yield return new object[] { "regPoint", "RegPoint", string.Empty, string.Empty, true };
        yield return new object[] { "width", "Width", string.Empty, string.Empty, true };
        yield return new object[] { "height", "Height", string.Empty, string.Empty, true };
        yield return new object[] { "size", "Size", string.Empty, string.Empty, true };
        yield return new object[] { "comments", "Comments", string.Empty, string.Empty, true };
        yield return new object[] { "fileName", "FileName", string.Empty, string.Empty, true };

        yield return new object[] { "text", "Text", string.Empty, "IBlingoMemberTextBase", true };
        yield return new object[] { "line", "Line", "[2]", "IBlingoMemberTextBase", true };
        yield return new object[] { "word", "Word", "[3]", "IBlingoMemberTextBase", true };
        yield return new object[] { "char", "Char", "[1]", "IBlingoMemberTextBase", true };
        yield return new object[] { "editable", "Editable", string.Empty, "IBlingoMemberTextBase", true };
        yield return new object[] { "wordWrap", "WordWrap", string.Empty, "IBlingoMemberTextBase", true };
        yield return new object[] { "scrollTop", "ScrollTop", string.Empty, "IBlingoMemberTextBase", true };
        yield return new object[] { "textFont", "Font", string.Empty, "IBlingoMemberTextBase", true };
        yield return new object[] { "textSize", "FontSize", string.Empty, "IBlingoMemberTextBase", true };
        yield return new object[] { "textStyle", "FontStyle", string.Empty, "IBlingoMemberTextBase", true };
        yield return new object[] { "textColor", "Color", string.Empty, "IBlingoMemberTextBase", true };
        yield return new object[] { "bold", "Bold", string.Empty, "IBlingoMemberTextBase", true };
        yield return new object[] { "italic", "Italic", string.Empty, "IBlingoMemberTextBase", true };
        yield return new object[] { "underline", "Underline", string.Empty, "IBlingoMemberTextBase", true };
        yield return new object[] { "alignment", "Alignment", string.Empty, "IBlingoMemberTextBase", true };
        yield return new object[] { "margin", "Margin", string.Empty, "IBlingoMemberTextBase", true };

        yield return new object[] { "loop", "Loop", string.Empty, "BlingoMemberSound", true };
        yield return new object[] { "linked", "IsLinked", string.Empty, "BlingoMemberSound", true };
        yield return new object[] { "isLinked", "IsLinked", string.Empty, "BlingoMemberSound", true };
        yield return new object[] { "linkedFilePath", "LinkedFilePath", string.Empty, "BlingoMemberSound", true };
        yield return new object[] { "isExternal", "IsExternal", string.Empty, "BlingoMemberSound", false };
        yield return new object[] { "length", "Length", string.Empty, "BlingoMemberSound", false };
        yield return new object[] { "stereo", "Stereo", string.Empty, "BlingoMemberSound", false };

        yield return new object[] { "format", "Format", string.Empty, "BlingoMemberBitmap", false };
        yield return new object[] { "imageData", "ImageData", string.Empty, "BlingoMemberBitmap", false };
        yield return new object[] { "isLoaded", "IsLoaded", string.Empty, "BlingoMemberBitmap", false };

        yield return new object[] { "shapeType", "ShapeType", string.Empty, "BlingoMemberShape", true };
        yield return new object[] { "shapeTypeInt", "ShapeTypeInt", string.Empty, "BlingoMemberShape", true };
        yield return new object[] { "fillColor", "FillColor", string.Empty, "BlingoMemberShape", true };
        yield return new object[] { "endColor", "EndColor", string.Empty, "BlingoMemberShape", true };
        yield return new object[] { "strokeColor", "StrokeColor", string.Empty, "BlingoMemberShape", true };
        yield return new object[] { "strokeWidth", "StrokeWidth", string.Empty, "BlingoMemberShape", true };
        yield return new object[] { "closed", "Closed", string.Empty, "BlingoMemberShape", true };
        yield return new object[] { "antiAlias", "AntiAlias", string.Empty, "BlingoMemberShape", true };
        yield return new object[] { "filled", "Filled", string.Empty, "BlingoMemberShape", true };

        yield return new object[] { "currentTime", "CurrentTime", string.Empty, "BlingoMemberMedia", true };
        yield return new object[] { "duration", "Duration", string.Empty, "BlingoMemberMedia", false };
        yield return new object[] { "mediaStatus", "MediaStatus", string.Empty, "BlingoMemberMedia", false };

        yield return new object[] { "scriptType", "ScriptType", string.Empty, "BlingoMemberScript", true };
        yield return new object[] { "behaviorTypeName", "BehaviorTypeName", string.Empty, "BlingoMemberScript", false };
    }

    [Theory]
    [MemberData(nameof(MemberPropertyCases))]
    public void MemberPropertyAssignment_UsesCorrectAccessor(string lingoProperty, string expectedProperty, string indexSuffix, string? memberType, bool canAssign)
    {
        if (!canAssign)
        {
            return;
        }

        const string valueExpression = "valueExpr";
        var source = $"""
on test
  member("Title").{lingoProperty}{indexSuffix} = {valueExpression}
end
""";

        var code = GenerateBehavior(source);
        var memberAccess = ComposeMemberAccess(expectedProperty, indexSuffix, memberType);
        var expected = CreateBehavior($"{memberAccess} = {valueExpression};");
        LegacyCodeAssert.AreEqual(expected, code);
    }

    [Theory]
    [MemberData(nameof(MemberPropertyCases))]
    public void MemberPropertyGetter_UsesCorrectAccessor(string lingoProperty, string expectedProperty, string indexSuffix, string? memberType, bool _)
    {
        var source = $"""
on test
  put member("Title").{lingoProperty}{indexSuffix} into result
end
""";

        var code = GenerateBehavior(source);
        var memberAccess = ComposeMemberAccess(expectedProperty, indexSuffix, memberType);
        var expected = CreateBehavior($"result = {memberAccess};");
        LegacyCodeAssert.AreEqual(expected, code);
    }

    private static string ComposeMemberAccess(string expectedProperty, string indexSuffix, string? memberType)
    {
        var builder = new StringBuilder();
        if (string.IsNullOrEmpty(memberType))
        {
            builder.Append("Member(\"Title\")");
        }
        else
        {
            builder.Append($"Member<{memberType}>(\"Title\")");
        }

        builder.Append('.');
        builder.Append(expectedProperty);
        builder.Append(indexSuffix);
        return builder.ToString();
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
