
namespace LingoEngine.Core
{
    public interface ILingoMembersContainer
    {
        /// <summary>
        /// Retrieves a cast member by its number in the library.
        /// Lingo: member x of castLib
        /// </summary>
        /// <param name="number">The cast member number (1-based).</param>
        /// <returns>The specified cast member.</returns>
        ILingoMember? this[int number] { get; }
        ILingoMember? this[string name] { get; }
        /// <summary>
        /// Retrieves a cast member by its number in the library.
        /// Lingo: member x of castLib
        /// </summary>
        /// <param name="number">The cast member number (1-based).</param>
        /// <returns>The specified cast member.</returns>
        T? Member<T>(int number) where T : class, ILingoMember;
        T? Member<T>(string name) where T : class, ILingoMember;
    }

    internal class LingoMembersContainer : ILingoMembersContainer
    {
        private readonly Dictionary<int, LingoMember> _members = new();
        private readonly bool containerForAll;
        private readonly Dictionary<string, LingoMember> _membersByName;


        /// <summary>
        /// Returns a copy array
        /// </summary>
        internal IReadOnlyCollection<LingoMember> All => _members.Values.ToArray();
        public int Count => _members.Count;
        internal LingoMembersContainer(bool containerForAll,Dictionary<string, LingoMember>? membersByName = null)
        {
            this.containerForAll = containerForAll;
            _membersByName = membersByName ?? new();
        }
        internal void Add(LingoMember member)
        {
            if (containerForAll)
                _members.Add(member.Number,member);
            else
            {
                if (member.NumberInCast == 0)
                    member.NumberInCast = GetNextNumber();
                _members[member.NumberInCast] = member;
            }
            if (!_membersByName.ContainsKey(member.Name))
                _membersByName.Add(member.Name, member);
        }

        internal void Remove(LingoMember member)
        {
            if (containerForAll)
                _members.Remove(member.Number);
            else
                _members.Remove(member.NumberInCast);
            if (_membersByName.ContainsKey(member.Name))
                _membersByName.Remove(member.Name);
        }

        internal void MemberNameChanged(string oldName, LingoMember member)
        {
            if (!string.IsNullOrWhiteSpace(oldName) && _membersByName.ContainsKey(member.Name))
                _membersByName.Remove(member.Name);
            if (!_membersByName.ContainsKey(member.Name))
                _membersByName.Add(member.Name, member);
        }

        public ILingoMember? this[int number] => _members.TryGetValue(number, out var member) ? member : null;
        public ILingoMember? this[string name] => _membersByName.TryGetValue(name, out var theValue) ? theValue : null;

        public T? Member<T>(int number) where T : class, ILingoMember => _members.TryGetValue(number, out var member) ? member as T: null;
        public T? Member<T>(string name) where T : class, ILingoMember => _membersByName.TryGetValue(name, out var theValue) ? theValue as T : null;
        public int GetNextNumber() => _members.Keys.Any()? _members.Keys.Max() + 1 : 1;

        private int _lastHighestNumber =1;
        internal int FindEmpty()
        {
            var newNumber = 1;
            for (int i = _lastHighestNumber; i < 5000; i++)
            {
                if (!_members.ContainsKey(i))
                {
                    _lastHighestNumber = i;
                    return i;
                }
            }
            return newNumber;
        }
    }
}
