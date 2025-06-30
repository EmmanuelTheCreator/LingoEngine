using Godot;
using LingoEngine.Director.Core.Styles;
using LingoEngine.LGodot.Primitives;
using LingoEngine.LGodot.Styles;

namespace LingoEngine.Director.LGodot;

/// <summary>
/// Provides the default theme used by the Director UI.
/// Converted from directorTheme.tres.
/// </summary>
public sealed class DirectorGodotStyle
{
    private readonly LingoGodotStyle lingoGodotStyle;

    public Theme Theme { get; }
    /// <summary>
    /// Highlight color for selected elements in the Director UI.
    /// </summary>
    public Color SelectedColor { get; } = Colors.DodgerBlue;

    public DirectorGodotStyle(LingoGodotStyle lingoGodotStyle)
    {
        this.lingoGodotStyle = lingoGodotStyle;
        Theme = BuildTheme();
    }
    public static Font DefaultFont => GD.Load<Font>("res://Media/Fonts/ARIAL.TTF");
    private Theme BuildTheme()
    {
        var theme = lingoGodotStyle.Theme;

        // Default font size (not specific to a control type — fallback)
        theme.DefaultFontSize = 11;

        // Font sizes per control type
        theme.SetFontSize("font_size", "Button", 10);
        theme.SetFontSize("font_size", "Label", 11);
        theme.SetFontSize("font_size", "Tree", 11);
        theme.SetFontSize("title_button_font_size", "Tree", 12);

        SetCloseButtonStyle(theme);
        //SetInputStyles(theme);
        SetTabColors(theme);
        return theme;
    }

   
    private static void SetCloseButtonStyle(Theme theme)
    {
        // StyleBoxes for CloseButton
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
        theme.SetFontSize("font_size", "CloseButton", 10); // already done
    }

    private static void SetTabColors(Theme theme)
    {
        var fontVariation = new FontVariation
        {
            BaseFont = DefaultFont,
        };
        var tabBgStyle = new StyleBoxFlat
        {
            BgColor = DirectorColors.BG_WhiteMenus.ToGodotColor(),
        };
        var tabBg = new StyleBoxFlat { BgColor = DirectorColors.BG_Tabs.ToGodotColor() };  // unselected
        var tabFg = new StyleBoxFlat { BgColor = DirectorColors.BG_WhiteMenus.ToGodotColor(), };  // selected

        theme.SetStylebox("tab_bg", "TabBar", tabBg);
        theme.SetStylebox("tab_fg", "TabBar", tabFg);
        theme.SetStylebox("panel", "TabBar", tabBgStyle); // background behind tabs
        theme.SetColor("font_color", "TabBar", new Color(0, 0, 0));         // normal
        theme.SetColor("font_color_disabled", "TabBar", new Color(0.5f, 0.5f, 0.5f)); // disabled
        theme.SetColor("font_color_focus", "TabBar", new Color(0.2f, 0.2f, 0.2f)); // focused

        var contentBg = new StyleBoxFlat { BgColor = DirectorColors.BG_WhiteMenus.ToGodotColor(), };
        theme.SetStylebox("panel", "TabContainer", contentBg);
        theme.SetFont("font", "TabBar", fontVariation);
        theme.SetFontSize("font_size", "TabBar", 10);
    }
  
}
