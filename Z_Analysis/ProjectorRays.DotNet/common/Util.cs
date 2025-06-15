using System;
using System.Globalization;
using System.Text;

namespace ProjectorRays.Common;

public static class Util
{
    public static string FourCCToString(uint fourcc)
    {
        char[] chars = new char[4];
        chars[0] = (char)(fourcc >> 24);
        chars[1] = (char)(fourcc >> 16);
        chars[2] = (char)(fourcc >> 8);
        chars[3] = (char)fourcc;
        return EscapeString(new string(chars));
    }

    public static string FloatToString(double value)
    {
        string res = value.ToString("G17", CultureInfo.InvariantCulture);
        if (res.Contains(".") && res.EndsWith("0"))
        {
            res = res.TrimEnd('0');
            if (res.EndsWith("."))
                res = res.TrimEnd('.');
        }
        return res;
    }

    public static string ByteToString(byte b) => b.ToString("X2", CultureInfo.InvariantCulture);

    public static string EscapeString(string str) => EscapeString(str.AsSpan());

    public static string EscapeString(ReadOnlySpan<char> span)
    {
        StringBuilder res = new();
        foreach (char ch in span)
        {
            switch (ch)
            {
                case '"': res.Append("\\\""); break;
                case '\\': res.Append("\\\\"); break;
                case '\b': res.Append("\\b"); break;
                case '\f': res.Append("\\f"); break;
                case '\n': res.Append("\\n"); break;
                case '\r': res.Append("\\r"); break;
                case '\t': res.Append("\\t"); break;
                case '\v': res.Append("\\v"); break;
                default:
                    if (ch < 0x20 || ch > 0x7F)
                    {
                        res.Append("\\x").Append(ByteToString((byte)ch));
                    }
                    else
                    {
                        res.Append(ch);
                    }
                    break;
            }
        }
        return res.ToString();
    }

    public static int Stricmp(string a, string b) => string.Compare(a, b, StringComparison.OrdinalIgnoreCase);

    public static int CompareIgnoreCase(string a, string b) => Stricmp(a, b);
}
