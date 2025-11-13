using BlingoEngine.IO.Data.DTO;
using BlingoEngine.IO.Legacy.Director;
using BlingoEngine.IO.Legacy.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace BlingoEngine.IO.Legacy.Tests.Director;

public class BlLegacyMovieBlingoExtensionsShould
{
    [Fact]
    public void PopulateSpriteDefaultsFromSingleSpriteMovie()
    {
        var path = TestFolder.AssetPath("KeyFrames/SingleSprite/SingleSprite.dir");
        var reader = new BlLegacyMovieReader();
        var archive = reader.Read(path);
        var resources = new DirFilesContainerDTO();
        
        var movie = archive.ToBlingo("SingleSprite", resources, NullLogger.Instance);

        var sprite = Assert.Single(movie.Sprite2Ds);
        Assert.Equal(6, sprite.SpriteNum);
        Assert.Equal(46f, sprite.LocH);
        Assert.Equal(56f, sprite.LocV);
        Assert.Equal(108f, sprite.Width);
        Assert.Equal(29f, sprite.Height);
        Assert.Equal(100f, sprite.Blend);
        Assert.Equal(1, sprite.BeginFrame);
        Assert.Equal(20, sprite.EndFrame);
        Assert.NotNull(sprite.Member);
        var member = sprite.Member!;
        Assert.Equal(1, member.MemberNum);
        Assert.Equal(1, member.CastLibNum);
        Assert.Null(sprite.Animator);
    }
  

    
}
