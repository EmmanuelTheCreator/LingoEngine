using System;
using System.Collections.Generic;
namespace Director
{
    public static class Util
    {
        public static int CastNumToNum(string str)
        {
            if (str.Length != 3)
                return -1;
            char c0 = char.ToLowerInvariant(str[0]);
            if (c0 >= 'a' && c0 <= 'h' && str[1] >= '1' && str[1] <= '8' && str[2] >= '1' && str[2] <= '8')
                return (c0 - 'a') * 64 + (str[1] - '1') * 8 + (str[2] - '1') + 1;
            return -1;
        }

        public static string NumToCastNum(int num)
        {
            char[] res = { '?', '?', '?', '\0' };
            num -= 1;
            if (num >= 0 && num < 512)
            {
                int c = num / 64;
                res[0] = (char)('A' + c);
                num -= 64 * c;
                c = num / 8;
                res[1] = (char)('1' + c);
                num -= 8 * c;
                res[2] = (char)('1' + num);
            }
            return new string(res, 0, 3);
        }

        public static bool IsAbsolutePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            if (path.StartsWith("@/", StringComparison.Ordinal) || path.StartsWith("@\\", StringComparison.Ordinal))
                return true;

            if (path.Length >= 3 && char.IsLetter(path[0]) && path[1] == ':' && (path[2] == '/' || path[2] == '\\'))
                return true;

            return System.IO.Path.IsPathRooted(path);
        }

        public static bool IsPathWithRelativeMarkers(string path)
        {
            if (path.Contains("::"))
                return true;
            if (path.StartsWith(".\\") || path.EndsWith("\\.") || path.Contains("\\.\\"))
                return true;
            if (path.StartsWith("../") || path.StartsWith("..\\") || path.EndsWith("/..") || path.EndsWith("\\..") || path.Contains("/../") || path.Contains("\\..\\"))
                return true;
            return false;
        }

        public static string RectifyRelativePath(string path, string basePath)
        {
            var components = new List<string>(basePath.Split(System.IO.Path.DirectorySeparatorChar, System.StringSplitOptions.RemoveEmptyEntries));
            var tokens = path.Split(new[] { '/', '\\', ':' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var token in tokens)
            {
                if (token == "..")
                {
                    if (components.Count > 0)
                        components.RemoveAt(components.Count - 1);
                }
                else if (token != ".")
                {
                    components.Add(token);
                }
            }

            return "@:" + string.Join(System.IO.Path.DirectorySeparatorChar, components);
        }

        public static string ConvertPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            if (!path.Contains(':') && !path.Contains('\\') && !path.Contains('@'))
                return path;

            int idx = 0;
            if (path.StartsWith("::"))
                idx = 2;
            else if (path.StartsWith("@/") || path.StartsWith("@\\"))
                idx = 2;
            else if (path.Length >= 3 && char.IsLetter(path[0]) && path[1] == ':' && (path[2] == '/' || path[2] == '\\'))
                idx = 3;
            else if (path[0] == ':')
                idx = 1;

            var sb = new System.Text.StringBuilder();
            while (idx < path.Length)
            {
                char ch = path[idx];
                if (ch == ':' || ch == '\\')
                    sb.Append(System.IO.Path.DirectorySeparatorChar);
                else
                    sb.Append(ch);
                idx++;
            }
            return sb.ToString();
        }

        public static string UnixToMacPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            var res = new System.Text.StringBuilder(path.Length);
            foreach (char ch in path)
            {
                if (ch == ':')
                    res.Append('/');
                else if (ch == '/')
                    res.Append(':');
                else
                    res.Append(ch);
            }
            return res.ToString();
        }

        public static string GetPath(string path, string cwd)
        {
            int idx = path.LastIndexOf(System.IO.Path.DirectorySeparatorChar);
            if (idx >= 0)
                return path.Substring(0, idx + 1);
            return cwd;
        }

        public static bool HasExtension(string filename)
        {
            if (filename.Length < 4)
                return false;
            return filename[^4] == '.' && char.IsLetter(filename[^3]) && char.IsLetter(filename[^2]) && char.IsLetter(filename[^1]);
        }

        public static string GetFileName(string path)
        {
            int idx = path.LastIndexOf(System.IO.Path.DirectorySeparatorChar);
            if (idx >= 0)
                return path.Substring(idx + 1);
            return path;
        }
    }
}
