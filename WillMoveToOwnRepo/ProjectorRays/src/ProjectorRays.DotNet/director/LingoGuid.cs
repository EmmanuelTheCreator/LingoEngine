using ProjectorRays.Common;

namespace ProjectorRays.Director;

/// <summary>
/// 128-bit identifier used by Director to label compression types and other
/// resources. This mirrors the C++ <c>MoaID</c> structure but is renamed to
/// <c>LingoGuid</c> to avoid conflicts with <see cref="System.Guid"/>.
/// </summary>
public unsafe struct LingoGuid
{
    public uint Data1;
    public ushort Data2;
    public ushort Data3;
    public fixed byte Data4[8];

    public LingoGuid(uint d1, ushort d2, ushort d3,
        byte d40, byte d41, byte d42, byte d43,
        byte d44, byte d45, byte d46, byte d47)
    {
        Data1 = d1;
        Data2 = d2;
        Data3 = d3;
        Data4[0] = d40; Data4[1] = d41; Data4[2] = d42; Data4[3] = d43;
        Data4[4] = d44; Data4[5] = d45; Data4[6] = d46; Data4[7] = d47;
    }

    /// <summary>Read the GUID fields from the given stream.</summary>
    public void Read(ReadStream stream)
    {
        Data1 = stream.ReadUint32();
        Data2 = stream.ReadUint16();
        Data3 = stream.ReadUint16();
        for (int i = 0; i < 8; i++)
            Data4[i] = stream.ReadUint8();
    }

    public override readonly string ToString()
    {
        unsafe
        {
            fixed (byte* d = Data4)
            {
                return string.Format(
                    "{0:X8}-{1:X4}-{2:X4}-{3:X2}{4:X2}-{5:X2}{6:X2}{7:X2}{8:X2}{9:X2}{10:X2}",
                    Data1, Data2, Data3,
                    d[0], d[1], d[2], d[3], d[4], d[5], d[6], d[7]);
            }
        }
    }

    public readonly bool Equals(LingoGuid other)
    {
        if (Data1 != other.Data1 || Data2 != other.Data2 || Data3 != other.Data3)
            return false;
        for (int i = 0; i < 8; i++)
            if (Data4[i] != other.Data4[i])
                return false;
        return true;
    }

    public static bool operator ==(LingoGuid left, LingoGuid right) => left.Equals(right);
    public static bool operator !=(LingoGuid left, LingoGuid right) => !left.Equals(right);

    public override bool Equals(object? obj) => obj is LingoGuid other && Equals(other);
    public override int GetHashCode()
    {
        var hc = new HashCode();
        hc.Add(Data1);
        hc.Add(Data2);
        hc.Add(Data3);
        for (int i = 0; i < 8; i++)
            hc.Add(Data4[i]);
        return hc.ToHashCode();
    }
}

public static class LingoGuidConstants
{
    public static readonly LingoGuid FONTMAP_COMPRESSION_GUID = new(0x8A4679A1, 0x3720, 0x11D0, 0x92, 0x23, 0x00, 0xA0, 0xC9, 0x08, 0x68, 0xB1);
    public static readonly LingoGuid NULL_COMPRESSION_GUID = new(0xAC99982E, 0x005D, 0x0D50, 0x00, 0x00, 0x08, 0x00, 0x07, 0x37, 0x7A, 0x34);
    public static readonly LingoGuid SND_COMPRESSION_GUID = new(0x7204A889, 0xAFD0, 0x11CF, 0xA2, 0x22, 0x00, 0xA0, 0x24, 0x53, 0x44, 0x4C);
    public static readonly LingoGuid ZLIB_COMPRESSION_GUID = new(0xAC99E904, 0x0070, 0x0B36, 0x00, 0x00, 0x08, 0x00, 0x07, 0x37, 0x7A, 0x34);
}
