using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace BlingoEngine.IO.Legacy.Data;

/// <summary>
/// Represents a four-character code stored directly in the byte stream.
/// </summary>
public readonly struct BlTag : IEquatable<BlTag>, IEquatable<string>
{
    private static readonly Dictionary<string, BlTag> _registry = new(StringComparer.Ordinal);
    private static readonly object _syncRoot = new();

    public string Value { get; }

    private BlTag(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException("Tag value cannot be null or empty.", nameof(value));
        }

        if (value.Length != 4)
        {
            throw new ArgumentException("Tags must contain exactly four characters.", nameof(value));
        }

        Value = value;
    }

    public static BlTag FromString(string value) => Get(value);

    public static bool TryParse(string? value, out BlTag tag)
    {
        if (!string.IsNullOrEmpty(value) && value.Length == 4)
        {
            tag = Get(value);
            return true;
        }

        tag = default;
        return false;
    }

    public static bool TryRead(ReadOnlySpan<byte> source, out BlTag tag)
    {
        return TryRead(source, 0, out tag);
    }

    public static bool TryRead(ReadOnlySpan<byte> source, int offset, out BlTag tag)
    {
        if (offset < 0 || source.Length - offset < 4)
        {
            tag = default;
            return false;
        }

        var value = Encoding.ASCII.GetString(source.Slice(offset, 4));
        return TryParse(value, out tag);
    }

    public static BlTag Read(ReadOnlySpan<byte> source, int offset = 0)
    {
        if (!TryRead(source, offset, out var tag))
        {
            throw new ArgumentException("Insufficient data to read a four character tag.", nameof(source));
        }

        return tag;
    }

    public static bool Equals(ReadOnlySpan<byte> data, int offset, BlTag tag)
    {
        if (offset < 0 || data.Length - offset < 4)
        {
            return false;
        }

        return data[offset] == tag.Value[0]
            && data[offset + 1] == tag.Value[1]
            && data[offset + 2] == tag.Value[2]
            && data[offset + 3] == tag.Value[3];
    }

    public static bool Equals(byte[]? data, int offset, BlTag tag)
    {
        return data is not null && Equals(data.AsSpan(), offset, tag);
    }

    public static IReadOnlyDictionary<string, BlTag> Known => _registry;

    public static implicit operator string(BlTag tag) => tag.Value;

    public static explicit operator BlTag(string value) => FromString(value);

    public static bool operator ==(BlTag left, BlTag right) => left.Equals(right);

    public static bool operator !=(BlTag left, BlTag right) => !left.Equals(right);

    public static bool operator ==(BlTag left, string? right) => left.Equals(right);

    public static bool operator !=(BlTag left, string? right) => !left.Equals(right);

    public static bool operator ==(string? left, BlTag right) => right.Equals(left);

    public static bool operator !=(string? left, BlTag right) => !right.Equals(left);

    public override string ToString() => Value;

    public bool Equals(BlTag other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

    public bool Equals(string? other) => string.Equals(Value, other, StringComparison.Ordinal);

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj switch
        {
            BlTag tag => Equals(tag),
            string text => Equals(text),
            _ => false
        };
    }

    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);

    public uint ToUInt32BigEndian()
    {
        var buffer = Encoding.ASCII.GetBytes(Value);
        return BinaryPrimitives.ReadUInt32BigEndian(buffer);
    }

    static BlTag()
    {
        RIFX = Register("RIFX");
        XFIR = Register("XFIR");
        MV93 = Register("MV93");
        MC95 = Register("MC95");
        APPL = Register("APPL");
        FGDM = Register("FGDM");
        FGDC = Register("FGDC");
        Imap = Register("imap");
        Mmap = Register("mmap");
        KeyStar = Register("KEY*");
        Free = Register("free");
        Junk = Register("junk");
        Fver = Register("Fver");
        Fcdr = Register("Fcdr");
        Abmp = Register("ABMP");
        Fgei = Register("FGEI");
        Cast = Register("CAST");
        CasStar = Register("CAS*");
        // Cast info block
        Cinf = Register("Cinf");
        MCsL = Register("MCsL"); 

        Register("ediM");
        Register("BITD");
        Register("DIB ");
        Register("PICT");
        Register("ALFA");
        Register("Thum");
        Register("CASt");
        Register("Lscr");
        Register("Lctx");
        Register("LctX");
        Register("STXT");
        Register("XMED");
        Register("snd ");
        Register("sndS");
        Register("SND ");
    }

    public static BlTag Get(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value.Length != 4)
        {
            throw new ArgumentException("Tags must contain exactly four characters.", nameof(value));
        }

        return Register(value);
    }

    private static BlTag Register(string value)
    {
        lock (_syncRoot)
        {
            if (_registry.TryGetValue(value, out var existing))
            {
                return existing;
            }

            var tag = new BlTag(value);
            _registry[value] = tag;
            return tag;
        }
    }

    public static BlTag RIFX { get; }
    public static BlTag XFIR { get; }
    public static BlTag MV93 { get; }
    public static BlTag MC95 { get; }
    public static BlTag APPL { get; }
    public static BlTag FGDM { get; }
    public static BlTag FGDC { get; }
    public static BlTag Imap { get; }
    public static BlTag Mmap { get; }
    public static BlTag KeyStar { get; }
    public static BlTag Free { get; }
    public static BlTag Junk { get; }
    public static BlTag Fver { get; }
    public static BlTag MCsL { get; }
    
    public static BlTag Fcdr { get; }
    public static BlTag Abmp { get; }
    public static BlTag Fgei { get; }
    public static BlTag Cast { get; }
    /// <summary>
    /// Cast info block
    /// </summary>
    public static BlTag Cinf { get; }
    public static BlTag CasStar { get; }
}
