using LingoEngine.Primitives;

namespace LingoEngine.Core
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
        CastMemberSelection? Selection { get; set; }


        public T? GetMember<T>(int number) where T : class, ILingoMember;
        /// <inheritdoc/>
        public T? GetMember<T>(string name) where T : class, ILingoMember;
        ILingoMembersContainer Member { get; }
        
        /// <summary>
        /// displays the next empty cast member position or the position after a specified cast member. This method is available only on the current cast library.
        /// </summary>
        int FindEmpty();
        ILingoMember Add(LingoMemberType type, int numberInCast, string name, string fileName = "", LingoPoint regPoint = default);

        IEnumerable<ILingoMember> GetAll();
    }
}
