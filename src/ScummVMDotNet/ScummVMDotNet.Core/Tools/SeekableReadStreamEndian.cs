using System;
using System.IO;
using System.Text;

namespace Director.Tools
{
    public class SeekableReadStreamEndian : IDisposable
    {
        private readonly Stream _stream;
        private readonly bool _isBigEndian;

        public SeekableReadStreamEndian(Stream stream, bool isBigEndian = true)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _isBigEndian = isBigEndian;
        }

        public long Position => _stream.Position;
        public long Length => _stream.Length;

        public byte ReadByte() => (byte)_stream.ReadByte();

        public ushort ReadUInt16()
        {
            Span<byte> buffer = stackalloc byte[2];
            _stream.Read(buffer);
            return _isBigEndian ? (ushort)(buffer[0] << 8 | buffer[1]) : (ushort)(buffer[1] << 8 | buffer[0]);
        }

        public short ReadInt16()
        {
            Span<byte> buffer = stackalloc byte[2];
            _stream.Read(buffer);
            return _isBigEndian ? (short)(buffer[0] << 8 | buffer[1]) : (short)(buffer[1] << 8 | buffer[0]);
        }

        public uint ReadUInt32()
        {
            Span<byte> buffer = stackalloc byte[4];
            _stream.Read(buffer);
            return _isBigEndian
                ? (uint)(buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3])
                : (uint)(buffer[3] << 24 | buffer[2] << 16 | buffer[1] << 8 | buffer[0]);
        }

        public int ReadInt32()
        {
            Span<byte> buffer = stackalloc byte[4];
            _stream.Read(buffer);
            return _isBigEndian
                ? buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3]
                : buffer[3] << 24 | buffer[2] << 16 | buffer[1] << 8 | buffer[0];
        }
        public uint ReadUInt32BE()
        {
            var bytes = ReadBytesRequired(4);
            return (uint)(bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3]);
        }
        public ushort ReadUInt16BE()
        {
            var bytes = ReadBytesRequired(2);
            return (ushort)(bytes[0] << 8 | bytes[1]);
        }


        public string ReadStringZ()
        {
            using var ms = new MemoryStream();
            while (true)
            {
                int b = _stream.ReadByte();
                if (b == -1 || b == 0)
                    break;
                ms.WriteByte((byte)b);
            }
            return Encoding.ASCII.GetString(ms.ToArray());
        }

        public string ReadString(int length)
        {
            var buffer = new byte[length];
            _stream.Read(buffer, 0, length);
            return Encoding.ASCII.GetString(buffer);
        }

        public void Seek(long offset, SeekOrigin origin)
        {
            _stream.Seek(offset, origin);
        }
        public byte[] ReadBytesRequired(int count)
        {
            byte[] buffer = new byte[count];
            int bytesRead = _stream.Read(buffer, 0, count);
            if (bytesRead != count)
                throw new EndOfStreamException($"Expected {count} bytes but only read {bytesRead}");
            return buffer;
        }

        public void Dispose()
        {
            _stream.Dispose();
        }

        public bool Eos => _stream.Position >= _stream.Length;

        public byte[] ReadBytes(int count)
        {
            var buffer = new byte[count];
            _stream.Read(buffer, 0, count);
            return buffer;
        }

        public string ReadString(int offset, int length)
        {
            if (offset > _stream.Length)
                throw new ArgumentOutOfRangeException(nameof(offset), "Offset exceeds stream length.");

            if (offset + length > _stream.Length)
                throw new ArgumentOutOfRangeException(nameof(length), "Offset + length exceeds stream length.");

            long originalPosition = _stream.Position;
            _stream.Seek(offset, SeekOrigin.Begin);

            byte[] bytes = ReadBytesRequired(length);
            _stream.Seek(originalPosition, SeekOrigin.Begin); // Restore original position

            // Assuming MacRoman or Windows-1252; you may adjust depending on actual file encoding
            return Encoding.GetEncoding("macintosh").GetString(bytes);
        }

    }
}
