using System.Text;

namespace Director.IO
{
    /// <summary>
    /// A seekable stream based on a memory buffer with endian-aware reading.
    /// </summary>
    public class MemoryReadStreamEndian : SeekableReadStreamEndian
    {
        private readonly byte[] _buffer;

        /// <summary>
        /// Initializes a new memory-based stream with endian-aware access.
        /// </summary>
        /// <param name="data">The byte buffer to wrap.</param>
        /// <param name="length">The number of valid bytes to expose from the buffer.</param>
        /// <param name="isBigEndian">Whether the stream uses big-endian byte order.</param>
        public MemoryReadStreamEndian(byte[] data, int length, bool isBigEndian)
            : base(new MemoryStream(data, 0, length, false), isBigEndian)
        {
            _buffer = data[..length];
        }
    }

}
