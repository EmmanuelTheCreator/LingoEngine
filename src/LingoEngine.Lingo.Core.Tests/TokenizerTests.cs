using System.Linq;
using Director.Scripts;
using Xunit;

namespace LingoEngine.Lingo.Core.Tests;

public class TokenizerTests
{
    [Fact]
    public void RoundTripSimpleScript()
    {
        var source = "put 1 into x";
        var tokenizer = new Tokenizer(source);
        var tokens = new System.Collections.Generic.List<LingoToken>();
        while (!tokenizer.End)
        {
            var token = tokenizer.NextToken();
            tokens.Add(token);
            if (token.Type == LingoTokenType.Eof) break;
        }
        Assert.Contains(tokens, t => t.Type == LingoTokenType.Put);
        Assert.Contains(tokens, t => t.Type == LingoTokenType.Into);
    }
}
