

namespace LingoEngine.Core
{
    public interface ILingoCastLibsContainer
    {
        ILingoCast this[int index] { get; }
        ILingoCast this[string name] { get; }
        /// <summary>
        /// Seacch in all members
        /// </summary>
        ILingoMembersContainer Member { get; }
        int Count { get; }
        T? GetMember<T>(int number) where T : class, ILingoMember;
        T? GetMember<T>(string name) where T : class, ILingoMember;
        ILingoCast AddCast(string name);
        ILingoCast GetCast(int number);
        string GetCastName(int number);
    }


    internal class LingoCastLibsContainer : ILingoCastLibsContainer
    {
        private Dictionary<string, LingoMember> _allMembersByName = new();
        private Dictionary<string, LingoCast> _castsByName = new();
        private List<LingoCast> _casts = new();
        private readonly LingoMembersContainer _allMembersContainer;
        
        public ILingoMembersContainer Member => _allMembersContainer;
        public int Count => _casts.Count;


        public LingoCastLibsContainer()
        {
            _allMembersContainer = new LingoMembersContainer(_allMembersByName);
        }

        public ILingoCast this[int number] => _casts[number - 1];
        public ILingoCast this[string name] => _castsByName[name];
        public string GetCastName(int number) => _casts[number - 1].Name;
        public ILingoCast GetCast(int number) => _casts[number - 1];

        public ILingoCast AddCast(string name)
        {
            var cast = new LingoCast(this, name);
            _casts.Add(cast);
            _castsByName.Add(name, cast);
            return cast;
        }
        public ILingoCast RemoveCast(ILingoCast cast)
        {
            var castTyped = (LingoCast)cast;
            castTyped.RemoveAll();
            _casts.Remove(castTyped);
            _castsByName.Remove(cast.Name);
            return cast;
        }

        public int GetNextMemberNumber() => _allMembersContainer.GetNextNumber();
        public T? GetMember<T>(int number) where T : class, ILingoMember
            => _allMembersContainer.Member<T>(number);

        public T? GetMember<T>(string name) where T : class, ILingoMember
            => _allMembersContainer.Member<T>(name);

        internal void AddMember(LingoMember member)
        {
            _allMembersContainer.Add(member);
            _allMembersByName.Add(member.Name, member);
        }

        internal void RemoveMember(LingoMember member)
        {
            _allMembersContainer.Remove(member);
            _allMembersByName.Remove(member.Name);
        }

        internal void MemberNameChanged(string oldName, LingoMember member)
        {
            if (!string.IsNullOrWhiteSpace(oldName) && _allMembersByName.ContainsKey(member.Name))
                _allMembersByName.Remove(member.Name);
            if (!_allMembersByName.ContainsKey(member.Name))
                _allMembersByName.Add(member.Name, member);
            _allMembersContainer.MemberNameChanged(oldName, member);
        }

        internal int GetNextCastNumber() => _casts.Count + 1;
    }
}
