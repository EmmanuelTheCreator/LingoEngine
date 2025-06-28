using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Commands;
using System;
using LingoEngine.Sprites;

namespace LingoEngine.Director.Core.Stages.Commands;

public sealed record AddSpriteCommand(
    LingoMovie Movie,
    ILingoMember Member,
    // Channel number is 1-based to match LingoMovie
    int Channel,
    int BeginFrame,
    int EndFrame) : ILingoCommand
{
    public Action ToUndo(LingoSprite sprite, Action refresh)
    {
        return () =>
        {
            sprite.RemoveMe();
            refresh();
        };
    }


    public Action ToRedo(Action refresh)
    {
        var movie = Movie;
        var member = Member;
        int channel = Channel;
        int begin = BeginFrame;
        int end = EndFrame;
        return () =>
        {
            movie.AddSprite(channel, begin, end, 0, 0, s => s.SetMember(member));
            refresh();
        };
    }
}
