using LingoEngine.Members;
using LingoEngine.Movies;
using LingoEngine.Commands;
using System;

namespace LingoEngine.Director.Core.Stages;

public sealed record AddSpriteCommand(
    LingoMovie Movie,
    ILingoMember Member,
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
}
