using LingoEngine.Core;
using LingoEngine.Primitives;

namespace LingoEngine.Pictures
{
    /// <summary>
    /// Represents a film loop cast member.
    /// </summary>
    public class LingoMemberFilmLoop : LingoMember
    {
        private readonly ILingoFrameworkMemberFilmLoop _frameworkFilmLoop;

        /// <summary>
        /// The sequence of cast members that make up this film loop. Each entry
        /// references a cast library number and member number pair.
        /// </summary>
        public List<LingoFilmLoopFrameRef> Frames { get; } = new();

        /// <summary>
        /// Gets the framework specific implementation for this film loop.
        /// </summary>
        public T Framework<T>() where T : class, ILingoFrameworkMemberFilmLoop => (T)_frameworkFilmLoop;

        /// <summary>
        /// The media data representing the frames and channels selection.
        /// Lingo: the media of member
        /// </summary>
        public byte[]? Media
        {
            get => _frameworkFilmLoop.Media;
            set => _frameworkFilmLoop.Media = value;
        }

        /// <summary>
        /// How the film loop content is framed within the sprite rectangle.
        /// Lingo: member.framing (#crop, #scale, #auto)
        /// </summary>
        public LingoFilmLoopFraming Framing
        {
            get => _frameworkFilmLoop.Framing;
            set => _frameworkFilmLoop.Framing = value;
        }

        /// <summary>
        /// Whether the film loop should restart automatically after the last frame.
        /// Lingo: sprite.loop when used with film loops
        /// </summary>
        public bool Loop
        {
            get => _frameworkFilmLoop.Loop;
            set => _frameworkFilmLoop.Loop = value;
        }

        public LingoMemberFilmLoop(ILingoFrameworkMemberFilmLoop frameworkMember, LingoCast cast, int numberInCast, string name = "", string fileName = "", LingoPoint regPoint = default)
            : base(frameworkMember, LingoMemberType.FilmLoop, cast, numberInCast, name, fileName, regPoint)
        {
            _frameworkFilmLoop = frameworkMember;
        }

        public override void Preload() => _frameworkFilmLoop.Preload();
        public override void Unload() => _frameworkFilmLoop.Unload();
        public override void Erase() => _frameworkFilmLoop.Erase();
        public override void ImportFileInto() => _frameworkFilmLoop.ImportFileInto();
        public override void CopyToClipBoard() => _frameworkFilmLoop.CopyToClipboard();
        public override void PasteClipBoardInto() => _frameworkFilmLoop.PasteClipboardInto();
    }
}
