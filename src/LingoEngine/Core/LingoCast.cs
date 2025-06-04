
namespace LingoEngine.Core
{
    public enum PreLoadModeType
    {
        /// <summary>
        /// Load the cast library when needed. This is the default value
        /// </summary>
        WhenNeeded = 0,
        /// <summary>
        /// Load the cast library before frame 1
        /// </summary>
        BeforeFrame1,
        /// <summary>
        /// Load the cast library after frame 1
        /// </summary>
        AfterFrame1,
    }
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
        /// eturns or sets the filename of a cast library. Read-only for internal cast
        /// libraries, read/write for external cast libraries.
        /// For external cast libraries, fileName returns the cast’s full pathname and filename.
        /// For internal cast libraries, fileName returns a value depending on which internal cast library
        /// is specified.
        /// • If the first internal cast library is specified, fileName returns the name of the movie.
        /// • If any other internal cast library is specified, fileName returns an empty string.
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// The index number of the cast library in the movie.
        /// Lingo: the number of castLib
        /// </summary>
        int Number { get; }
        /// <summary>
        /// Cast library property; determines the preload mode of a specified cast library.
        /// </summary>
        PreLoadModeType PreLoadMode { get; set; }
        /// <summary>
        /// Returns the cast members that are selected in a given Cast window
        /// </summary>
        CastMemberSelection Selection { get; set; }

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
        LingoMember Member(int number);
        T? Member<T>(int number) where T : LingoMember;
        LingoMember Member(string name);
        T? Member<T>(string name) where T : LingoMember;

        ILingoCast Add(LingoMember member);

        /// <summary>
        /// Attempts to retrieve a cast member by name.
        /// Lingo: member "name" of castLib
        /// </summary>
        /// <param name="name">The name of the cast member.</param>
        /// <param name="member">The resulting cast member, if found.</param>
        /// <returns>True if found; otherwise, false.</returns>
        bool TryGetMember(string name, out LingoMember? member);
        /// <summary>
        /// displays the next empty cast member position or the position after a specified cast member. This method is available only on the current cast library.
        /// </summary>
        int FindEmpty();
    }
    public class CastMemberSelection
    {
        // Todo
    }

    /// <inheritdoc/>
    public class LingoCast : ILingoCast
    {
        private Dictionary<string, LingoMember> _membersByName = new();
        private List<LingoMember> _members = new();
        /// <inheritdoc/>
        public string Name { get; set; }
        /// <inheritdoc/>
        public string FileName { get; set; }
        /// <inheritdoc/>
        public int Number { get; private set; }
        /// <inheritdoc/>
        public int MemberCount => _members.Count;
        /// <inheritdoc/>
        public PreLoadModeType PreLoadMode { get; set; }
        /// <inheritdoc/>
        public CastMemberSelection Selection { get; set; }
        public LingoCast(string name, int number)
        {
            Name = name;
            Number = number;
        }

        /// <inheritdoc/>
        public string GetMemberName(int number) => _members[number - 1].Name;
        /// <inheritdoc/>
        public LingoMember Member(int number) => _members[number - 1];
        /// <inheritdoc/>
        public T? Member<T>(int number) where T : LingoMember => _members[number - 1] as T;

        /// <inheritdoc/>
        public LingoMember Member(string name) => _membersByName[name];
        /// <inheritdoc/>
        public T? Member<T>(string name) where T : LingoMember
            => _membersByName[name] as T;
        /// <inheritdoc/>
        public ILingoCast Add(LingoMember member)
        {
            //var member = new LingoMember( name, _members.Count + 1);
            _members.Add(member);
            _membersByName.Add(member.Name, member);
            return this;
        }
        /// <inheritdoc/>
        public bool TryGetMember(string name, out LingoMember? member) => _membersByName.TryGetValue(name, out member);
        /// <inheritdoc/>
        public int FindEmpty() => _members.Count + 1;
    }
}
