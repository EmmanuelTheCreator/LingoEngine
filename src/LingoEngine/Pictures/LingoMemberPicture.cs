
namespace LingoEngine.Pictures
{
    namespace LingoEngine
    {
        /// <summary>
        /// Represents a bitmap or picture cast member in a Director movie.
        /// Lingo: member("name").type = #bitmap or #picture
        /// </summary>
        public class LingoMemberPicture : LingoMember
        {
            private readonly ILingoFrameworkMemberPicture _lingoFrameworkMemberPicture;

            /// <summary>
            /// Gets the framework object that implements the ILingoFrameworkMemberPicture interface.
            /// </summary>
            /// <typeparam name="T">The type of the framework object.</typeparam>
            /// <returns>Framework object implementing ILingoFrameworkMemberPicture.</returns>
            public T FrameworkObj<T>() where T : ILingoFrameworkMemberPicture => (T)_lingoFrameworkMemberPicture;

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
            /// Initializes a new instance of the LingoMemberPicture class.
            /// </summary>
            /// <param name="lingoFrameworkMemberPicture">The framework picture object.</param>
            /// <param name="number">The number of the member.</param>
            /// <param name="name">The name of the member.</param>
            public LingoMemberPicture(ILingoFrameworkMemberPicture lingoFrameworkMemberPicture, int number, string name = "")
                : base(LingoMemberType.Picture, number, name)
            {
                _lingoFrameworkMemberPicture = lingoFrameworkMemberPicture;
            }

            /// <summary>
            /// Preloads the picture into memory.
            /// Corresponds to: member.picture.preload
            /// </summary>
            public void Preload() => _lingoFrameworkMemberPicture.Preload();

            /// <summary>
            /// Unloads the picture from memory.
            /// Corresponds to: member.picture.unload
            /// </summary>
            public void Unload() => _lingoFrameworkMemberPicture.Unload();

            /// <summary>
            /// Erases the picture.
            /// Corresponds to: member.picture.erase
            /// </summary>
            public void Erase() => _lingoFrameworkMemberPicture.Erase();

            /// <summary>
            /// Imports a file into the picture. This is a placeholder for future external image loading functionality.
            /// </summary>
            public void ImportFileInto()
            {
                // Future: Implement external image loading
            }

            /// <summary>
            /// Copies the picture to the clipboard.
            /// Corresponds to: member.picture.copy
            /// </summary>
            public void CopyToClipBoard() => _lingoFrameworkMemberPicture.CopyToClipBoard();

            /// <summary>
            /// Pastes the picture from the clipboard into the current picture.
            /// Corresponds to: member.picture.paste
            /// </summary>
            public void PasteClipBoardInto() => _lingoFrameworkMemberPicture.PasteClipBoardInto();

            /// <summary>
            /// Creates a duplicate of the current picture member.
            /// </summary>
            /// <returns>A new LingoMemberPicture object that is a duplicate of the current one.</returns>
            public ILingoMember Duplicate()
            {
                var clone = new LingoMemberPicture(_lingoFrameworkMemberPicture, Number, Name)
                {
                    Width = Width,
                    Height = Height,
                    Comments = Comments,
                    PurgePriority = PurgePriority
                };
                return clone;
            }
        }

    }

}
