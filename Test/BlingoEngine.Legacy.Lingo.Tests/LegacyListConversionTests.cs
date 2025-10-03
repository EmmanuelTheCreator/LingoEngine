using System.Text;
using Xunit;

namespace BlingoEngine.Legacy.Lingo.Tests;

public sealed class LegacyListConversionTests : LegacyHandlerTestBase
{
    [Fact]
    public void ListCommands_AreConverted()
    {
        const string source = """
global items

on test
  add items,42
  addAt items,1,99
  setAt items,2,100
  deleteAt items,3
  deleteOne items,99
  deleteAll items
  sort items
end
""";

        var code = GenerateBehavior(source);
        var expected = CreateBehavior(
            "items.Add(42);",
            "items.AddAt(1, 99);",
            "items.SetAt(2, 100);",
            "items.DeleteAt(3);",
            "items.DeleteOne(99);",
            "items.DeleteAll();",
            "items.Sort();");

        LegacyCodeAssert.AreEqual(expected, code);
    }

    [Fact]
    public void ListFunctions_AreConverted()
    {
        const string source = """
global items

on test
  put getAt(items,1) into firstItem
  put getOne(items) into oneItem
  put getLast(items) into lastItem
  put getAValue(items) into randomItem
  put findPos(items,42) into position
  put findPosNear(items,42) into nearPosition
  put getPos(items,24) into pos
  put count(items) into total
  put duplicate(items) into copy
  put ilk(items) into type
  put listP(items) into isList
  put max(items) into maxValue
  put min(items) into minValue
end
""";

        var code = GenerateBehavior(source);
        var expected = CreateBehavior(
            "firstItem = items.GetAt(1);",
            "oneItem = items.GetOne();",
            "lastItem = items.GetLast();",
            "randomItem = items.GetAValue();",
            "position = items.FindPos(42);",
            "nearPosition = items.FindPosNear(42);",
            "pos = items.GetPos(24);",
            "total = items.Count;",
            "copy = items.Duplicate();",
            "type = items.Ilk();",
            "isList = items.ListP();",
            "maxValue = items.Max();",
            "minValue = items.Min();");

        LegacyCodeAssert.AreEqual(expected, code);
    }

    private static string CreateBehavior(params string[] statements)
    {
        var builder = new StringBuilder();
        builder.AppendLine("public class TestScriptBehavior : BlingoSpriteBehavior");
        builder.AppendLine("{");
        builder.AppendLine("    public TestScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }");
        builder.AppendLine("    public void Test()");
        builder.AppendLine("    {");
        foreach (var statement in statements)
        {
            builder.Append("        ");
            builder.AppendLine(statement);
        }

        builder.AppendLine("    }");
        builder.AppendLine("}");
        return builder.ToString();
    }
}
