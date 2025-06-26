using LingoEngine.Movies;
using LingoEngine.Commands;
using System;

namespace LingoEngine.Director.Core.Stages;

public sealed record ChangeSpriteRangeCommand(
    LingoMovie Movie,
    LingoSprite Sprite,
    int StartChannel,
    int StartBegin,
    int StartEnd,
    int EndChannel,
    int EndBegin,
    int EndEnd) : ILingoCommand
{
    public Action ToUndo(Action refresh)
    {
        var sprite = Sprite;
        var movie = Movie;
        int channel = StartChannel;
        int begin = StartBegin;
        int end = StartEnd;
        return () =>
        {
            if (sprite.SpriteNum - 1 != channel)
                movie.ChangeSpriteChannel(sprite, channel);
            sprite.BeginFrame = begin;
            sprite.EndFrame = end;
            refresh();
        };
    }

    public Action ToRedo(Action refresh)
    {
        var sprite = Sprite;
        var movie = Movie;
        int channel = EndChannel;
        int begin = EndBegin;
        int end = EndEnd;
        return () =>
        {
            if (sprite.SpriteNum - 1 != channel)
                movie.ChangeSpriteChannel(sprite, channel);
            sprite.BeginFrame = begin;
            sprite.EndFrame = end;
            refresh();
        };
    }
}
