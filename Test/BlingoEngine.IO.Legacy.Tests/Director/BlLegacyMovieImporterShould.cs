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
            // todo: resources must be directly bound inside the cast member.
            var resources = data.Resources;
        }
    }
}
