using AbstUI;
using AbstUI.Primitives;
using BlingoEngine.Bitmaps;
using BlingoEngine.Casts;
using BlingoEngine.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BlingoEngine.Members
{
    public enum BlingoMemberType
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
    /// Lingo Member With Texture interface.
    /// </summary>
    public interface IBlingoMemberWithTexture
    {
        /// <summary>
        /// The texture associated with this member, if any.
        /// </summary>
        IAbstTexture2D? TextureBlingo { get; }
        IAbstTexture2D? RenderToTexture(BlingoInkType ink, AColor transparentColor);
    }

    /// <summary>
    /// Represents a cast member within a cast library.
    /// Cast members can contain media (e.g., images, sounds, video) or scripts (behaviors, movie scripts).
    /// Corresponds to Lingo: member "Name" or member x
    /// </summary>
    public interface IBlingoMember : IDisposable, IHasPropertyChanged
    {
        /// <summary>
        /// Retrieves the framework object like godot, unity or SDL
        /// </summary>
        IBlingoFrameworkMember FrameworkObj { get; }
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
        /// The number in the cast
        /// </summary>
        int NumberInCast { get; }

        /// <summary>
        /// The priority with which the member will be purged from memory.
        /// 0 = never purge, higher = lower priority.
        /// Lingo: the purgePriority of member
        /// </summary>
        int PurgePriority { get; set; }
        /// <summary>
        /// Specifies the registration point of a cast member
        /// </summary>
        APoint RegPoint { get; set; }

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
        string FileName { get; set; }

        /// <summary>
        /// The type of the cast member (e.g., bitmap, sound, script).
        /// Lingo: the type of member
        /// </summary>
        BlingoMemberType Type { get; }
        string CastName { get; }
        public IBlingoCast Cast { get; }

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
        Task PreloadAsync();

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
        IBlingoMember Duplicate(int? newNumber = null);
        /// <summary>
        /// Retrieves the next member
        /// </summary>
        IBlingoMember? GetMemberInCastByOffset(int numberOffset);

        /// <summary>
        /// Determines whether the pixel at the specified coordinates is fully transparent.
        /// Coordinates are relative to the member's top-left corner.
        /// </summary>
        /// <param name="x">X coordinate in pixels.</param>
        /// <param name="y">Y coordinate in pixels.</param>
        /// <returns><c>true</c> if the pixel is transparent; otherwise, <c>false</c>.</returns>
        bool IsPixelTransparent(int x, int y);
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
    [DebuggerDisplay("Member:{Number}:Cast={CastLibNum},{NumberInCast}:{Type}:{Name}:Size={Width}x{Height}")]
    public class BlingoMember : IBlingoMember, IHasPropertyChanged
    {
        protected readonly BlingoCast _cast;
        private string _name = string.Empty;
        private readonly IBlingoFrameworkMember _frameworkMember;
        private readonly List<IMemberRefUser> _linkedMemberRefUsers = new();
        private bool _hasBeenDisposed;
        private string _fileName;
        private string _comments = string.Empty;

        public IBlingoFrameworkMember FrameworkObj => _frameworkMember;

        /// <inheritdoc/>
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                var oldName = _name;
                _name = value;
                if (_cast != null && !string.IsNullOrWhiteSpace(_name)) _cast.MemberNameHasChanged(oldName, this);
                OnPropertyChanged();
                MemberChanged?.Invoke();
            }
        }
        /// <inheritdoc/>
        public int Number { get; private set; }
        /// <inheritdoc/>
        public DateTime CreationDate { get; set; }
        /// <inheritdoc/>
        public DateTime ModifiedDate { get; set; }
        /// <inheritdoc/>
        private bool _hilite;
        public bool Hilite
        {
            get => _hilite;
            private set => SetProperty(ref _hilite, value);
        }
        /// <inheritdoc/>
        public int CastLibNum { get; private set; }
        /// <inheritdoc/>
        private APoint _regPoint;
        public APoint RegPoint
        {
            get => _regPoint;
            set => SetProperty(ref _regPoint, value);
        }
        /// <inheritdoc/>
        public int PurgePriority { get; set; }
        /// <inheritdoc/>
        private int _width;
        public virtual int Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }
        /// <inheritdoc/>
        private int _height;
        public virtual int Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }
        /// <inheritdoc/>
        public long Size { get; set; }
        /// <inheritdoc/>
        public string Comments
        {
            get => _comments;
            set => SetProperty(ref _comments, value);
        }
        /// <inheritdoc/>
        public string FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }
        /// <inheritdoc/>
        public BlingoMemberType Type { get; private set; }
        public int NumberInCast { get; internal set; }
        public string CastName { get => _cast.Name; }
        public IBlingoCast Cast { get => _cast; }
        public bool HasChanged { get; internal set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action? MemberChanged;

        /// <inheritdoc/>
        public BlingoMember(IBlingoFrameworkMember frameworkMember, BlingoMemberType type, BlingoCast cast, int numberInCast, string name = "", string fileName = "", APoint regPoint = default)
        {
            _frameworkMember = frameworkMember;
            NumberInCast = numberInCast;
            // We need to first set the name to not trigger the NameChangedEvent
            Name = name;
            // Then the cast
            _cast = cast;
            RegPoint = regPoint;
            CastLibNum = _cast.Number;
            Number = _cast.GetUniqueNumber(NumberInCast);
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
        public virtual Task PreloadAsync() => _frameworkMember.PreloadAsync();
        public virtual void Unload() => _frameworkMember.Unload();

        public IBlingoMember Duplicate(int? newNumber = null)
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

        public virtual void ChangesHasBeenApplied() => HasChanged = false;
        protected virtual BlingoMember OnDuplicate(int newNumber)
        {
            throw new NotImplementedException();
            //var clone = new BlingoMember( Type, _cast, newNumber, Name);
            //return clone;
        }

        public IBlingoMember? GetMemberInCastByOffset(int numberOffset)
        {
            return _cast.Member[Number + numberOffset];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        internal void UsedBy(IMemberRefUser refUser)
        {
            if (!_linkedMemberRefUsers.Contains(refUser))
                _linkedMemberRefUsers.Add(refUser);
        }

        /// <inheritdoc/>
        public virtual bool IsPixelTransparent(int x, int y)
            => _frameworkMember.IsPixelTransparent(x, y);

        internal virtual void ReleaseFromRefUser(IMemberRefUser refUser)
        {
            _linkedMemberRefUsers.Remove(refUser);
        }

        protected virtual void OnDispose() { }

        public void Dispose()
        {
            if (_hasBeenDisposed)
                return;
            _hasBeenDisposed = true;

            foreach (var user in _linkedMemberRefUsers.ToArray())
                user.MemberHasBeenRemoved();
            _linkedMemberRefUsers.Clear();

            OnDispose();

            if (FrameworkObj is IDisposable disposable)
                disposable.Dispose();
        }
    }
}

