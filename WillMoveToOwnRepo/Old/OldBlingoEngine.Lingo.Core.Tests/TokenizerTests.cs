using System.Linq;
using OldBlingoEngine.Lingo.Core.Tokenizer;
using Xunit;

namespace BlingoEngine.Lingo.Core.Tests;

public class TokenizerTests
{
    [Fact]
    public void RoundTripSimpleScript()
    {
        var source = "put 1 into x";
        var tokenizer = new BlingoTokenizer(source);
        var tokens = new System.Collections.Generic.List<BlingoToken>();
        while (!tokenizer.End)
        {
            var token = tokenizer.NextToken();
            tokens.Add(token);
            if (token.Type == BlingoTokenType.Eof) break;
        }
        Assert.Contains(tokens, t => t.Type == BlingoTokenType.Put);
        Assert.Contains(tokens, t => t.Type == BlingoTokenType.Into);
    }
}

