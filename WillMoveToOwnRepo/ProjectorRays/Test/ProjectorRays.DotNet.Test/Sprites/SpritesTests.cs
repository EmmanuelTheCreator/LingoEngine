using FluentAssertions;
using Microsoft.Extensions.Logging;
using ProjectorRays.Common;
using ProjectorRays.director.Scores;
using ProjectorRays.director.Scores.Data;
using ProjectorRays.Director;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace ProjectorRays.DotNet.Test.Sprites
{
    public class SpritesTests
    {
        private readonly ILogger<DirectorFileTests> _logger;

        public SpritesTests(ITestOutputHelper output)
        {
            var factory = LoggerFactory.Create(builder =>
            {
                builder.AddProvider(new XunitLoggerProvider(output));
            });

            _logger = factory.CreateLogger<DirectorFileTests>();
        }

        [Fact]
        public void Test5spritesTest()
        {
            var path = GetPath("Sprites/5spritesTest.dir");
            var sprites = ReadSprites(path);
            sprites.Count.Should().Be(5);
            sprites[0].StartFrame.Should().Be(1);
            sprites[0].EndFrame.Should().Be(3);
            sprites[1].StartFrame.Should().Be(4);
            sprites[1].EndFrame.Should().Be(6);
            sprites[2].StartFrame.Should().Be(7);
            sprites[2].EndFrame.Should().Be(12);
            sprites[3].StartFrame.Should().Be(10);
            sprites[3].EndFrame.Should().Be(12);
            sprites[4].StartFrame.Should().Be(13);
            sprites[4].EndFrame.Should().Be(15);
        }
        [Fact]
        public void TestKeyFramesTest()
        {
            var path = GetPath("KeyFrames/KeyFramesTest.dir");
            //var path = GetPath("Behaviors/5spritesTest_With_Behavior.dir");
            var sprites = ReadSprites(path);
        }

        [Fact]
        public void TestTetriGrounds()
        {
            var baseDir = AppContext.BaseDirectory;
            var path = Path.Combine(baseDir, "../../../../../../../", "Demo", "TetriGrounds", "TetriGrounds.dir");
            var sprites = ReadSprites(path);
            //sprites.Count.Should().BeGreaterThan(40);
        }
        [Fact]
        public void TestLockedSprites()
        {
            var path = GetPath("Sprites/SpriteLock.dir");
            var sprites = ReadSprites(path);
            sprites[0].IsLocked.Should().BeFalse();
            sprites[1].IsLocked.Should().BeTrue();
            sprites[2].IsLocked.Should().BeFalse();
            sprites[3].IsLocked.Should().BeFalse();
            sprites[4].IsLocked.Should().BeFalse();
            sprites[5].IsLocked.Should().BeTrue();
        }
        [Fact]
        public void HasBehavior()
        {
            var path = GetPath("Behaviors/5spritesTest_With_Behavior.dir");
            var sprites = ReadSprites(path);
            sprites[0].Behaviors.Count.Should().Be(1);
        }

        private List<RaySprite> ReadSprites(string path, bool dumpToFile = false)
        {
            var score = TestFileReader.ReadScore(path, dumpToFile);
            var parser = new RaysScoreFrameParserV2(_logger, new RayStreamAnnotatorDecorator(0));
            var stream = new ReadStream(score, score.Length, Endianness.BigEndian);
            var sprites = parser.ParseScore(stream);
            return sprites;
        }

        private static string GetPath(string fileName)
        {
            var baseDir = AppContext.BaseDirectory;
            return Path.Combine(baseDir, "../../../../TestData", fileName);
        }
    }
}
