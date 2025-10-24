using AbstUI.Commands;
using BlingoEngine.Members;

namespace BlingoEngine.Director.Core.Members.Commands
{
    public sealed class FindCastMemberInScoreCommand : IAbstCommand
    {
        public FindCastMemberInScoreCommand(IBlingoMember member)
        {
            Member = member;
        }

        public IBlingoMember Member { get; }
    }
}
