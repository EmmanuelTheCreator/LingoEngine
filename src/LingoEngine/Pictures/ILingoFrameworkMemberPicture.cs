namespace LingoEngine.Pictures
{
    public interface ILingoFrameworkMemberPicture
    {
       
        /// <summary>
        /// Raw image data (e.g., pixel data or encoded image format).
        /// This field is implementation-independent; renderers may interpret this as needed.
        /// </summary>
        byte[]? ImageData { get;  }
        /// <summary>
        /// Indicates whether this image is loaded into memory.
        /// Corresponds to: member.picture.loaded
        /// </summary>
        bool IsLoaded { get; }
        /// <summary>
        /// Optional MIME type or encoding format (e.g., "image/png", "image/jpeg")
        /// </summary>
        string Format { get;  }
        int Width { get;  }
        int Height { get;  }

        void CopyToClipBoard();
        void Erase();
        void PasteClipBoardInto();
        void Preload();
        void Unload();
    }
}
