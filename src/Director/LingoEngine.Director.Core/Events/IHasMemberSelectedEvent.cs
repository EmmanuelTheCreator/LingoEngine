using LingoEngine.Members;

namespace LingoEngine.Director.Core.Events
{
    public interface IHasMemberSelectedEvent
    {
        void MemberSelected(ILingoMember member);
    }
}
