using System.Linq;
using BlingoEngine.IO.Legacy.Director;
using BlingoEngine.IO.Legacy.Scores;
using BlingoEngine.IO.Legacy.Scores.Datas;
using BlingoEngine.IO.Legacy.Tests.Helpers;
using Xunit;

namespace BlingoEngine.IO.Legacy.Tests.Scores
{
    public class BlLegacyScoreReaderShould
    {
        [Fact]
        public void SplitMultiChannelBlocksIntoSeparateTokens()
        {
            using var harness = TestContextHarness.Open("KeyFrames/Animation_types.dir");
            harness.ReadResources();
            var reader = new BlLegacyScoreReader(harness.Context);
            reader.Read();

            Assert.Equal(6, reader.Frames.Count);
            Assert.Equal(12, reader.Sprites.Count);

            var firstFrame = reader.Frames[0];
            Assert.Equal(14, firstFrame.Tokens.Count);

            var spriteChannels = firstFrame.Tokens
                .Where(t => t.Payload.Length == 0x30)
                .Select(t => t.Channel)
                .ToArray();

            Assert.Equal(new[] { 6, 7, 8, 9, 10, 11, 12, 13, 25, 86, 105, 154 }, spriteChannels);

            foreach (var token in firstFrame.Tokens.Where(t => t.Payload.Length == 0x30))
                Assert.Equal(0x30, token.Payload.Length);
        }

        [Fact]
        public void PreserveSpriteKeyframesWhenBuildingDtos()
        {
            using var harness = TestContextHarness.Open("KeyFrames/KeyFramesTest.dir");
            harness.ReadResources();
            var reader = new BlLegacyScoreReader(harness.Context);
            reader.Read();

            var score = new BlLegacyScore(reader.Sprites.ToArray(), reader.Frames.ToArray());
            var sprites = BlLegacyScoreSpriteBuilder.Build(score);

            Assert.Equal(new[] { 8, 10 }, sprites.Select(s => s.SpriteNum).OrderBy(v => v));

            var sprite10 = Assert.Single(sprites, s => s.SpriteNum == 10);
            Assert.NotNull(sprite10.Animator);
            var animator10 = sprite10.Animator!;

            Assert.Collection(
                animator10.Position,
                k =>
                {
                    Assert.Equal(11, k.Frame);
                    Assert.Equal(86, k.Value.X);
                    Assert.Equal(158, k.Value.Y);
                },
                k =>
                {
                    Assert.Equal(19, k.Frame);
                    Assert.Equal(217, k.Value.X);
                    Assert.Equal(89, k.Value.Y);
                },
                k =>
                {
                    Assert.Equal(30, k.Frame);
                    Assert.Equal(217, k.Value.X);
                    Assert.Equal(173, k.Value.Y);
                });

            Assert.Collection(
                animator10.Rotation,
                k =>
                {
                    Assert.Equal(11, k.Frame);
                    Assert.Equal(29.93f, k.Value, 2);
                },
                k =>
                {
                    Assert.Equal(19, k.Frame);
                    Assert.Equal(-80.56f, k.Value, 2);
                });

            Assert.Collection(
                animator10.Blend,
                k =>
                {
                    Assert.Equal(11, k.Frame);
                    Assert.Equal(20f, k.Value);
                },
                k =>
                {
                    Assert.Equal(19, k.Frame);
                    Assert.Equal(100f, k.Value);
                });

            var sprite8 = Assert.Single(sprites, s => s.SpriteNum == 8);
            Assert.NotNull(sprite8.Animator);
            var animator8 = sprite8.Animator!;

            Assert.Empty(animator8.Position);

            Assert.Collection(
                animator8.Rotation,
                k =>
                {
                    Assert.Equal(10, k.Frame);
                    Assert.Equal(17.23f, k.Value, 2);
                },
                k =>
                {
                    Assert.Equal(12, k.Frame);
                    Assert.Equal(20f, k.Value);
                });

            Assert.Collection(
                animator8.Blend,
                k =>
                {
                    Assert.Equal(10, k.Frame);
                    Assert.Equal(20f, k.Value);
                },
                k =>
                {
                    Assert.Equal(12, k.Frame);
                    Assert.Equal(90.2f, k.Value, 1);
                });
        }

        [Fact]
        public void ParseRotationAndSkewFromFullAndDeltaKeyFrames()
        {
            using var harness = TestContextHarness.Open("KeyFrames/KeyFramesTestMultiple.dir");
            harness.ReadResources();
            var reader = new BlLegacyScoreReader(harness.Context);
            reader.Read();

            var firstFrame = reader.Frames.First();
            var initialToken = firstFrame.Tokens.First(t => t.Channel == 10 && t.Payload.Length == 0x30);

            Assert.Equal(-1133, initialToken.Properties.Single(p => p.Property == BlSpriteRawData.BlSpriteRawProperty.Rotation).Value);
            Assert.Equal(3233, initialToken.Properties.Single(p => p.Property == BlSpriteRawData.BlSpriteRawProperty.Skew).Value);

            var frameSix = reader.Frames.Single(f => f.FrameNum == 6);
            var rotationDelta = frameSix.Tokens.Single(t => t.Channel == 10 && t.AddressOffset == 0x01FE);
            var skewDelta = frameSix.Tokens.Single(t => t.Channel == 10 && t.AddressOffset == 0x0202);

            Assert.Equal(-445, rotationDelta.Properties.Single(p => p.Property == BlSpriteRawData.BlSpriteRawProperty.Rotation).Value);
            Assert.Equal(2694, skewDelta.Properties.Single(p => p.Property == BlSpriteRawData.BlSpriteRawProperty.Skew).Value);
        }

    }
}
