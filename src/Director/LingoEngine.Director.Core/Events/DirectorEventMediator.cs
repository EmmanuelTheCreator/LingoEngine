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
        void RaiseMenuSelected(string menuCode);
        IDirecorEventSubscription SubscribeToMenu(string menuCode, Func<bool> value);
    }
    public interface IDirecorEventSubscription
    {
        void Release();
    }
    internal class DirectorEventMediator : IDirectorEventMediator
    {
        private readonly List<IHasSpriteSelectedEvent> _spriteSelected = new();
        private readonly List<IHasMemberSelectedEvent> _membersSelected = new();
        private readonly List<IHasFindMemberEvent> _findMemberEvents = new();
        private readonly List<IHasMenuItemSelectedEvent> _menuItemsSelected = new();
        private readonly Dictionary<string,List<EventSubscription>> _menuItemsSelectedSubscriptions = new();

        public void Subscribe(object listener)
        {
            if (listener is IHasSpriteSelectedEvent spriteSelected) _spriteSelected.Add(spriteSelected);
            if (listener is IHasMemberSelectedEvent memberSelected) _membersSelected.Add(memberSelected);
            if (listener is IHasFindMemberEvent findMember) _findMemberEvents.Add(findMember);
            if (listener is IHasMenuItemSelectedEvent menuItemSelected) _menuItemsSelected.Add(menuItemSelected);
        }

        public void Unsubscribe(object listener)
        {
            if (listener is IHasSpriteSelectedEvent spriteSelected)
                _spriteSelected.Remove(spriteSelected);
            if (listener is IHasMemberSelectedEvent memberSelected)
                _membersSelected.Remove(memberSelected);
            if (listener is IHasFindMemberEvent findMember)
                _findMemberEvents.Remove(findMember);
            if (listener is IHasMenuItemSelectedEvent menuItemSelected)
                _menuItemsSelected.Remove(menuItemSelected);
        }

        public void RaiseSpriteSelected(ILingoSprite sprite)
            => _spriteSelected.ForEach(x => x.SpriteSelected(sprite));

        public void RaiseMemberSelected(ILingoMember member)
            => _membersSelected.ForEach(x => x.MemberSelected(member));

        public void RaiseFindMember(ILingoMember member)
            => _findMemberEvents.ForEach(x => x.FindMember(member));

        public void RaiseMenuSelected(string menuCode)
        {
            GetForMenu(menuCode).ForEach(x => x.Do());
            _menuItemsSelected.ForEach(x => x.MenuItemSelected(menuCode));
        }

        public IDirecorEventSubscription SubscribeToMenu(string menuCode, Func<bool> action)
        {
            var subscription = new EventSubscription(menuCode, action, o => _menuItemsSelectedSubscriptions[menuCode].Remove(o));
            GetForMenu(menuCode).Add(subscription);
            return subscription;
        }
        private List<EventSubscription> GetForMenu(string menuCode)
        {
            if (_menuItemsSelectedSubscriptions.TryGetValue(menuCode, out var subscriptions)) return subscriptions;
            var subscriptions2 = new List<EventSubscription>();
            _menuItemsSelectedSubscriptions.Add(menuCode, subscriptions2);
            return subscriptions2;
        }

        private class EventSubscription : IDirecorEventSubscription
        {
            private readonly Func<bool> _action;
            private readonly Action<EventSubscription> _onRelease;
            public string MenuCode { get; }

            public EventSubscription(string menuCode,Func<bool> action, Action<EventSubscription> onRelease)
            {
                MenuCode = menuCode;
                _action = action;
                _onRelease = onRelease;
            }
            public void Do() => _action();

            public void Release() => _onRelease(this);
        }
    }
}
