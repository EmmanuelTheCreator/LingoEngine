using AbstUI.Primitives;
using System.Collections.Generic;
using BlingoEngine.Core;
using BlingoEngine.Members;
using System;

namespace BlingoEngine.Casts
{
    /// <summary>
    /// Represents a single cast library within a movie.
    /// A movie can consist of one or more cast libraries.
    /// A cast library can contain cast members such as sounds, text, graphics, and media.
    /// Lingo equivalent: castLib("LibraryName")
    /// </summary>
    public interface IBlingoCast : IDisposable
    {
        /// <summary>
        /// The name of the cast library.
        /// Lingo: the name of castLib
        /// </summary>
        string Name { get; set; }
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
        /// Indicates whether this cast library belongs to a specific movie (internal) or is shared (external).
        /// </summary>
        bool IsInternal { get; }
        /// <summary>
        /// Returns the cast members that are selected in a given Cast window
        /// </summary>
        CastMemberSelection? Selection { get; set; }

        event Action<IBlingoMember>? MemberAdded;
        event Action<IBlingoMember>? MemberDeleted;
        event Action<IBlingoMember>? MemberNameChanged;


        public T? GetMember<T>(int number) where T : IBlingoMember;
        /// <inheritdoc/>
        public T? GetMember<T>(string name) where T : IBlingoMember;
        IBlingoMembersContainer Member { get; }

        /// <summary>
        /// displays the next empty cast member position or the position after a specified cast member. This method is available only on the current cast library.
        /// </summary>
        int FindEmpty();

        /// <summary>
        /// Resolves the next available cast member slots starting from a given slot number.
        /// </summary>
        /// <param name="startSlot">The slot number used as a starting point. When zero or negative the search starts from the first available slot.</param>
        /// <param name="requiredCount">The number of slots that should be returned.</param>
        /// <returns>A list containing up to <paramref name="requiredCount"/> available slot numbers ordered by appearance.</returns>
        IReadOnlyList<int> ResolveFreeSlotNumbers(int startSlot, int requiredCount);
        IBlingoMember Add(BlingoMemberType type, int numberInCast, string name, string fileName = "", APoint regPoint = default);
        T Add<T>(int numberInCast, string name, Action<T>? configure = null) where T: IBlingoMember;

        IEnumerable<IBlingoMember> GetAll();

        /// <summary>
        /// Swap the positions of two cast members identified by their slot numbers.
        /// </summary>
        /// <param name="slot1">First slot number.</param>
        /// <param name="slot2">Second slot number.</param>
        void SwapMembers(int slot1, int slot2);

        void Save();
    }
}

