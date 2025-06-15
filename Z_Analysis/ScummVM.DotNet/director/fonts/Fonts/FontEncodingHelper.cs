using Director.Primitives;
using System.Text;

namespace Director.Fonts
{
    public static class FontEncodingHelper
    {
        public static Encoding DetectFontEncoding(Platform platform, int fontId)
        {
            // You can expand this mapping logic depending on actual Mac/Win font encoding behavior
            return platform switch
            {
                Platform.Macintosh => Encoding.GetEncoding("macintosh"),
                Platform.Windows => Encoding.GetEncoding("windows-1252"),
                _ => Encoding.UTF8,
            };
        }

        public static string ToPrintable(string text)
        {
            var sb = new StringBuilder();
            foreach (var c in text)
            {
                sb.Append(char.IsControl(c) ? $"\\x{(int)c:X2}" : c);
            }
            return sb.ToString();
        }

        public static string ToPrintable(byte[] data)
        {
            var sb = new StringBuilder();
            foreach (var b in data)
            {
                sb.Append(b < 32 || b > 126 ? $"\\x{b:X2}" : (char)b);
            }
            return sb.ToString();
        }
    }

   
}
