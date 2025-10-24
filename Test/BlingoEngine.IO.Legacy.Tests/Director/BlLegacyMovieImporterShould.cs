using System;
using System.IO;
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
    }
}
