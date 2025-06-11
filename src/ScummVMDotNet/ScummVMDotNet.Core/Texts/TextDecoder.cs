using Director.Primitives;
using System.Text;

namespace Director.Texts
{
    public static class TextDecoder
    {
        public static string Decode(string input, CodePage codePage)
        {
            Encoding encoding = codePage switch
            {
                CodePage.MacRoman => Encoding.GetEncoding("macintosh"),
                CodePage.WindowsLatin1 => Encoding.GetEncoding(1252),
                _ => Encoding.UTF8
            };

            // This assumes `input` is a byte string incorrectly interpreted as UTF-16
            byte[] rawBytes = Encoding.Default.GetBytes(input);
            return encoding.GetString(rawBytes);
        }
    }
}
