using AbstUI.Commands;
using BlingoEngine.Casts.Commands;
using BlingoEngine.Core;
using BlingoEngine.Director.Core.Tools;

namespace BlingoEngine.Director.Core.Casts.Commands;

public sealed class DirectorCastCommandHandler : BlingoCastCommandHandler
{
    private readonly IDirectorEventMediator _mediator;

    public DirectorCastCommandHandler(BlingoPlayer player, IHistoryManager historyManager, IDirectorEventMediator mediator)
        : base(player, historyManager)
    {
        _mediator = mediator;
    }

    protected override void NotifyCastChanged()
    {
        _mediator.Raise(DirectorEventType.CastPropertiesChanged);
    }
}
