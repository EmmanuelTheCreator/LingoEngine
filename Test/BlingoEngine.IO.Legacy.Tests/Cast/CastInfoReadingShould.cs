using BlingoEngine.IO.Legacy.Cast;
using BlingoEngine.IO.Legacy.Tests.Helpers;
using Xunit;

namespace BlingoEngine.IO.Legacy.Tests.Cast
{
    public class CastInfoReadingShould
    {
        [Fact]
        public void ReadCinf()
        {
            var file = "Casts/My External Cast.cst";
            //var file = "Casts/MyMultiCastsMovie.dir";
            using var harness = TestContextHarness.Open(file);
            harness.ReadResources();
            var ctx = harness.Context;
            var reader = new BlLegacyCastReader(ctx);
            reader.Read();
        }
        [Fact]
        public void ReadMultipleCasts()
        {

            //var file = "Casts/OneCast.dir";
            //var file = "Casts/Cast_LoadAfterFrameOne.dir";
            //var file = "Casts/MyMultiCastsMovie2.dir";
            var file = "Casts/MyMulti_4_CastsMovie.dir";
            using var harness = TestContextHarness.Open(file);
            harness.ReadResources();
            var ctx = harness.Context;
            var reader = new BlLegacyCastReader(ctx);
            reader.Read();
        }
    }
}
