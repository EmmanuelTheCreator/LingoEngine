
namespace LingoEngine
{
    /// <summary>
    /// Represents a single cast library within a movie.
    /// A movie can consist of one or more cast libraries.
    /// A cast library can contain cast members such as sounds, text, graphics, and media.
    /// Lingo equivalent: castLib("LibraryName")
    /// </summary>
    public interface ILingoCast
    {
        /// <summary>
        /// The name of the cast library.
        /// Lingo: the name of castLib
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The index number of the cast library in the movie.
        /// Lingo: the number of castLib
        /// </summary>
        int Number { get; }

        /// <summary>
        /// Returns the name of the cast member at the given position.
        /// Lingo: the name of member x of castLib
        /// </summary>
        /// <param name="number">The cast member number (1-based).</param>
        /// <returns>The name of the cast member.</returns>
        string GetMemberName(int number);

        /// <summary>
        /// Retrieves a cast member by its number in the library.
        /// Lingo: member x of castLib
        /// </summary>
        /// <param name="number">The cast member number (1-based).</param>
        /// <returns>The specified cast member.</returns>
        LingoMember GetMember(int number);
        T? GetMember<T>(int number) where T : LingoMember;
        LingoMember GetMember(string name);
        T? GetMember<T>(string name) where T : LingoMember;

        ILingoCast Add(LingoMember member);

        /// <summary>
        /// Attempts to retrieve a cast member by name.
        /// Lingo: member "name" of castLib
        /// </summary>
        /// <param name="name">The name of the cast member.</param>
        /// <param name="member">The resulting cast member, if found.</param>
        /// <returns>True if found; otherwise, false.</returns>
        bool TryGetMember(string name, out LingoMember? member);
    }

    /// <inheritdoc/>
    public class LingoCast : ILingoCast
    {
        private Dictionary<string, LingoMember> _membersByName = new();
        private List<LingoMember> _members = new();

        public string Name { get; set; }
        public int Number { get; private set; }
        /// <summary>
        /// Lingo : .member.count
        /// </summary>
        public int MemberCount => _members.Count;


        public LingoCast(string name, int number)
        {
            Name = name;
            Number = number;
        }


        public string GetMemberName(int number) => _members[number - 1].Name;
        public LingoMember GetMember(int number) => _members[number - 1];
        public T? GetMember<T>(int number) where T : LingoMember => _members[number - 1] as T;


        public LingoMember GetMember(string name) => _membersByName[name];

        public T? GetMember<T>(string name) where T : LingoMember
            => _membersByName[name] as T;
        public ILingoCast Add(LingoMember member)
        {
            //var member = new LingoMember( name, _members.Count + 1);
            _members.Add(member);
            _membersByName.Add(member.Name, member);
            return this;
        }

        public bool TryGetMember(string name, out LingoMember? member) => _membersByName.TryGetValue(name, out member);

       
    }
}
