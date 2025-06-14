using System.Reflection;
using System.Text;

namespace Director.Fonts
{
    public static class FontMapDefaults
    {
        public static string GetFontMapText(int version)
        {
            string name = version switch
            {
                >= 1150 => "fontmap_D11_5.txt",
                >= 1100 => "fontmap_D11.txt",
                >= 1000 => "fontmap_D10.txt",
                >= 900 => "fontmap_D9.txt",
                >= 850 => "fontmap_D8_5.txt",
                >= 800 => "fontmap_D8.txt",
                >= 700 => "fontmap_D7.txt",
                _ => "fontmap_D6.txt",
            };

            string resource = $"Director.fontmaps.{name}";
            using Stream? s = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);
            if (s == null)
                throw new InvalidOperationException($"Missing embedded resource {resource}");
            using var reader = new StreamReader(s, Encoding.UTF8);
            return reader.ReadToEnd();
        }
    }
}
