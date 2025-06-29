using Godot;
using LingoEngine.Styles;

namespace LingoEngine.LGodot.Styles
{
    internal class LingoGodotFontManager : ILingoFontManager
    {
        private readonly List<(string Name, string FileName)> _fontsToLoad = new();
        private readonly Dictionary<string, FontFile> _loadedFonts = new();
        public LingoGodotFontManager()
        {
            
        }

        public ILingoFontManager AddFont(string name, string pathAndName)
        {
            _fontsToLoad.Add((name, pathAndName));
            return this;
        }
        public void LoadAll()
        {
            foreach (var font in _fontsToLoad)
            {
                var fontFile = GD.Load<FontFile>($"res://{font.FileName}");
                if (fontFile == null)
                    throw new Exception("Font file not found:" + font.Name + ":" + font.FileName);
                _loadedFonts.Add(font.Name, fontFile);
            }
            _fontsToLoad.Clear();
        }
        public T? Get<T>(string name) where T : class
             => _loadedFonts.TryGetValue(name, out var fontt) ? fontt as T : null;
        public FontFile GetTyped(string name)
            => _loadedFonts[name];

        private Font _defaultStyle = ThemeDB.FallbackFont;
        public T GetDefaultFont<T>() where T : class => (_defaultStyle as T)!;
        public void SetDefaultFont<T>(T font) where T : class => _defaultStyle = (font as Font)!;
    }
}
