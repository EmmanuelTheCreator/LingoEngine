using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Compression;
using System.Reflection.PortableExecutable;
using System.Text;

namespace ProjectorRays.Common;

public enum Endianness
{
    BigEndian = 0,
    LittleEndian = 1
}

public class BufferView
{
    protected byte[] _data;
    protected int _offset;
    protected int _size;

    public BufferView()
    {
        _data = Array.Empty<byte>();
        _offset = 0;
        _size = 0;
    }

    public BufferView(byte[] data, int size)
        : this(data, 0, size) {}

    public BufferView(byte[] data, int offset, int size)
    {
        _data = data;
        _offset = offset;
        _size = size;
    }

    public int Size => _size;
    public byte[] Data => _data;
    public int Offset => _offset;
    public static readonly BufferView Empty = new BufferView(Array.Empty<byte>(), 0, 0);

    public string LogHex(int length = 256, int bytesPerLine = 16)
    {
        var sb = new StringBuilder();
        int start = _offset;
        int limit = Math.Min(_size, length);

        for (int i = 0; i < limit; i += bytesPerLine)
        {
            sb.Append($"{i:X4}: ");
            for (int j = 0; j < bytesPerLine && i + j < limit; j++)
            {
                sb.Append($"{_data[start + i + j]:X2} ");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }
}

public class RaysStream : BufferView
{
    protected int _pos;
    public Endianness Endianness { get; set; }

    public RaysStream(byte[] d, int s, Endianness e = Endianness.BigEndian, int p = 0)
        : base(d, 0, s)
    {
        _pos = p;
        Endianness = e;
    }

    public RaysStream(BufferView view, Endianness e = Endianness.BigEndian, int p = 0)
        : base(view.Data, view.Offset, view.Size)
    {
        _pos = p;
        Endianness = e;
    }

    public int Pos => _pos;

    public long Seek(long offset, SeekOrigin whence)
    {
        switch (whence)
        {
            case SeekOrigin.Begin:
                _pos = (int)offset;
                break;
            case SeekOrigin.Current:
                _pos += (int)offset;
                break;
            case SeekOrigin.End:
                _pos = _size + (int)offset;
                break;
            default:
                return -1;
        }
        return _pos;
    }

    public void Seek(int pos) => _pos = pos;

    public void Skip(long offset) => _pos += (int)offset;

    public bool Eof => _pos >= _size;

    public bool PastEOF => _pos > _size;
}

public class ReadStream : RaysStream
{
    private readonly RayStreamAnnotatorDecorator? _annotator;

    public ReadStream(byte[] d, int s, Endianness e = Endianness.BigEndian, int p = 0,
                      RayStreamAnnotatorDecorator? annotator = null)
        : base(d, s, e, p)
    {
        _annotator = annotator;
    }

    public ReadStream(BufferView view, Endianness e = Endianness.BigEndian, int p = 0,
                      RayStreamAnnotatorDecorator? annotator = null)
        : base(view, e, p)
    {
        _annotator = annotator;
    }
    public int Position
    {
        get => _pos;
        set
        {
            if (value < 0 || value > _size)
                throw new ArgumentOutOfRangeException(nameof(value), "Position out of bounds.");
            _pos = value;
        }
    }

    public int BytesLeft => Size - Position;
    public byte Peek(int offset = 0)
    {
        int targetPos = Position + offset;
        if (targetPos < 0 || targetPos >= Size)
            throw new IndexOutOfRangeException($"Cannot peek at offset {offset}, out of bounds.");
        return Data[targetPos];
    }


    public BufferView ReadByteView(int len, string description = "", Dictionary<string, int>? keys = null)
    {
        int current = _pos;
        var view = new BufferView(_data, _offset + _pos, len);
        _pos += len;
        _annotator?.Annotate(_offset + current, len, description, keys);
        return view;
    }
    public byte[] ReadBytes(int len, string description = "", Dictionary<string, int>? keys = null)
    {
        if (PastEOF || _pos + len > _size)
            throw new InvalidOperationException("ReadStream.ReadBytes: Read past end of stream!");

        var result = new byte[len];
        Array.Copy(_data, _offset + _pos, result, 0, len);
        _pos += len;
        _annotator?.Annotate(_offset + _pos - len, len, description, keys);
        return result;
    }
    public int ReadUpToBytes(int len, byte[] dest, string description = "", Dictionary<string, int>? keys = null)
    {
        if (Eof)
            return 0;
        if (_pos + len > _size)
            len = _size - _pos;
        Array.Copy(_data, _offset + _pos, dest, 0, len);
        int current = _pos;
        _pos += len;
        _annotator?.Annotate(_offset + current, len, description, keys);
        return len;
    }
    public byte[] ReadBytesAt(int offset, int length)
    {
        return _data.Skip(offset).Take(length).ToArray();
    }
    public int ReadZlibBytes(int len, byte[] dest, int destLen, string description = "", Dictionary<string, int>? keys = null)
    {
        var view = ReadByteView(len, description, keys);
        if (PastEOF)
            throw new InvalidOperationException("ReadStream.ReadZlibBytes: Read past end of stream!");

        using var ms = new MemoryStream(view.Data, view.Offset, view.Size);
        using var zs = new ZLibStream(ms, CompressionMode.Decompress);
        return zs.Read(dest, 0, destLen);
    }

    public byte ReadUint8(string description = "", Dictionary<string, int>? keys = null)
    {
        int p = _pos;
        _pos += 1;
        if (PastEOF)
            throw new InvalidOperationException("ReadStream.ReadUint8: Read past end of stream!");
        _annotator?.Annotate(_offset + p, 1, description, keys);
        return _data[_offset + p];
    }

    public sbyte ReadInt8(string description = "", Dictionary<string, int>? keys = null) => (sbyte)ReadUint8(description, keys);

    public ushort ReadUint16(string description = "", Dictionary<string, int>? keys = null)
    {
        int p = _pos;
        _pos += 2;
        if (PastEOF)
            throw new InvalidOperationException("ReadStream.ReadUint16: Read past end of stream!");
        _annotator?.Annotate(_offset + p, 2, description, keys);
        return Endianness == Endianness.LittleEndian
            ? BinaryPrimitives.ReadUInt16LittleEndian(_data.AsSpan(_offset + p, 2))
            : BinaryPrimitives.ReadUInt16BigEndian(_data.AsSpan(_offset + p, 2));
    }

    public short ReadInt16(string description = "", Dictionary<string, int>? keys = null) => (short)ReadUint16(description, keys);

    public uint ReadUint32(string description = "", Dictionary<string, int>? keys = null)
    {
        int p = _pos;
        _pos += 4;
        if (PastEOF)
            throw new InvalidOperationException("ReadStream.ReadUint32: Read past end of stream!");
        _annotator?.Annotate(_offset + p, 4, description, keys);
        return Endianness == Endianness.LittleEndian
            ? BinaryPrimitives.ReadUInt32LittleEndian(_data.AsSpan(_offset + p, 4))
            : BinaryPrimitives.ReadUInt32BigEndian(_data.AsSpan(_offset + p, 4));
    }

    public int ReadInt32(string description = "", Dictionary<string, int>? keys = null) => (int)ReadUint32(description, keys);

    public float ReadFloat32(string description = "", Dictionary<string, int>? keys = null)
    {
        return BitConverter.Int32BitsToSingle((int)ReadUint32(description, keys));
    }

    public double ReadDouble(string description = "", Dictionary<string, int>? keys = null)
    {
        int p = _pos;
        _pos += 8;
        if (PastEOF)
            throw new InvalidOperationException("ReadStream.ReadDouble: Read past end of stream!");
        _annotator?.Annotate(_offset + p, 8, description, keys);
        ulong val = Endianness == Endianness.LittleEndian
            ? BinaryPrimitives.ReadUInt64LittleEndian(_data.AsSpan(_offset + p, 8))
            : BinaryPrimitives.ReadUInt64BigEndian(_data.AsSpan(_offset + p, 8));
        return BitConverter.Int64BitsToDouble((long)val);
    }
    public float ReadFloat(string description = "", Dictionary<string, int>? keys = null)
    {
        int p = _pos;
        _pos += 4; // 4 bytes for a float
        if (PastEOF)
            throw new InvalidOperationException("ReadStream.ReadFloat: Read past end of stream!");

        _annotator?.Annotate(_offset + p, 4, description, keys);

        // Read the 4-byte value and convert to float based on the Endianness
        uint rawValue = Endianness == Endianness.LittleEndian
            ? BinaryPrimitives.ReadUInt32LittleEndian(_data.AsSpan(_offset + p, 4))
            : BinaryPrimitives.ReadUInt32BigEndian(_data.AsSpan(_offset + p, 4));

        return BitConverter.ToSingle(BitConverter.GetBytes(rawValue), 0);
    }

    public float ReadFixed16_16(string description = "", Dictionary<string, int>? keys = null)
    {
        int p = _pos;
        _pos += 4;

        if (PastEOF)
            throw new InvalidOperationException("ReadStream.ReadFixed16_16: Read past end of stream!");

        _annotator?.Annotate(_offset + p, 4, description, keys);

        int raw = Endianness == Endianness.LittleEndian
            ? BinaryPrimitives.ReadInt32LittleEndian(_data.AsSpan(_offset + p, 4))
            : BinaryPrimitives.ReadInt32BigEndian(_data.AsSpan(_offset + p, 4));

        return raw / 65536f;
    }

    public double ReadAppleFloat80(string description = "", Dictionary<string, int>? keys = null)
    {
        int p = _pos;
        _pos += 10;
        if (PastEOF)
            throw new InvalidOperationException("ReadStream.ReadAppleFloat80: Read past end of stream!");
        _annotator?.Annotate(_offset + p, 10, description, keys);

        ushort exponent = BinaryPrimitives.ReadUInt16BigEndian(_data.AsSpan(_offset + p, 2));
        ulong f64sign = (ulong)(exponent & 0x8000) << 48;
        exponent &= 0x7fff;
        ulong fraction = BinaryPrimitives.ReadUInt64BigEndian(_data.AsSpan(_offset + p + 2, 8));
        fraction &= 0x7fffffffffffffffUL;
        ulong f64exp = 0;
        if (exponent == 0)
        {
            f64exp = 0;
        }
        else if (exponent == 0x7fff)
        {
            f64exp = 0x7ff;
        }
        else
        {
            int normexp = (int)exponent - 0x3fff;
            if (-0x3fe > normexp || normexp >= 0x3ff)
                throw new InvalidOperationException("Constant float exponent too big for a double");
            f64exp = (ulong)(normexp + 0x3ff);
        }
        f64exp <<= 52;
        ulong f64fract = fraction >> 11;
        ulong f64bin = f64sign | f64exp | f64fract;
        return BitConverter.Int64BitsToDouble((long)f64bin);
    }

    public uint ReadVarInt(string description = "", Dictionary<string, int>? keys = null)
    {
        uint val = 0;
        byte b;
        do
        {
            b = ReadUint8(description, keys);
            val = (val << 7) | (uint)(b & 0x7f);
        } while ((b >> 7) != 0);
        return val;
    }

    public string ReadString(int len, string description = "", Dictionary<string, int>? keys = null)
    {
        int p = _pos;
        _pos += len;
        if (PastEOF)
            throw new InvalidOperationException("ReadStream.ReadString: Read past end of stream!");
        _annotator?.Annotate(_offset + p, len, description, keys);
        return Encoding.ASCII.GetString(_data, _offset + p, len);
    }

    public string ReadCString(string description = "", Dictionary<string, int>? keys = null)
    {
        var sb = new StringBuilder();
        byte ch = ReadUint8(description, keys);
        while (ch != 0)
        {
            sb.Append((char)ch);
            ch = ReadUint8(description, keys);
        }
        return sb.ToString();
    }

    public string ReadPascalString(string description = "", Dictionary<string, int>? keys = null)
    {
        int len = ReadUint8(description, keys);
        return ReadString(len, description, keys);
    }

    public byte PeekChar()
    {
        if (_pos >= _size)
            throw new InvalidOperationException("PeekChar: position out of bounds.");
        return _data[_offset + _pos];
    }

    public byte[] PeekBytes(int count)
    {
        if (_pos + count > _size)
            count = _size - _pos;
        var buffer = new byte[count];
        Array.Copy(_data, _offset + _pos, buffer, 0, count);
        return buffer;
    }
    public byte[] PeekBytesAt(int pos, int count)
    {
        if (pos < 0 || pos >= _size)
            return Array.Empty<byte>();

        if (pos + count > _size)
            count = _size - pos;

        var buffer = new byte[count];
        Array.Copy(_data, _offset + pos, buffer, 0, count);
        return buffer;
    }

    public string LogHex(int length = 256, int bytesPerLine = 16)
    {
        var sb = new StringBuilder();
        int limit = Math.Min(_data.Length, length);
        for (int i = 0; i < limit; i += bytesPerLine)
        {
            sb.Append($"{i:X4}: ");
            for (int j = 0; j < bytesPerLine && i + j < limit; j++)
            {
                sb.Append($"{_data[i + j]:X2} ");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    
}

public class WriteStream : RaysStream
{
    public WriteStream(byte[] d, int s, Endianness e = Endianness.BigEndian, int p = 0)
        : base(d, s, e, p) { }

    public WriteStream(BufferView view, Endianness e = Endianness.BigEndian, int p = 0)
        : base(view, e, p) { }

    public int WriteBytes(ReadOnlySpan<byte> data)
    {
        int p = _pos;
        _pos += data.Length;
        if (PastEOF)
            throw new InvalidOperationException("WriteStream.WriteBytes: Write past end of stream!");
        data.CopyTo(_data.AsSpan(_offset + p, data.Length));
        return data.Length;
    }

    public int WriteBytes(BufferView view) => WriteBytes(view.Data.AsSpan(view.Offset, view.Size));

    public void WriteUint8(byte value)
    {
        int p = _pos;
        _pos += 1;
        if (PastEOF)
            throw new InvalidOperationException("WriteStream.WriteUint8: Write past end of stream!");
        _data[_offset + p] = value;
    }

    public void WriteInt8(sbyte value) => WriteUint8((byte)value);

    public void WriteUint16(ushort value)
    {
        int p = _pos;
        _pos += 2;
        if (PastEOF)
            throw new InvalidOperationException("WriteStream.WriteUint16: Write past end of stream!");
        if (Endianness == Endianness.LittleEndian)
            BinaryPrimitives.WriteUInt16LittleEndian(_data.AsSpan(_offset + p, 2), value);
        else
            BinaryPrimitives.WriteUInt16BigEndian(_data.AsSpan(_offset + p, 2), value);
    }

    public void WriteInt16(short value) => WriteUint16((ushort)value);

    public void WriteUint32(uint value)
    {
        int p = _pos;
        _pos += 4;
        if (PastEOF)
            throw new InvalidOperationException("WriteStream.WriteUint32: Write past end of stream!");
        if (Endianness == Endianness.LittleEndian)
            BinaryPrimitives.WriteUInt32LittleEndian(_data.AsSpan(_offset + p, 4), value);
        else
            BinaryPrimitives.WriteUInt32BigEndian(_data.AsSpan(_offset + p, 4), value);
    }

    public void WriteInt32(int value) => WriteUint32((uint)value);

    public void WriteDouble(double value)
    {
        int p = _pos;
        _pos += 8;
        if (PastEOF)
            throw new InvalidOperationException("WriteStream.WriteDouble: Write past end of stream!");
        ulong v = (ulong)BitConverter.DoubleToInt64Bits(value);
        if (Endianness == Endianness.LittleEndian)
            BinaryPrimitives.WriteUInt64LittleEndian(_data.AsSpan(_offset + p, 8), v);
        else
            BinaryPrimitives.WriteUInt64BigEndian(_data.AsSpan(_offset + p, 8), v);
    }

    public void WriteString(string value)
    {
        WriteBytes(Encoding.ASCII.GetBytes(value));
    }

    public void WritePascalString(string value)
    {
        WriteUint8((byte)value.Length);
        WriteString(value);
    }
}
