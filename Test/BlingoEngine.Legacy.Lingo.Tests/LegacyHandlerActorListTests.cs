using Xunit;

namespace BlingoEngine.Legacy.Lingo.Tests;

public sealed class LegacyHandlerActorListTests : LegacyHandlerTestBase
{
    [Fact]
    public void ActorListAppend_AddsToMovieList()
    {
        const string source = """
on test
  the actorList.append(me)
end
""";

        var code = GenerateBehavior(source);
        Assert.Contains("_Movie.ActorList.Add(this);", code);
    }

    [Fact]
    public void ActorListDeleteOne_RemovesFromMovieList()
    {
        const string source = """
on test
  the actorList.deleteOne(me)
end
""";

        var code = GenerateBehavior(source);
        Assert.Contains("_Movie.ActorList.Remove(this);", code);
    }
}
