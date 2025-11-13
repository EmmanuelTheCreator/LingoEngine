using BlingoEngine.IO.Legacy.Data;
using System;
using System.Buffers.Binary;
using System.Globalization;
using System.IO;
using System.Text;

namespace BlingoEngine.IO.Legacy.Tools;

/// <summary>
/// Represents the byte ordering used when reading multi-byte values.
/// </summary>
public enum BlEndianness
{
    BigEndian = 0,
    LittleEndian = 1
}

/// <summary>
/// Wraps a <see cref="Stream"/> with endian-aware helpers for consuming Director's big- and little-endian byte sequences.
/// </summary>
public sealed class BlStreamReader
{
    private readonly Stream _baseStream;
    private readonly byte[] _scratch = new byte[8];

    /// <summary>
    /// Gets the underlying stream containing the Director bytes.
    /// </summary>
    public Stream BaseStream => _baseStream;

    /// <summary>
    /// Gets or sets the byte ordering used when decoding multi-byte values.
    /// </summary>
    public BlEndianness Endianness { get; set; } = BlEndianness.BigEndian;

    /// <summary>
    /// Gets or sets the current byte offset within the stream.
    /// </summary>
    public long Position
    {
        get => _baseStream.Position;
        set => _baseStream.Seek(value, SeekOrigin.Begin);
    }

    /// <summary>
    /// Gets the total stream length in bytes.
    /// </summary>
    public long Length => _baseStream.Length;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlStreamReader"/> class.
    /// </summary>
    /// <param name="stream">Underlying stream that exposes the Director bytes.</param>
    public BlStreamReader(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        if (!stream.CanRead)
        {
            throw new ArgumentException("Stream must be readable.", nameof(stream));
        }

        if (!stream.CanSeek)
        {
            throw new ArgumentException("Stream must support seeking.", nameof(stream));
        }

        _baseStream = stream;
    }

    /// <summary>
    /// Reads a single unsigned byte from the current stream position.
    /// </summary>
    /// <returns>The byte value located at the current position.</returns>
    public byte ReadByte()
    {
        int value = _baseStream.ReadByte();
        if (value < 0)
        {
            throw new EndOfStreamException();
        }

        return (byte)value;
    }

    /// <summary>
    /// Reads a signed byte from the current stream position.
    /// </summary>
    /// <returns>The signed byte value located at the current position.</returns>
    public sbyte ReadSByte() => unchecked((sbyte)ReadByte());

    /// <summary>
    /// Reads a 16-bit unsigned integer respecting the configured endianness.
    /// </summary>
    /// <returns>The decoded 16-bit value.</returns>
    public ushort ReadUInt16() => ReadUInt16(Endianness);

    /// <summary>
    /// Reads a 16-bit signed integer respecting the configured endianness.
    /// </summary>
    /// <returns>The decoded 16-bit value interpreted as signed.</returns>
    public short ReadInt16() => unchecked((short)ReadUInt16());

    /// <summary>
    /// Reads a 32-bit unsigned integer respecting the configured endianness.
    /// </summary>
    /// <returns>The decoded 32-bit value.</returns>
    public uint ReadUInt32() => ReadUInt32(Endianness);

    /// <summary>
    /// Reads a 32-bit signed integer respecting the configured endianness.
    /// </summary>
    /// <returns>The decoded 32-bit value interpreted as signed.</returns>
    public int ReadInt32() => unchecked((int)ReadUInt32());

    /// <summary>
    /// Reads a 64-bit unsigned integer respecting the configured endianness.
    /// </summary>
    /// <returns>The decoded 64-bit value.</returns>
    public ulong ReadUInt64() => ReadUInt64(Endianness);

    /// <summary>
    /// Reads a 64-bit signed integer respecting the configured endianness.
    /// </summary>
    /// <returns>The decoded 64-bit value interpreted as signed.</returns>
    public long ReadInt64() => unchecked((long)ReadUInt64());

    /// <summary>
    /// Reads a four-character tag directly from the byte stream.
    /// </summary>
    /// <returns>The decoded <see cref="BlTag"/>.</returns>
    public BlTag ReadTag()
    {
        var span = _scratch.AsSpan(0, 4);
        _baseStream.ReadExactly(span);
        if (Endianness == BlEndianness.LittleEndian)
        {
            span.Reverse();
        }

        if (!BlTag.TryRead(span, out var tag))
        {
            throw new InvalidDataException("Unable to decode tag from stream.");
        }

        return tag;
    }

    /// <summary>
    /// Reads a four-character tag and returns it as a string.
    /// </summary>
    /// <returns>The decoded tag value.</returns>
    public string ReadTagAsString() => ReadTag().Value;

    /// <summary>
    /// Reads a block of bytes of the specified length.
    /// </summary>
    /// <param name="count">Number of bytes to read.</param>
    /// <returns>A new array containing the requested bytes.</returns>
    public byte[] ReadBytes(int count)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        var buffer = new byte[count];
        _baseStream.ReadExactly(buffer);
        return buffer;
    }

    /// <summary>
    /// Reads the specified number of bytes and formats them as hexadecimal text.
    /// </summary>
    /// <param name="count">Number of bytes to read.</param>
    /// <param name="bytesPerLine">Number of bytes rendered on each output line.</param>
    /// <returns>Hexadecimal representation of the requested bytes.</returns>
    public string ReadBytesAsHexString(int count, int bytesPerLine = 16)
    {
        var bytes = ReadBytes(count);
        return ToHexString(bytes, bytesPerLine);
    }

    /// <summary>
    /// Reads bytes exactly into the provided destination span.
    /// </summary>
    /// <param name="destination">Span that receives the bytes from the stream.</param>
    public void ReadExactly(Span<byte> destination)
    {
        _baseStream.ReadExactly(destination);
    }

    /// <summary>
    /// Skips the specified number of bytes relative to the current position.
    /// </summary>
    /// <param name="count">Number of bytes to advance.</param>
    public void Skip(long count)
    {
        if (count == 0)
        {
            return;
        }

        _baseStream.Seek(count, SeekOrigin.Current);
    }

    /// <summary>
    /// Advances the stream to the next even byte boundary when required by chunk alignment rules.
    /// </summary>
    public void AlignToEven()
    {
        if ((Position & 1) != 0)
        {
            Skip(1);
        }
    }

    /// <summary>
    /// Reads a variable-length 32-bit unsigned integer using Director's 7-bit continuation encoding.
    /// </summary>
    /// <returns>The decoded 32-bit value.</returns>
    public uint ReadVariableUInt32()
    {
        uint value = 0;
        while (true)
        {
            byte b = ReadByte();
            value = value << 7 | (uint)(b & 0x7F);
            if ((b & 0x80) == 0)
            {
                return value;
            }
        }
    }

    /// <summary>
    /// Reads a null-terminated ASCII string from the current position.
    /// </summary>
    /// <returns>The decoded string.</returns>
    public string ReadCString()
    {
        var builder = new StringBuilder();
        byte ch = ReadByte();
        while (ch != 0)
        {
            builder.Append((char)ch);
            ch = ReadByte();
        }

        return builder.ToString();
    }

    /// <summary>
    /// Reads an ASCII string of the specified length.
    /// </summary>
    /// <param name="length">Number of bytes that compose the string.</param>
    /// <returns>The decoded string.</returns>
    public string ReadAsciiString(int length)
    {
        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        if (length == 0)
        {
            return string.Empty;
        }

        var buffer = ReadBytes(length);
        return Encoding.ASCII.GetString(buffer);
    }
    public string ReadStringWithFirstByteLength()
    {
        var length = ReadByte();
        var buffer = ReadBytes(length);
        return Encoding.ASCII.GetString(buffer);
    }

    /// <summary>
    /// Formats bytes as a hexadecimal string, optionally prefixing each line with the byte offset (e.g. <c>0x00000000:</c>).
    /// </summary>
    /// <param name="data">Byte span to format.</param>
    /// <param name="bytesPerLine">Number of bytes shown per output line.</param>
    /// <param name="includeAddresses">Whether to prefix each line with the starting offset.</param>
    /// <returns>Formatted hexadecimal text for the supplied bytes.</returns>
    public static string ToHexString(ReadOnlySpan<byte> data, int bytesPerLine = 16, bool includeAddresses = false)
    {
        if (bytesPerLine <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bytesPerLine));
        }

        if (data.IsEmpty)
        {
            return string.Empty;
        }

        var builder = new StringBuilder(data.Length * 3);
        for (int i = 0; i < data.Length; i++)
        {
            if (i % bytesPerLine == 0)
            {
                if (i > 0)
                {
                    builder.AppendLine();
                }

                if (includeAddresses)
                {
                    builder.Append($"0x{i:X8}: ");
                }
            }
            else
            {
                builder.Append(' ');
            }

            builder.Append(data[i].ToString("X2", CultureInfo.InvariantCulture));
        }

        return builder.ToString();
    }

    /// <summary>
    /// Formats bytes as a hexadecimal string, optionally prefixing addresses on each line.
    /// </summary>
    /// <param name="data">Byte array to format.</param>
    /// <param name="bytesPerLine">Number of bytes shown per output line.</param>
    /// <param name="includeAddresses">Whether to prefix each line with the starting offset.</param>
    /// <returns>Formatted hexadecimal text for the supplied bytes.</returns>
    public static string ToHexString(byte[] data, int bytesPerLine = 16, bool includeAddresses = false)
    {
        ArgumentNullException.ThrowIfNull(data);
        return ToHexString(data.AsSpan(), bytesPerLine, includeAddresses);
    }

    private ushort ReadUInt16(BlEndianness endianness)
    {
        var span = _scratch.AsSpan(0, 2);
        _baseStream.ReadExactly(span);
        return endianness == BlEndianness.LittleEndian
            ? BinaryPrimitives.ReadUInt16LittleEndian(span)
            : BinaryPrimitives.ReadUInt16BigEndian(span);
    }

    private uint ReadUInt32(BlEndianness endianness)
    {
        var span = _scratch.AsSpan(0, 4);
        _baseStream.ReadExactly(span);
        return endianness == BlEndianness.LittleEndian
            ? BinaryPrimitives.ReadUInt32LittleEndian(span)
            : BinaryPrimitives.ReadUInt32BigEndian(span);
    }

    private ulong ReadUInt64(BlEndianness endianness)
    {
        var span = _scratch.AsSpan(0, 8);
        _baseStream.ReadExactly(span);
        return endianness == BlEndianness.LittleEndian
            ? BinaryPrimitives.ReadUInt64LittleEndian(span)
            : BinaryPrimitives.ReadUInt64BigEndian(span);
    }
}
