using BlingoEngine.IO.Legacy.Movies;
using BlingoEngine.IO.Legacy.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace BlingoEngine.IO.Legacy.Tests.Movies
{
    public class BlMovieVWFIReaderShould
    {
        [Fact]
        public void ReadAboutAndCopyright()
        {
            using var harness = TestContextHarness.Open("Movies/Movie_About_Copyright.dir");
            harness.ReadResources();
            var reader = new BlMovieVWFIReader(harness.Context);
            BlMovieRawInfo movieInfo = reader.Read();
            movieInfo.AboutText.Should().Be("My about text");
            movieInfo.CopyRightText.Should().Be("My copyright text");

        }
    }
}
