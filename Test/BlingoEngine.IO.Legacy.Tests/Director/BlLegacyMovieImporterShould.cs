using System;
using System.IO;
using System.Linq;
using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Legacy.Director;
using BlingoEngine.IO.Legacy.Tests.Helpers;
using BlingoEngine.IO.Legacy.Tests.Texts;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace BlingoEngine.IO.Legacy.Tests.Director
{
    public class BlLegacyMovieImporterShould
    {
        private readonly ILogger<XmedFileTest> _logger;

        public BlLegacyMovieImporterShould(ITestOutputHelper output)
        {
            var factory = LoggerFactory.Create(builder =>
            {
                builder.AddProvider(new XunitLoggerProvider(output));
            });

            _logger = factory.CreateLogger<XmedFileTest>();
        }

        [Fact]
        public void ImportSimpleMovie()
        {
            var importer = new BlLegacyMovieImporter(_logger);
            var file = TestContextHarness.GetAudioAssetPath("DirFileWith_3_Sounds.dir");
            var data = importer.Import(file);
            var resources = data.Resources;

            Assert.All(resources.Files, resource => Assert.NotEqual(DirFileResourceKind.Unknown, resource.Kind));
            Assert.All(resources.Files, resource =>
            {
                var extension = Path.GetExtension(resource.FileName);
                Assert.True(string.Equals(".wav", extension, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(".mp3", extension, StringComparison.OrdinalIgnoreCase));
            });
        }

        [Fact]
        public void ImportMovie_UsesCastNamesAndLoadsExternalCast()
        {
            var importer = new BlLegacyMovieImporter(_logger);
            var file = TestFolder.AssetPath("Casts/MyMultiCastsMovie.dir");
            var archive = new BlLegacyMovieReader().Read(file);

            var data = importer.Import(file);
            var movie = data.Movie;

            Assert.Equal(archive.CastLibraries.Count, movie.Casts.Count);

            for (var i = 0; i < archive.CastLibraries.Count; i++)
            {
                var expectedName = string.IsNullOrWhiteSpace(archive.CastLibraries[i].Name)
                    ? $"Cast {i + 1}"
                    : archive.CastLibraries[i].Name;
                Assert.Equal(expectedName, movie.Casts[i].Name);
            }

            var external = archive.CastLibraries
                .Select((cast, index) => new { cast, index })
                .First(item => !item.cast.IsInternal);

            Assert.Empty(external.cast.MemberSlots);

            var externalDto = movie.Casts[external.index];
            Assert.NotEmpty(externalDto.Members);
            Assert.All(externalDto.Members, member => Assert.Equal(externalDto.Number, member.CastLibNum));
        }
    }
}
