using Godot;

namespace LingoEngine.Director.LGodot;

/// <summary>
/// Provides the default theme used by the Director UI.
/// Converted from directorTheme.tres.
/// </summary>
public sealed class DirectorStyle
{
    public Theme Theme { get; }

    public DirectorStyle()
    {
        Theme = BuildTheme();
    }

    private static Theme BuildTheme()
    {
        var theme = new Theme();

        var focus = new StyleBoxFlat { BgColor = new Color("#640808") };
        var hover = new StyleBoxFlat { BgColor = new Color("#640808") };
        var hoverPressedMirrored = new StyleBoxFlat { BgColor = new Color("#3c0303") };
        var normal = new StyleBoxFlat { BgColor = new Color("#aa1615") };
        var pressed = new StyleBoxFlat { BgColor = new Color("#3c0303") };

        theme.SetTypeVariation("CloseButton", "Button");
        theme.SetStylebox("focus", "CloseButton", focus);
        theme.SetStylebox("hover", "CloseButton", hover);
        theme.SetStylebox("hover_pressed_mirrored", "CloseButton", hoverPressedMirrored);
        theme.SetStylebox("normal", "CloseButton", normal);
        theme.SetStylebox("pressed", "CloseButton", pressed);
        theme.SetColor("font_color", "CloseButton", Colors.White);
        theme.SetFontSize("font_size", "Button", 10);
        theme.SetFontSize("font_size", "CloseButton", 10);
        theme.SetFontSize("font_size", "Label", 13);
        theme.SetFontSize("font_size", "LineEdit", 13);
        theme.SetFontSize("font_size", "TabBar", 10);
        theme.SetFontSize("font_size", "TextEdit", 13);
        theme.SetFontSize("font_size", "Tree", 11);
        theme.SetFontSize("title_button_font_size", "Tree", 12);

        return theme;
    }
}
