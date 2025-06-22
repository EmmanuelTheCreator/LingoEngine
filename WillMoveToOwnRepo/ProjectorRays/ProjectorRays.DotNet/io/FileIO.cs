using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace ProjectorRays.IO;

using ProjectorRays.Common;

public static class FileIO
{
    public static readonly string PlatformLineEnding = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "\r\n" : "\n";

    public static bool ReadFile(string path, List<byte> buf)
    {
        try
        {
            byte[] data = File.ReadAllBytes(path);
            buf.Clear();
            buf.AddRange(data);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
    }

    public static void WriteFile(string path, string contents)
    {
        File.WriteAllText(path, contents);
    }

    public static void WriteFile(string path, byte[] contents)
    {
        File.WriteAllBytes(path, contents);
    }

    public static void WriteFile(string path, BufferView view)
    {
        byte[] slice = new byte[view.Size];
        Array.Copy(view.Data, view.Offset, slice, 0, view.Size);
        File.WriteAllBytes(path, slice);
    }

    public static string CleanFileName(string fileName)
    {
        var res = new System.Text.StringBuilder();
        foreach (char ch in fileName)
        {
            switch (ch)
            {
                case '<':
                case '>':
                case ':':
                case '"':
                case '/':
                case '\\':
                case '|':
                case '?':
                case '*':
                    res.Append('_');
                    break;
                default:
                    res.Append(ch);
                    break;
            }
        }
        return res.ToString();
    }
}
