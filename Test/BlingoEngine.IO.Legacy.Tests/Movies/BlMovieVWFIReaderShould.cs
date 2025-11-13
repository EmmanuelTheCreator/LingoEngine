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
        [Fact]
        public void ReadRenderer()
        {
            using var harness = TestContextHarness.Open("Movies/Movie_Render_DirectX5.dir");
            harness.ReadResources();
            var reader = new BlMovieVWFIReader(harness.Context);
            BlMovieRawInfo movieInfo = reader.Read();
            // #CC9933
            movieInfo.BackgroundColor.Should().BeEquivalentTo(new IO.Data.DTO.BlingoColorDTO(0xCC, 0x99, 0x33));
        }
        [Fact]
        public void ParseColor()
        {
            using var harness = TestContextHarness.Open("Movies/Movie_BgColor_Orange.dir");
            harness.ReadResources();
            var reader = new BlMovieVWFIReader(harness.Context);
            BlMovieRawInfo movieInfo = reader.Read();
            // #CC9933
            movieInfo.BackgroundColor.Should().BeEquivalentTo(new IO.Data.DTO.BlingoColorDTO(0xCC, 0x99, 0x33));
        }

        [Theory]
        [InlineData("Movies/Movie640x480.dir",640,480)]
        [InlineData("Movies/Movie987x654.dir", 987,654)]
        public void ReadMovieSize(string movieName, int width, int height)
        {
            using var harness = TestContextHarness.Open(movieName);
            //using var harness = TestContextHarness.Open("Movies/Movie987x654.dir");
            harness.ReadResources();
            var reader = new BlMovieVWFIReader(harness.Context);
            BlMovieRawInfo movieInfo = reader.Read();
            movieInfo.Width.Should().Be(width);
            movieInfo.Height.Should().Be(height);
        }
    }
}
