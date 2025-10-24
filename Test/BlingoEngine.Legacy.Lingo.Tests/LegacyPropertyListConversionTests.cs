using System.Text;
using Xunit;

namespace BlingoEngine.Legacy.Lingo.Tests;

public sealed class LegacyPropertyListConversionTests : LegacyHandlerTestBase
{
    [Fact]
    public void PropertyListCommands_AreConverted()
    {
        const string source = """
global plist

on test
  addProp plist,#foo,42
  setProp plist,#bar,24
  deleteProp plist,#zap
  setaProp plist,#foo,99
  addAt plist,2,#newProp,77
  deleteAt plist,3
  setAt plist,4,88
end
""";

        var code = GenerateBehavior(source);
        var expected = CreateBehavior(
            "plist.Add(Symbol(\"foo\"), 42);",
            "plist.SetProp(Symbol(\"bar\"), 24);",
            "plist.DeleteProp(Symbol(\"zap\"));",
            "plist.SetaProp(Symbol(\"foo\"), 99);",
            "plist.AddAt(2, Symbol(\"newProp\"), 77);",
            "plist.DeleteAt(3);",
            "plist.SetAt(4, 88);");

        LegacyCodeAssert.AreEqual(expected, code);
    }

    [Fact]
    public void PropertyListFunctions_AreConverted()
    {
        const string source = """
global plist

on test
  put getProp(plist,#foo) into x
  put getaProp(plist,#foo) into y
  put getPropAt(plist,2) into keyName
  put findPos(plist,#foo) into pos
  put findPosNear(plist,#foo) into nearPos
  put getPos(plist,42) into valuePos
  put getAt(plist,1) into firstValue
  put count(plist) into total
  put duplicate(plist) into copy
end
""";

        var code = GenerateBehavior(source);
        var expected = CreateBehavior(
            "x = plist.GetProp(Symbol(\"foo\"));",
            "y = plist.GetaProp(Symbol(\"foo\"));",
            "keyName = plist.GetPropAt(2);",
            "pos = plist.FindPos(Symbol(\"foo\"));",
            "nearPos = plist.FindPosNear(Symbol(\"foo\"));",
            "valuePos = plist.GetPos(42);",
            "firstValue = plist.GetAt(1);",
            "total = plist.Count;",
            "copy = plist.Duplicate();");

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
