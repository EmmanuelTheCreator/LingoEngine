
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
        private readonly List<LingoMember> _members = new();
        private readonly Dictionary<string, LingoMember> _membersByName;


        /// <summary>
        /// Returns a copy array
        /// </summary>
        internal IReadOnlyCollection<LingoMember> All => _members.ToArray();
        public int Count => _members.Count;
        internal LingoMembersContainer(Dictionary<string, LingoMember>? membersByName = null)
        {
            _membersByName = membersByName ?? new();
        }
        internal void Add(LingoMember member)
        {
            _members.Add(member);
            if (!_membersByName.ContainsKey(member.Name))
                _membersByName.Add(member.Name, member);
        }

        internal void Remove(LingoMember member)
        {
            _members.Remove(member);
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

        public ILingoMember? this[int number] => _members[number - 1];
        public ILingoMember? this[string name] => _membersByName.TryGetValue(name, out var theValue) ? theValue : null;

        public T? Member<T>(int number) where T : class, ILingoMember => _members[number - 1] as T;
        public T? Member<T>(string name) where T : class, ILingoMember => _membersByName.TryGetValue(name, out var theValue) ? theValue as T : null;
        public int GetNextNumber() => _members.Count + 1;

        internal int FindEmpty() => _members.Count + 1;

        
    }
}
