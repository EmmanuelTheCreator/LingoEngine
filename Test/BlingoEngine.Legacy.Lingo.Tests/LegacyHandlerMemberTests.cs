using Xunit;

namespace BlingoEngine.Legacy.Lingo.Tests;

public sealed class LegacyHandlerMemberTests : LegacyHandlerTestBase
{
    [Fact]
    public void SpriteMemberAssignment_UsesSetMember()
    {
        const string source = """
on test
  sprite(2).member = member("Name")
end
""";

        var code = GenerateBehavior(source);
        Assert.Contains("Sprite(2).SetMember(\"Name\");", code);
    }

    [Fact]
    public void SpriteMemberAssignment_WithCastLibKeepsArguments()
    {
        const string source = """
on test
  sprite(3).member = member("Title", "CastLib")
end
""";

        var code = GenerateBehavior(source);
        Assert.Contains("Sprite(3).SetMember(\"Title\", \"CastLib\");", code);
    }

    [Fact]
    public void PutMemberAssignment_UsesSetMember()
    {
        const string source = """
on test
  put member("Marker") into sprite(4).member
end
""";

        var code = GenerateBehavior(source);
        Assert.Contains("Sprite(4).SetMember(\"Marker\");", code);
    }
}
