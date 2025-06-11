using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Director.IO
{
    using System;
    using System.IO;

    /// <summary>
    /// A read-only stream representing a window into an underlying stream.
    /// </summary>
    public class SliceStream : Stream
    {
        private readonly Stream _baseStream;
        private readonly long _start;
        private readonly long _length;
        private long _position;

        public SliceStream(Stream baseStream, long start, long length)
        {
            _baseStream = baseStream;
            _start = start;
            _length = length;
            _position = 0;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var toRead = (int)Math.Min(count, _length - _position);
            if (toRead <= 0) return 0;

            lock (_baseStream)
            {
                _baseStream.Position = _start + _position;
                int read = _baseStream.Read(buffer, offset, toRead);
                _position += read;
                return read;
            }
        }

        public override long Length => _length;
        public override long Position
        {
            get => _position;
            set { if (value < 0 || value > _length) throw new ArgumentOutOfRangeException(); _position = value; }
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Seek(long offset, SeekOrigin origin)
        {
            long newPos = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => _position + offset,
                SeekOrigin.End => _length + offset,
                _ => throw new ArgumentException()
            };
            if (newPos < 0 || newPos > _length) throw new ArgumentOutOfRangeException();
            _position = newPos;
            return _position;
        }

        public override void Flush() { }
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

}
