using LingoEngine.Members;
using LingoEngine.Movies;

namespace LingoEngine.Director.Core.Events
{
    public interface IDirectorEventMediator
    {
        void Subscribe(object listener);
        void Unsubscribe(object listener);
        void RaiseSpriteSelected(ILingoSprite sprite);
        void RaiseMemberSelected(ILingoMember member);
        void RaiseFindMember(ILingoMember member);
    }
    internal class DirectorEventMediator : IDirectorEventMediator
    {
        private readonly List<IHasSpriteSelectedEvent> _spriteSelected = new();
        private readonly List<IHasMemberSelectedEvent> _membersSelected = new();
        private readonly List<IHasFindMemberEvent> _findMemberEvents = new();

        public void Subscribe(object listener)
        {
            if (listener is IHasSpriteSelectedEvent spriteSelected) _spriteSelected.Add(spriteSelected);
            if (listener is IHasMemberSelectedEvent memberSelected) _membersSelected.Add(memberSelected);
            if (listener is IHasFindMemberEvent findMember)
                _findMemberEvents.Add(findMember);
        }

        public void Unsubscribe(object listener)
        {
            if (listener is IHasSpriteSelectedEvent spriteSelected)
                _spriteSelected.Remove(spriteSelected);
            if (listener is IHasMemberSelectedEvent memberSelected)
                _membersSelected.Remove(memberSelected);
            if (listener is IHasFindMemberEvent findMember)
                _findMemberEvents.Remove(findMember);
        }

        public void RaiseSpriteSelected(ILingoSprite sprite)
            => _spriteSelected.ForEach(x => x.SpriteSelected(sprite));

        public void RaiseMemberSelected(ILingoMember member)
            => _membersSelected.ForEach(x => x.MemberSelected(member));

        public void RaiseFindMember(ILingoMember member)
            => _findMemberEvents.ForEach(x => x.FindMember(member));

        private class EventSubscription : IDirectorEventSubscription
        {
            private readonly Action<EventSubscription> _onRelease;
            private readonly Func<bool> _action;

            public string Code { get; }

            public EventSubscription(string code, Func<bool> action, Action<EventSubscription> onRelease)
            {
                Code = code;
                _action = action;
                _onRelease = onRelease;
            }

            public void Do() => _action();
            public void Release() => _onRelease(this);
        }
    }

    public interface IDirectorEventSubscription
    {
        void Release();
    }
}
