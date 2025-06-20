using LingoEngine.Members;
using LingoEngine.Movies;

namespace LingoEngine.Director.Core.Events
{
    public interface IHasSpriteSelectedEvent
    {
        void SpriteSelected(ILingoSprite sprite);
    }
    public interface IHasMemberSelectedEvent
    {
        void MemberSelected(ILingoMember member);
    }
    public interface IHasMenuItemSelectedEvent
    {
        void MenuItemSelected(string menuCode);
    }

    public interface IHasFindMemberEvent
    {
        void FindMember(ILingoMember member);
    }
}
