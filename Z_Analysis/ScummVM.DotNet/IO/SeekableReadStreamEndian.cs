using Director.Primitives;
using System.Text;

namespace Director.IO
{
    /// <summary>
    /// Provides endian-aware reading from a seekable stream.
    /// </summary>
    public class SeekableReadStreamEndian : IDisposable
    {
        protected readonly Stream _stream;
        protected readonly bool _isBigEndian;

        public bool IsBigEndian => _isBigEndian;

        /// <summary>
        /// Initializes a new instance of the SeekableReadStreamEndian class.
        /// </summary>
        /// <param name="stream">The input stream.</param>
        /// <param name="isBigEndian">True if big-endian, false if little-endian.</param>
        public SeekableReadStreamEndian(Stream stream, bool isBigEndian)
        {
            _stream = stream;
            _isBigEndian = isBigEndian;
        }

        public long Position
        {
            get => _stream.Position;
            set => _stream.Position = value;
        }

        public long Length => _stream.Length;
        public Stream BaseStream => _stream;
        public bool EOF => _stream.Position >= _stream.Length;
        public void Seek(long offset)
        {
            _stream.Seek(offset, SeekOrigin.Begin);
        }
        public virtual byte ReadByte()
        {
            int val = _stream.ReadByte();
            if (val == -1)
                throw new EndOfStreamException();
            return (byte)val;
        }

        public virtual ushort ReadUInt16()
        {
            Span<byte> buffer = stackalloc byte[2];
            _stream.Read(buffer);
            return _isBigEndian
                ? (ushort)(buffer[0] << 8 | buffer[1])
                : (ushort)(buffer[1] << 8 | buffer[0]);
        }

        public virtual short ReadInt16() => (short)ReadUInt16();

        public virtual short ReadInt16BE()
        {
            var bytes = ReadBytesRequired(2);
            return (short)(bytes[0] << 8 | bytes[1]);
        }

        public virtual ushort ReadUInt16BE()
        {
            var bytes = ReadBytesRequired(2);
            return (ushort)(bytes[0] << 8 | bytes[1]);
        }

        public virtual uint ReadUInt32()
        {
            Span<byte> buffer = stackalloc byte[4];
            _stream.Read(buffer);
            return _isBigEndian
                ? (uint)(buffer[0] << 24 | buffer[1] << 16 | buffer[2] << 8 | buffer[3])
                : (uint)(buffer[3] << 24 | buffer[2] << 16 | buffer[1] << 8 | buffer[0]);
        }

        public virtual int ReadInt32() => (int)ReadUInt32();

        public virtual int ReadInt32BE()
        {
            var bytes = ReadBytesRequired(4);
            return bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3];
        }

        public virtual uint ReadUInt32BE()
        {
            var bytes = ReadBytesRequired(4);
            return (uint)(bytes[0] << 24 | bytes[1] << 16 | bytes[2] << 8 | bytes[3]);
        }

        public virtual byte[] ReadBytes(int count)
        {
            var buffer = new byte[count];
            _stream.Read(buffer, 0, count);
            return buffer;
        }

        /// <summary>
        /// Reads the specified number of bytes or throws if not enough data is available.
        /// </summary>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>A byte array of the requested length.</returns>
        public virtual byte[] ReadBytesRequired(int count)
        {
            byte[] buffer = new byte[count];
            int totalRead = 0;

            while (totalRead < count)
            {
                int read = _stream.Read(buffer, totalRead, count - totalRead);
                if (read == 0)
                    throw new EndOfStreamException($"Expected {count} bytes, but only read {totalRead}.");
                totalRead += read;
            }

            return buffer;
        }


        public virtual string ReadString(int length)
        {
            var buffer = new byte[length];
            _stream.Read(buffer, 0, length);
            return Encoding.ASCII.GetString(buffer);
        }

        public virtual string ReadString(int offset, int length)
        {
            if (offset > _stream.Length)
                throw new ArgumentOutOfRangeException(nameof(offset), "Offset exceeds stream length.");

            if (offset + length > _stream.Length)
                throw new ArgumentOutOfRangeException(nameof(length), "Offset + length exceeds stream length.");

            long originalPosition = _stream.Position;
            _stream.Seek(offset, SeekOrigin.Begin);

            byte[] bytes = ReadBytesRequired(length);
            _stream.Seek(originalPosition, SeekOrigin.Begin);

            return Encoding.GetEncoding("macintosh").GetString(bytes);
        }

        public virtual string ReadStringZ()
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

        public virtual string ReadPascalString()
        {
            int length = ReadByte();
            if (length == 0)
                return string.Empty;

            byte[] bytes = ReadBytesRequired(length);
            return Encoding.ASCII.GetString(bytes);
        }

        public virtual LingoRect ReadRect()
        {
            int top = ReadInt16();
            int left = ReadInt16();
            int bottom = ReadInt16();
            int right = ReadInt16();
            return new LingoRect(top, left, bottom, right);
        }

        public virtual void Seek(long offset, SeekOrigin origin) => _stream.Seek(offset, origin);

        public virtual void Dispose() => _stream.Dispose();

        public byte[] ReadAllBytes()
        {
            long originalPosition = _stream.Position;
            _stream.Seek(0, SeekOrigin.Begin);

            byte[] buffer = new byte[_stream.Length];
            _stream.Read(buffer, 0, buffer.Length);

            _stream.Seek(originalPosition, SeekOrigin.Begin); // Restore original position
            return buffer;
        }



        public void HexDump(int count)
        {
            long originalPos = BaseStream.Position;
            byte[] buffer = ReadBytes(count);
            Console.WriteLine($"HexDump @ {originalPos:X8}:");

            for (int i = 0; i < buffer.Length; i += 16)
            {
                StringBuilder hex = new();
                StringBuilder ascii = new();

                for (int j = 0; j < 16; j++)
                {
                    if (i + j < buffer.Length)
                    {
                        byte b = buffer[i + j];
                        hex.AppendFormat("{0:X2} ", b);
                        ascii.Append(b >= 32 && b < 127 ? (char)b : '.');
                    }
                    else
                    {
                        hex.Append("   ");
                        ascii.Append(' ');
                    }
                }

                Console.WriteLine($"{originalPos + i:X8}: {hex} {ascii}");
            }

            BaseStream.Seek(originalPos, SeekOrigin.Begin);
        }

        public uint ReadVarInt()
        {
            uint value = 0;
            byte b;
            do
            {
                b = ReadByte();
                value = (value << 7) | (uint)(b & 0x7F);
            } while ((b & 0x80) != 0);
            return value;
        }



    }
}
