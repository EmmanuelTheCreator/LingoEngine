using BlingoEngine.IO.Legacy.Scores;
using BlingoEngine.IO.Legacy.Tests.Helpers;
using Xunit;

namespace BlingoEngine.IO.Legacy.Tests.Scores
{
    public class BlLegacyScoreReaderShould
    {
        [Fact]
        public void ReadKeyFrames()
        {
            //var file = "KeyFrames/Animation_types.dir";
            var file = "KeyFrames/KeyFramesTest.dir";
            //var file = "Behaviors/5spritesTest_With_Behavior.dir";
            using var harness = TestContextHarness.Open(file);
            harness.ReadResources();
            var ctx = harness.Context;
            var reader = new BlLegacyScoreReader(ctx);
            reader.Read();
        }
    }
}
