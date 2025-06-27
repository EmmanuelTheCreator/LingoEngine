using LingoEngine.Members;

namespace LingoEngine.Director.Core.Events
{
    public interface IHasFindMemberEvent
    {
        void FindMember(ILingoMember member);
    }
}
