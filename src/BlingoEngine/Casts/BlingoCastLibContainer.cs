using BlingoEngine.FrameworkCommunication;
using BlingoEngine.Members;
using BlingoEngine.Core;

namespace BlingoEngine.Casts
{
    /// <summary>
    /// Lingo Cast Libs Container interface.
    /// </summary>
    public interface IBlingoCastLibsContainer
    {
        IBlingoCast ActiveCast { get; set; }
        int Count { get; }
        IBlingoCast this[int index] { get; }
        IBlingoCast this[string name] { get; }
        /// <summary>
        /// Seacch in all members
        /// </summary>
        IBlingoMembersContainer Member { get; }
        IBlingoMember? GetMember(BlingoMemberRef memberRef);
        T? GetMember<T>(BlingoMemberRef memberRef) where T : class, IBlingoMember;
        IBlingoMember? GetMember(int number, int? castLibNum = null);
        IBlingoMember? GetMember(string name, int? castLibNum = null);
        T? GetMember<T>(int number, int? castLibNum = null) where T : IBlingoMember;
        T? GetMember<T>(string name, int? castLibNum = null) where T : IBlingoMember;
        IBlingoCast AddCast(string name, bool isInternal = false);
        IBlingoCast GetCast(int number);
        string GetCastName(int number);
        IEnumerable<IBlingoCast> GetAll();
        void LoadCastLibFromBuilder(IBlingoCastLibBuilder builder);
    }


    public class BlingoCastLibsContainer : IBlingoCastLibsContainer
    {
        private Dictionary<string, IBlingoMember> _allMembersByName = new();
        private Dictionary<string, BlingoCast> _castsByName = new();
        private List<BlingoCast> _casts = new();
        private IBlingoCast _activeCast = null!;
        private readonly BlingoMembersContainer _allMembersContainer;
        private readonly IBlingoFrameworkFactory _factory;

        public IBlingoMembersContainer Member => _allMembersContainer;
        public int Count => _casts.Count;

        public IBlingoCast ActiveCast
        {
            get => _activeCast; set
            {
                if (_casts.Contains(value))
                    _activeCast = value;
            }
        }
        public BlingoCastLibsContainer(IBlingoFrameworkFactory factory)
        {
            _allMembersContainer = new BlingoMembersContainer(true, _allMembersByName);
            _factory = factory;
        }

        public IBlingoCast this[int number] => _casts[number - 1];
        public IBlingoCast this[string name] => _castsByName[name.ToLower()];
        public string GetCastName(int number) => _casts[number - 1].Name;
        public IBlingoCast GetCast(int number) => _casts[number - 1];

        public IBlingoCast AddCast(string name, bool isInternal = false)
        {
            var nameL = name.ToLower();
            var cast = new BlingoCast(this, _factory, name, isInternal);
            _casts.Add(cast);
            _castsByName.Add(nameL, cast);
            if (ActiveCast == null)
                ActiveCast = cast;
            return cast;
        }
        public IBlingoCast RemoveCast(IBlingoCast cast)
        {
            var nameL = cast.Name.ToLower();
            var castTyped = (BlingoCast)cast;
            castTyped.Dispose();
            _casts.Remove(castTyped);
            _castsByName.Remove(nameL);
            if (_activeCast == cast && _casts.Count > 0)
                _activeCast = _casts[0];

            return cast;
        }

        public void RemoveInternal()
        {
            for (int i = _casts.Count - 1; i >= 0; i--)
            {
                var cast = _casts[i];
                if (cast.IsInternal)
                    RemoveCast(cast);
            }
        }

        public int GetNextMemberNumber(int castNumber, int numberInCast) => _allMembersContainer.GetNextNumber(castNumber, numberInCast);
        public IBlingoMember? GetMember(BlingoMemberRef memberRef)
        {
            if (memberRef.MemberNum <= 0)
                return null;

            var member = GetMember(memberRef.MemberNum, memberRef.CastLibNum > 0 ? memberRef.CastLibNum : null);
            if (member == null)
                return null;

            if (memberRef.MemberType != BlingoMemberType.Unknown && member.Type != memberRef.MemberType)
                return null;

            return member;
        }

        public T? GetMember<T>(BlingoMemberRef memberRef) where T : class, IBlingoMember
            => GetMember(memberRef) as T;

        public T? GetMember<T>(int number, int? castLibNum = null) where T : IBlingoMember
            => !castLibNum.HasValue
             ? _allMembersContainer.Member<T>(number)
             : _casts[castLibNum.Value - 1].GetMember<T>(number);
        public IBlingoMember? GetMember(int number, int? castLibNum = null)
            => !castLibNum.HasValue
             ? _allMembersContainer[number]
             : _casts[castLibNum.Value - 1].Member[number];
        public IBlingoMember? GetMember(string name, int? castLibNum = null)
            => !castLibNum.HasValue
             ? _allMembersContainer[name]
             : _casts[castLibNum.Value - 1].Member[name];

        public T? GetMember<T>(string name, int? castLibNum = null) where T : IBlingoMember
            => !castLibNum.HasValue
             ? _allMembersContainer.Member<T>(name)
             : _casts[castLibNum.Value - 1].GetMember<T>(name);

        internal void AddMember(BlingoMember member)
        {
            _allMembersContainer.Add(member);
            if (!_allMembersByName.ContainsKey(member.Name))
                _allMembersByName.Add(member.Name, member);
        }

        internal void RemoveMember(IBlingoMember member)
        {
            _allMembersContainer.Remove(member);
            _allMembersByName.Remove(member.Name);
        }

        internal void MemberNameChanged(string oldName, BlingoMember member)
        {
            if (!string.IsNullOrWhiteSpace(oldName) && _allMembersByName.ContainsKey(member.Name))
                _allMembersByName.Remove(member.Name);
            if (!_allMembersByName.ContainsKey(member.Name))
                _allMembersByName.Add(member.Name, member);
            _allMembersContainer.MemberNameChanged(oldName, member);
        }

        internal int GetNextCastNumber() => _casts.Count + 1;

        public IEnumerable<IBlingoCast> GetAll() => _casts;

        public void LoadCastLibFromBuilder(IBlingoCastLibBuilder builder)
            => builder.BuildAsync(this);
    }
}

