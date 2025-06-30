using Godot;

namespace LingoEngine.LGodot.Styles
{
    public enum LingoGodotThemeElementType
    {
        Tabs,
        TabItem
    }

    public interface ILingoGodotStyleManager
    {
        Theme? GetTheme(LingoGodotThemeElementType type);
        void Register(LingoGodotThemeElementType type, Theme theme);
    }

    internal class LingoGodotStyleManager : ILingoGodotStyleManager
    {
        private static readonly Dictionary<LingoGodotThemeElementType, Theme> _themes = new Dictionary<LingoGodotThemeElementType, Theme>();
        public Theme? GetTheme(LingoGodotThemeElementType type)
             => _themes.TryGetValue(type, out var theme) ? theme : null;

        public void Register(LingoGodotThemeElementType type, Theme theme)
        {
            if (_themes.ContainsKey(type))
            {
                _themes[type] = theme;
            }
            else
            {
                _themes.Add(type, theme);
            }
        }
    }
}
