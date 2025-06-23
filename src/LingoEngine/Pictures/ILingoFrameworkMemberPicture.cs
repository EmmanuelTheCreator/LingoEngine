using LingoEngine.FrameworkCommunication;

namespace LingoEngine.Pictures
{
    public interface ILingoFrameworkMemberPicture : ILingoFrameworkMember
    {
       
        /// <summary>
        /// Raw image data (e.g., pixel data or encoded image format).
        /// This field is implementation-independent; renderers may interpret this as needed.
        /// </summary>
        byte[]? ImageData { get;  }
       
        /// <summary>
        /// Optional MIME type or encoding format (e.g., "image/png", "image/jpeg")
        /// </summary>
        string Format { get;  }
        int Width { get;  }
        int Height { get;  }

        void SetImageData(byte[] bytes);
    }
}
