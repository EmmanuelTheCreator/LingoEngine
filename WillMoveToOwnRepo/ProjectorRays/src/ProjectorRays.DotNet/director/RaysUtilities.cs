using System.Text;

namespace ProjectorRays.director;

public static class RaysUtilities
{
    public static uint HumanVersion(uint ver)
    {
        if (ver >= 1951) return 1200;
        if (ver >= 1922) return 1150;
        if (ver >= 1921) return 1100;
        if (ver >= 1851) return 1000;
        if (ver >= 1700) return 850;
        if (ver >= 1410) return 800;
        if (ver >= 1224) return 700;
        if (ver >= 1218) return 600;
        if (ver >= 1201) return 500;
        if (ver >= 1117) return 404;
        if (ver >= 1115) return 400;
        if (ver >= 1029) return 310;
        if (ver >= 1028) return 300;
        return 200;
    }
    public static string DumpHexWithAscii(byte[] data, int offset = 0, int length = 12, bool paintAscii = true)
    {
        int len = Math.Min(length, data.Length - offset);
        var hex = new StringBuilder(len * 3);
        var ascii = new StringBuilder(len);

        for (int i = 0; i < len; i++)
        {
            byte b = data[offset + i];
            hex.AppendFormat("{0:X2} ", b);
            if (paintAscii)
                ascii.Append(b >= 32 && b <= 126 ? (char)b : '.');
        }

        return $"{hex.ToString().TrimEnd()}"+ (paintAscii? $"|{ascii}|":"");
    }

    public static string LogHex(byte[] data, int length = 256, int bytesPerLine = 16)
    {
        var sb = new StringBuilder();
        int limit = Math.Min(data.Length, length);
        for (int i = 0; i < limit; i += bytesPerLine)
        {
            sb.Append($"{i:X4}: ");
            for (int j = 0; j < bytesPerLine && i + j < limit; j++)
            {
                sb.Append($"{data[i + j]:X2} ");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    public static string DumpAscii(byte[] data, int offset = 0, int length = 12)
    {
        int len = Math.Min(length, data.Length - offset);
        var sb = new StringBuilder(len);
        for (int i = 0; i < len; i++)
        {
            byte b = data[offset + i];
            // If printable ASCII range 32..126, show char, else dot
            if (b >= 32 && b <= 126)
                sb.Append((char)b);
            else
                sb.Append('.');
        }
        return sb.ToString();
    }
    public static string VersionNumber(uint ver, string fverVersionString)
    {
        uint major = ver / 100;
        uint minor = ver / 10 % 10;
        uint patch = ver % 10;

        if (string.IsNullOrEmpty(fverVersionString))
        {
            string res = major + "." + minor;
            if (patch != 0)
                res += "." + patch;
            return res;
        }
        else
        {
            return fverVersionString;
        }
    }

    public static string VersionString(uint ver, string fverVersionString)
    {
        uint major = ver / 100;
        string versionNum = VersionNumber(ver, fverVersionString);

        if (major >= 11)
            return "Adobe Director " + versionNum;
        if (major == 10)
            return "Macromedia Director MX 2004 (" + versionNum + ")";
        if (major == 9)
            return "Macromedia Director MX (" + versionNum + ")";
        return "Macromedia Director " + versionNum;
    }
}
