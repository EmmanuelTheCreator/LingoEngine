using System;
using System.Text.RegularExpressions;
using BlingoEngine.Legacy.Lingo.Analysis;
using BlingoEngine.Legacy.Lingo.CodeGen;
using Xunit;

namespace BlingoEngine.Legacy.Lingo.Tests;

public sealed class LegacyHandlerMiscTests : LegacyHandlerTestBase
{
    [Fact]
    public void CommentLine_IsEmittedAsComment()
    {
        const string source = """
on test
  put 1 into value -- this is a comment
end
""";

        var code = GenerateBehavior(source);

        const string expected = """
public class TestScriptBehavior : BlingoSpriteBehavior
{
    public TestScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public void Test()
    {
        value = 1;
        // this is a comment
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }

    [Fact]
    public void ExpressionLine_IsEmittedAsStatement()
    {
        const string source = """
on test
  doStuff()
end
""";

        var code = GenerateBehavior(source);

        const string expected = """
public class TestScriptBehavior : BlingoSpriteBehavior
{
    public TestScriptBehavior(IBlingoMovieEnvironment env) : base(env) { }
    public void Test()
    {
        doStuff();
    }
}
""";

        LegacyCodeAssert.AreEqual(expected, code);
    }

    [Fact]
    public void ArithmeticAssignment_InfersIntegerTypes()
    {
        const string source = """
property pNum
on new me,_beginningsprite
  pNum=_beginningsprite
end
on Sadd me
  pNum=pNum+1
end
""";

        var generator = new BlLegacyClassGenerator();
        var code = generator.GenerateClass("SpriteManager", source, BlLingoScriptKind.Parent);

        Assert.Contains("public int pNum { get; set; }", code);
        Assert.Matches(new Regex(@"int\s+\w*beginningsprite", RegexOptions.IgnoreCase), code);
        Assert.DoesNotMatch(new Regex(@"object\s+\w*beginningsprite", RegexOptions.IgnoreCase), code);
    }
}
