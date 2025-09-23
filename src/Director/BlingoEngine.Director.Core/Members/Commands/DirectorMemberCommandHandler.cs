using AbstUI.Commands;
using BlingoEngine.Core;
using BlingoEngine.Director.Core.Tools;
using BlingoEngine.Members;
using BlingoEngine.Members.Commands;

namespace BlingoEngine.Director.Core.Members.Commands;

public sealed class DirectorMemberCommandHandler : BlingoMemberCommandHandler
{
    private readonly IDirectorEventMediator _mediator;

    public DirectorMemberCommandHandler(BlingoPlayer player, IHistoryManager historyManager, IDirectorEventMediator mediator)
        : base(player, historyManager)
    {
        _mediator = mediator;
    }

    protected override void NotifyMemberChanged(IBlingoMember member, bool refreshCast)
    {
        if (refreshCast)
            _mediator.Raise(DirectorEventType.CastPropertiesChanged);

        _mediator.RaiseMemberSelected(member);
    }
}
