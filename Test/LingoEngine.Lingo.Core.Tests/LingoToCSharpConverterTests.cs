using LingoEngine.Lingo.Core;
using Xunit;

namespace LingoEngine.Lingo.Core.Tests;

public class LingoToCSharpConverterTests
{
    [Fact]
    public void PutStatementIsConverted()
    {
        var result = LingoToCSharpConverter.Convert("put 1 into x");
        Assert.Equal("x = 1;", result.Trim());
    }

    [Fact]
    public void AssignmentStatementIsConverted()
    {
        var result = LingoToCSharpConverter.Convert("x = 5");
        Assert.Equal("x = 5;", result.Trim());
    }

    [Fact]
    public void CallStatementIsConverted()
    {
        var result = LingoToCSharpConverter.Convert("myFunc");
        Assert.Equal("myFunc();", result.Trim());
    }

    [Fact]
    public void IfStatementIsConverted()
    {
        var lingo = "if 1 then\nput 2 into x\nend if";
        var result = LingoToCSharpConverter.Convert(lingo);
        var expected = string.Join('\n',
            "if (1)",
            "{",
            "x = 2;",
            "}");
        Assert.Equal(expected.Trim(), result.Trim());
    }

    [Fact]
    public void RepeatWhileStatementIsConverted()
    {
        var lingo = "repeat while 1\nput 2 into x\nend repeat";
        var result = LingoToCSharpConverter.Convert(lingo);
        var expected = string.Join('\n',
            "while (1)",
            "{",
            "x = 2;",
            "}");
        Assert.Equal(expected.Trim(), result.Trim());
    }

    [Fact]
    public void ExitRepeatIfStatementIsConverted()
    {
        var result = LingoToCSharpConverter.Convert("exit repeat if 1");
        Assert.Equal("if (1) break;", result.Trim());
    }

    [Fact]
    public void NextRepeatStatementIsConverted()
    {
        var result = LingoToCSharpConverter.Convert("next repeat");
        Assert.Equal("continue;", result.Trim());
    }
}
