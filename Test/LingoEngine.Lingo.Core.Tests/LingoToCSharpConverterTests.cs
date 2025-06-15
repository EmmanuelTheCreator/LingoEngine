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
}
