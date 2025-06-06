using LingoEngine.FrameworkCommunication;
using LingoEngine.Primitives;
using System.Collections.Generic;
using System.ComponentModel;

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
        CastMemberSelection? Selection { get; set; }


        public T? GetMember<T>(int number) where T : class, ILingoMember;
        /// <inheritdoc/>
        public T? GetMember<T>(string name) where T : class, ILingoMember;
        ILingoMembersContainer Member { get; }
        
        /// <summary>
        /// displays the next empty cast member position or the position after a specified cast member. This method is available only on the current cast library.
        /// </summary>
        int FindEmpty();
        void Add(LingoMemberType type, string name, string fileName = "", LingoPoint regPoint = default);


    }
    public class CastMemberSelection
    {
        // Todo
    }

    /// <inheritdoc/>
    public class LingoCast : ILingoCast
    {
        
        private readonly LingoCastLibsContainer _castLibsContainer;
        private readonly ILingoFrameworkFactory _factory;
        private readonly LingoMembersContainer _MembersContainer = new();

        /// <inheritdoc/>
        public string Name { get; set; }
        /// <inheritdoc/>
        public string FileName { get; set; } = "";
        /// <inheritdoc/>
        public int Number { get; private set; }
        /// <inheritdoc/>
        public PreLoadModeType PreLoadMode { get; set; } = PreLoadModeType.WhenNeeded;
        /// <inheritdoc/>
        public CastMemberSelection? Selection { get; set; } = null;

        public ILingoMembersContainer Member => _MembersContainer;
        
        internal LingoCast(LingoCastLibsContainer castLibsContainer, ILingoFrameworkFactory factory, string name)
        {
            _castLibsContainer = castLibsContainer;
            _factory = factory;
            Name = name;
            Number = castLibsContainer.GetNextCastNumber();
        }

        /// <inheritdoc/>
        public T? GetMember<T>(int number) where T : class, ILingoMember => _MembersContainer[number - 1] as T;
        /// <inheritdoc/>
        public T? GetMember<T>(string name) where T : class, ILingoMember => _MembersContainer[name] as T;
        /// <inheritdoc/>
        internal ILingoCast Add(LingoMember member)
        {
            _castLibsContainer.AddMember(member);
            _MembersContainer.Add(member);
            return this;
        }
        internal ILingoCast Remove(LingoMember member)
        {
            _castLibsContainer.RemoveMember(member);
            _MembersContainer.Remove(member);
            return this;
        }
        internal void MemberNameChanged(string oldName, LingoMember member)
        {
            _castLibsContainer.MemberNameChanged(oldName, member);
            _MembersContainer.MemberNameChanged(oldName, member);
        }
        /// <inheritdoc/>
        public int FindEmpty() => _MembersContainer.FindEmpty();
        internal int GetUniqueNumber() => _castLibsContainer.GetNextMemberNumber();

        internal void RemoveAll()
        {
            var allMembers = _MembersContainer.All;
            foreach (var member in allMembers)
                Remove(member);
        }

        public void Add(LingoMemberType type, string name, string fileName = "", LingoPoint regPoint =default)
        {
            switch (type)
            {
                case LingoMemberType.Bitmap: _factory.CreateMemberPicture(this, name, fileName, regPoint); break;
                case LingoMemberType.Sound: _factory.CreateMemberSound(this, name, fileName, regPoint); break;
                case LingoMemberType.Text: _factory.CreateMemberText(this, name, fileName, regPoint); break;
                default:
                    _factory.CreateEmpty(this, name, fileName, regPoint); break;
            }

        }
    }
}
