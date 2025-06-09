using LingoEngine.FrameworkCommunication;
using LingoEngine.Primitives;
using LingoEngine.Texts;
using System.Security.Cryptography;
using static System.Net.Mime.MediaTypeNames;

namespace LingoEngine.Core
{
    public enum LingoMemberType
    {
        Unknown,
        Animgif, Ole,
        Bitmap, Palette,
        Button, Picture,
        Cursor, QuickTimeMedia,
        DigitalVideo, RealMedia,
        DVD, Script,
        Empty, Shape,
        Field, Shockwave3D,
        FilmLoop, Sound,
        Flash, Swa,
        Flashcomponent, Text,
        Font, Transition,
        Havok, VectorShape,
        Movie, WindowsMedia
    }
    /// <summary>
    /// Represents a cast member within a cast library.
    /// Cast members can contain media (e.g., images, sounds, video) or scripts (behaviors, movie scripts).
    /// Corresponds to Lingo: member "Name" or member x
    /// </summary>
    public interface ILingoMember
    {
        /// <summary>
        /// Retrieves the framework object like godot, unity or SDL
        /// </summary>
        ILingoFrameworkMember FrameworkObj { get; }
        /// <summary>
        /// The name of the cast member.
        /// Lingo: the name of member
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// indicates the cast library number of a specified cast member.
        /// The value of this property is a unique identifier for the cast member that is a single integer
        /// describing its location in and position in the cast library.
        /// Its a unique number over all casts.
        /// </summary>
        int Number { get; }

        /// <summary>
        /// The creation timestamp of the cast member.
        /// Lingo: the date of member
        /// </summary>
        DateTime CreationDate { get; }

        /// <summary>
        /// The last modification timestamp of the cast member.
        /// Lingo: the modification date of member
        /// </summary>
        DateTime ModifiedDate { get; }


        /// <summary>
        /// Whether the cast member is currently highlighted in the Cast window.
        /// Lingo: the hilite of member
        /// </summary>
        bool Hilite { get; }

        /// <summary>
        /// The number of the cast library to which this member belongs.
        /// Lingo: the castLibNum of member
        /// </summary>
        int CastLibNum { get; }

        /// <summary>
        /// The priority with which the member will be purged from memory.
        /// 0 = never purge, higher = lower priority.
        /// Lingo: the purgePriority of member
        /// </summary>
        int PurgePriority { get; set; }
        /// <summary>
        /// Specifies the registration point of a cast member
        /// </summary>
        LingoPoint RegPoint { get; set; }

        /// <summary>
        /// The width (in pixels) of the cast member's content, if applicable.
        /// Lingo: the width of member
        /// </summary>
        int Width { get; set; }

        /// <summary>
        /// The height (in pixels) of the cast member's content, if applicable.
        /// Lingo: the height of member
        /// </summary>
        int Height { get; set; }

        /// <summary>
        /// The size of the member in memory (bytes).
        /// Lingo: the size of member
        /// </summary>
        long Size { get; set; }

        /// <summary>
        /// Arbitrary comments associated with the cast member.
        /// Lingo: the comment of member
        /// </summary>
        string Comments { get; set; }

        /// <summary>
        /// The filename associated with an external linked cast member (if any).
        /// Lingo: the fileName of member
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// The type of the cast member (e.g., bitmap, sound, script).
        /// Lingo: the type of member
        /// </summary>
        LingoMemberType Type { get; }

        /// <summary>
        /// Copies the member’s data to the system clipboard.
        /// Lingo: copy member to clipboard
        /// </summary>
        void CopyToClipBoard();

        /// <summary>
        /// Deletes the cast member from the cast library.
        /// Lingo: erase member
        /// </summary>
        void Erase();

        /// <summary>
        /// Opens a file dialog to import external content into this cast member.
        /// Lingo: importFileInto member
        /// </summary>
        void ImportFileInto();

        /// <summary>
        /// Moves the cast member within the cast library (e.g., reordering).
        /// Lingo: move member
        /// </summary>
        void Move();

        /// <summary>
        /// Pastes data from the clipboard into this cast member.
        /// Lingo: pasteClipBoardInto member
        /// </summary>
        void PasteClipBoardInto();

        /// <summary>
        /// Loads the cast member into memory before use (optional optimization).
        /// Lingo: preload member
        /// </summary>
        void Preload();

        /// <summary>
        /// Unloads the cast member from memory.
        /// Lingo: unload member
        /// </summary>
        void Unload();

        /// <summary>
        /// Creates a copy of the cast member with the same contents.
        /// Optional. An integer that specifies the Cast window for the duplicate cast member. If omitted, the duplicate cast member is placed in the first open Cast window position.
        /// Lingo: duplicate member
        /// </summary>
        ILingoMember Duplicate(int? newNumber = null);
        /// <summary>
        /// Retrieves the next member
        /// </summary>
        ILingoMember? GetMemberInCastByOffset(int numberOffset);
    }

    /// <summary>
    /// Represents a cast member within a cast library. Cast members are the media and script assets in a
    /// movie.Media cast members may be text, bitmaps, shapes, and so on.Script cast members include
    /// behaviors, movie scripts, and so on.
    /// A cast member can be referenced either by number or by name.
    /// • When referring to a cast member by number, Director searches a particular cast library for that
    ///     cast member, and retrieves the member’s data. This method is faster than referring to a cast
    ///     member by name.However, because Director does not automatically update references to cast
    ///     member numbers in script, a numbered reference to a cast member that has moved position in
    ///     its cast library will be broken.
    /// • When referring to a cast member by name, Director searches all cast libraries in a movie from
    ///     first to last, and retrieves the member’s data when it finds the named member. This method is
    ///     slower than referring to a cast member by number, especially when referring to large movies
    ///     that contain many cast libraries and cast members. However, a named reference to a cast
    ///     member allows the reference to remain intact even if the cast member moves position in its
    ///     cast library.
    /// </summary>
    public class LingoMember : ILingoMember
    {
        protected readonly LingoCast _cast;
        private string _name = string.Empty;
        private readonly ILingoFrameworkMember _frameworkMember;
        public ILingoFrameworkMember FrameworkObj => _frameworkMember;

        /// <inheritdoc/>
        public string Name
        {
            get => _name;
            set
            {
                var oldName = _name;
                var changed = _name != value;
                _name = value;
                if (_cast != null && changed && !string.IsNullOrWhiteSpace(_name)) _cast.MemberNameChanged(oldName, this);
            }
        }
        /// <inheritdoc/>
        public int Number { get; private set; }
        /// <inheritdoc/>
        public DateTime CreationDate { get; set; }
        /// <inheritdoc/>
        public DateTime ModifiedDate { get; set; }
        /// <inheritdoc/>
        public bool Hilite { get; private set; }
        /// <inheritdoc/>
        public int CastLibNum { get; private set; }
        /// <inheritdoc/>
        public LingoPoint RegPoint { get; set; }
        /// <inheritdoc/>
        public int PurgePriority { get; set; }
        /// <inheritdoc/>
        public int Width { get; set; }
        /// <inheritdoc/>
        public int Height { get; set; }
        /// <inheritdoc/>
        public long Size { get; set; }
        /// <inheritdoc/>
        public string Comments { get; set; }
        /// <inheritdoc/>
        public string FileName { get; private set; }
        /// <inheritdoc/>
        public LingoMemberType Type { get; private set; }
        public int NumberInCast { get; internal set; }

        /// <inheritdoc/>
        public LingoMember(ILingoFrameworkMember frameworkMember, LingoMemberType type, LingoCast cast, int numberInCast, string name = "", string fileName = "", LingoPoint regPoint = default)
        {
            _frameworkMember = frameworkMember;
            NumberInCast = numberInCast;
            // We need to first set the name to not trigger the NameChangedEvent
            Name = name;
            // Then the cast
            _cast = cast;
            RegPoint = regPoint;
            CastLibNum = _cast.Number;
            Number = _cast.GetUniqueNumber();
            Type = type;
            CreationDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
            FileName = fileName;
            Comments = string.Empty;
            cast.Add(this);
        }

        /// <inheritdoc/>
        public virtual void Erase() => _frameworkMember.Erase();
        public virtual void ImportFileInto() => _frameworkMember.ImportFileInto();
        public virtual void Move() { }
        public virtual void CopyToClipBoard() => _frameworkMember.CopyToClipboard();
        public virtual void PasteClipBoardInto() => _frameworkMember.PasteClipboardInto();
        public virtual void Preload() => _frameworkMember.Preload();
        public virtual void Unload() => _frameworkMember.Unload();

        public ILingoMember Duplicate(int? newNumber = null)
        {
            if (!newNumber.HasValue)
                newNumber = _cast.FindEmpty();
            var clone = OnDuplicate(newNumber.Value);
            clone.Width = Width;
            clone.Height = Height;
            clone.Size = Size;
            clone.Comments = Comments;
            clone.PurgePriority = PurgePriority;
            clone.CastLibNum = CastLibNum;
            clone.FileName = FileName;
            clone.Hilite = Hilite;
            clone.Name = Name;
            clone.RegPoint = RegPoint;
            _cast.Add(clone);
            return clone;
        }
        protected virtual LingoMember OnDuplicate(int newNumber)
        {
            throw new NotImplementedException();
            //var clone = new LingoMember( Type, _cast, newNumber, Name);
            //return clone;
        }

        public ILingoMember? GetMemberInCastByOffset(int numberOffset)
        {
            return _cast.Member[Number + numberOffset];
        }
    }
}
