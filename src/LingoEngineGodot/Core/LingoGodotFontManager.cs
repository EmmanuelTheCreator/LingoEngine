using Godot;
using LingoEngine.FrameworkCommunication.Events;

namespace LingoEngineGodot
{
    internal class LingoGodotFontManager : ILingoFontManager
    {
        private readonly List<(string Name, string FileName)> _fonts = new();
        private readonly Dictionary<string, FontFile> _loadedFonts = new();
        public ILingoFontManager AddFont(string name, string pathAndName)
        {
            _fonts.Add((name, pathAndName));
            return this;
        }
        public void LoadAll()
        {
            foreach (var font in _fonts)
            {
                var fontFile = GD.Load<FontFile>(font.FileName);
                _loadedFonts.Add(font.Name, fontFile);
            }
        }
        public T Get<T>(string name) where T: class
             => (_loadedFonts[name] as T)!;
        public FontFile GetTyped(string name)
            => _loadedFonts[name];
    }
}
