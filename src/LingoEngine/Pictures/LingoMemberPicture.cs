
using LingoEngine.Casts;
using LingoEngine.Members;
using LingoEngine.Primitives;

namespace LingoEngine.Pictures
{

    /// <summary>
    /// Represents a bitmap or picture cast member in a Director movie.
    /// Lingo: member("name").type = #bitmap or #picture
    /// </summary>
    public class LingoMemberPicture : LingoMember
    {
        private readonly ILingoFrameworkMemberPicture _lingoFrameworkMemberPicture;

        /// <summary>
        /// Gets the framework object that implements the <see cref="ILingoFrameworkMemberPicture"/> interface.
        /// </summary>
        /// <typeparam name="T">The type of the framework object.</typeparam>
        /// <returns>Framework object implementing <see cref="ILingoFrameworkMemberPicture"/>.</returns>
        public T Framework<T>() where T : class, ILingoFrameworkMemberPicture => (T)_lingoFrameworkMemberPicture;

        /// <summary>
        /// Gets the raw image data (e.g., pixel data or encoded image format).
        /// This field is implementation-independent; renderers may interpret this as needed.
        /// </summary>
        public byte[]? ImageData => _lingoFrameworkMemberPicture.ImageData;

        /// <summary>
        /// Indicates whether this image is loaded into memory.
        /// Corresponds to: member.picture.loaded
        /// </summary>
        public bool IsLoaded => _lingoFrameworkMemberPicture.IsLoaded;

        /// <summary>
        /// Gets the MIME type or encoding format of the image (e.g., "image/png", "image/jpeg").
        /// </summary>
        public string Format => _lingoFrameworkMemberPicture.Format;

        /// <summary>
        /// Initializes a new instance of the <see cref="LingoMemberPicture"/> class.
        /// </summary>
        /// <param name="lingoFrameworkMemberPicture">The framework picture object.</param>
        /// <param name="numberInCast">The number of the member in the <see cref="LingoCast"/>.</param>
        /// <param name="name">The name of the member.</param>
        public LingoMemberPicture(LingoCast cast, ILingoFrameworkMemberPicture lingoFrameworkMemberPicture, int numberInCast, string name = "", string fileName = "", LingoPoint regPoint = default)
            : base(lingoFrameworkMemberPicture, LingoMemberType.Picture, cast, numberInCast, name, fileName, regPoint)
        {
            _lingoFrameworkMemberPicture = lingoFrameworkMemberPicture;
        }

        protected override LingoMember OnDuplicate(int newNumber)
        {
            throw new NotImplementedException("_lingoFrameworkMemberPicture has to be retieved from the factory");
            //var clone = new LingoMemberPicture(_cast, _lingoFrameworkMemberPicture, newNumber, Name);
            //return clone;
        }


        /// <summary>
        /// Preloads the picture into memory using <see cref="ILingoFrameworkMemberPicture.Preload"/>.
        /// Corresponds to: member.picture.preload
        /// </summary>
        public override void Preload() => _lingoFrameworkMemberPicture.Preload();

        /// <summary>
        /// Unloads the picture from memory using <see cref="ILingoFrameworkMemberPicture.Unload"/>.
        /// Corresponds to: member.picture.unload
        /// </summary>
        public override void Unload() => _lingoFrameworkMemberPicture.Unload();

        /// <summary>
        /// Erases the picture using <see cref="ILingoFrameworkMemberPicture.Erase"/>.
        /// Corresponds to: member.picture.erase
        /// </summary>
        public override void Erase() => _lingoFrameworkMemberPicture.Erase();

        /// <summary>
        /// Imports a file into the picture. This is a placeholder for future external image loading functionality.
        /// </summary>
        public override void ImportFileInto()
        {
            // Future: Implement external image loading
        }

        /// <summary>
        /// Copies the picture to the clipboard.
        /// Corresponds to: member.picture.copy
        /// </summary>
        public override void CopyToClipBoard() => _lingoFrameworkMemberPicture.CopyToClipboard();

        /// <summary>
        /// Pastes the picture from the clipboard into the current picture.
        /// Corresponds to: member.picture.paste
        /// </summary>
        public override void PasteClipBoardInto() => _lingoFrameworkMemberPicture.PasteClipboardInto();


    }

}


