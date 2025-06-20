using System;
using LingoEngine.Core;
using LingoEngine.Movies;

namespace LingoEngine.Commands.Handlers;

internal sealed class StepFrameCommandHandler : ICommandHandler<StepFrameCommand>
{
    private readonly ILingoPlayer _player;
    public StepFrameCommandHandler(ILingoPlayer player) => _player = player;

    public bool CanExecute(StepFrameCommand command) => _player.ActiveMovie is LingoMovie;

    public bool Handle(StepFrameCommand command)
    {
        if (_player.ActiveMovie is not LingoMovie movie) return true;
        int offset = command.Offset;
        if (movie.IsPlaying)
        {
            var steps = Math.Abs(offset);
            for (int i = 0; i < steps; i++)
            {
                if (offset > 0) movie.NextFrame();
                else movie.PrevFrame();
            }
        }
        else
        {
            var target = Math.Clamp(movie.Frame + offset, 1, movie.FrameCount);
            movie.GoToAndStop(target);
        }
        return true;
    }
}
