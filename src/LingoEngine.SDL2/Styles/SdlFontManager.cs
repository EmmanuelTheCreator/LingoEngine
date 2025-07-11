using LingoEngine.Styles;

namespace LingoEngine.SDL2.Styles;

internal class SdlFontManager : ILingoFontManager
{
    private readonly List<(string Name, string FileName)> _fontsToLoad = new();
    private readonly Dictionary<string, object> _loadedFonts = new();
    public ILingoFontManager AddFont(string name, string pathAndName)
    {
        _fontsToLoad.Add((name, pathAndName));
        return this;
    }
    public void LoadAll()
    {
        foreach (var font in _fontsToLoad)
            _loadedFonts[font.Name] = font.FileName; // placeholder
        _fontsToLoad.Clear();
    }
    public T? Get<T>(string name) where T : class
        => _loadedFonts.TryGetValue(name, out var f) ? f as T : null;
    public object GetTyped(string name) => _loadedFonts[name];

    private object? _defaultFont;
    public T? GetDefaultFont<T>() where T : class
    {
        return _defaultFont as T;
    }

    public void SetDefaultFont<T>(T font) where T : class
    {
        _defaultFont = font;
    }
}
