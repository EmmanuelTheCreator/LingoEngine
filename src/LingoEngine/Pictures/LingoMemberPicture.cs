using System;
using System.Collections.Generic;
using System.Text;

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
            /// <summary>
            /// Raw image data (e.g., pixel data or encoded image format).
            /// This field is implementation-independent; renderers may interpret this as needed.
            /// </summary>
            public byte[]? ImageData { get; set; }

            /// <summary>
            /// Indicates whether this image is loaded into memory.
            /// Corresponds to: member.picture.loaded
            /// </summary>
            public bool IsLoaded { get; private set; }

            /// <summary>
            /// Optional MIME type or encoding format (e.g., "image/png", "image/jpeg")
            /// </summary>
            public string Format { get; set; } = "image/unknown";

            public LingoMemberPicture(int number, string name = "")
                : base(LingoMemberType.Picture, number, name)
            {
            }

            /// <inheritdoc/>
            public void Preload()
            {
                IsLoaded = true;
            }

            /// <inheritdoc/>
            public void Unload()
            {
                IsLoaded = false;
            }

            /// <inheritdoc/>
            public void Erase()
            {
                ImageData = null;
                IsLoaded = false;
            }

            /// <inheritdoc/>
            public void ImportFileInto()
            {
                // Future: Implement external image loading
            }

            /// <inheritdoc/>
            public  void CopyToClipBoard()
            {
                // Future: Copy image to platform clipboard
            }

            /// <inheritdoc/>
            public void PasteClipBoardInto()
            {
                // Future: Paste image from clipboard into member
            }

            /// <inheritdoc/>
            public ILingoMember Duplicate()
            {
                var clone = new LingoMemberPicture(Number, Name)
                {
                    ImageData = ImageData != null ? (byte[])ImageData.Clone() : null,
                    Width = Width,
                    Height = Height,
                    Format = Format,
                    Comments = Comments,
                    PurgePriority = PurgePriority
                };
                return clone;
            }
        }
    }

}
