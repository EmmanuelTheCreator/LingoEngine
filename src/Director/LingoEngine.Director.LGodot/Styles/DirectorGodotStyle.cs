using Godot;
using LingoEngine.Director.Core.Styles;
using LingoEngine.LGodot.Primitives;

namespace LingoEngine.Director.LGodot;

/// <summary>
/// Provides the default theme used by the Director UI.
/// Converted from directorTheme.tres.
/// </summary>
public sealed class DirectorGodotStyle
{
    public Theme Theme { get; }
    /// <summary>
    /// Highlight color for selected elements in the Director UI.
    /// </summary>
    public Color SelectedColor { get; } = Colors.DodgerBlue;

    public DirectorGodotStyle()
    {
        Theme = BuildTheme();
    }
    public static Font DefaultFont => GD.Load<Font>("res://Media/Fonts/ARIAL.TTF");
    private static Theme BuildTheme()
    {
        var theme = new Theme();
        theme.SetFont("font", "LineEdit", DefaultFont);
        theme.SetFontSize("font_size", "LineEdit", 11);

        // StyleBoxes for CloseButton
        var focus = new StyleBoxFlat { BgColor = new Color("#640808") };
        var hover = new StyleBoxFlat { BgColor = new Color("#640808") };
        var hoverPressedMirrored = new StyleBoxFlat { BgColor = new Color("#3c0303") };
        var normal = new StyleBoxFlat { BgColor = new Color("#aa1615") };
        var pressed = new StyleBoxFlat { BgColor = new Color("#3c0303") };

        theme.DefaultFontSize = 11;
        theme.SetTypeVariation("CloseButton", "Button");
        theme.SetStylebox("focus", "CloseButton", focus);
        theme.SetStylebox("hover", "CloseButton", hover);
        theme.SetStylebox("hover_pressed_mirrored", "CloseButton", hoverPressedMirrored);
        theme.SetStylebox("normal", "CloseButton", normal);
        theme.SetStylebox("pressed", "CloseButton", pressed);
        theme.SetColor("font_color", "CloseButton", Colors.White);

        // Default font size (not specific to a control type — fallback)
        theme.DefaultFontSize = 11;

        // Font sizes per control type
        theme.SetFontSize("font_size", "Button", 10);
        theme.SetFontSize("font_size", "CloseButton", 10); // already done
        theme.SetFontSize("font_size", "Label", 11);
        theme.SetFontSize("font_size", "LineEdit", 11);
        theme.SetFontSize("font_size", "TabBar", 10);
        theme.SetFontSize("font_size", "TextEdit", 11);
        theme.SetFontSize("font_size", "Tree", 11);
        theme.SetFontSize("title_button_font_size", "Tree", 12);


        var fontVariation = new FontVariation
        {
            BaseFont = DefaultFont,
        };

        // Define padding via content margins in a StyleBoxFlat
        var inputStyle = new StyleBoxFlat
        {
            BgColor = new Color("#FFFFFF"), // or keep default
            ContentMarginLeft = 1,
            ContentMarginRight = 1,
            ContentMarginTop = 0,
            ContentMarginBottom = 0,
            BorderWidthTop = 1,
            BorderWidthBottom = 1,
            BorderWidthLeft = 1,
            BorderWidthRight = 1,
            BorderColor = DirectorColors.InputBorder.ToGodotColor(),
        };

        // Apply to each input-like control
        foreach (var controlType in new[] { "LineEdit", "TextEdit", "SpinBox", "SearchBox" })
        {
            theme.SetStylebox("normal", controlType, inputStyle);
            theme.SetStylebox("focus", controlType, inputStyle); // optional
            theme.SetStylebox("hover", controlType, inputStyle); // optional
            theme.SetColor("font_color", controlType, DirectorColors.InputText.ToGodotColor());
            theme.SetFont("font", controlType, fontVariation);
            theme.SetFontSize("font_size", controlType, 11);
            theme.SetConstant("minimum_height", controlType, 11);

        }

        SetTabColors(theme);
        return theme;
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
    }
    //private static void SetTabColors(Theme theme)
    //{
    //    var fontVariation = new FontVariation
    //    {
    //        BaseFont = DefaultFont,
    //    };

    //    var bgWhite = DirectorColors.BG_WhiteMenus.ToGodotColor();
    //    var tabBgColor = DirectorColors.BG_Tabs.ToGodotColor();
    //    var borderColor = DirectorColors.Border_Tabs.ToGodotColor();

    //    // Unselected tabs
    //    var tabBg = new StyleBoxFlat
    //    {
    //        BgColor = tabBgColor,
    //        BorderColor = borderColor,
    //        BorderWidthBottom = 1
    //    };

    //    // Selected tab
    //    var tabFg = new StyleBoxFlat
    //    {
    //        BgColor = bgWhite,
    //        BorderColor = borderColor,
    //        BorderWidthTop = 1,
    //        BorderWidthLeft = 1,
    //        BorderWidthRight = 1,
    //        BorderWidthBottom = 0 // 👈 IMPORTANT: no bottom border so it blends with content
    //    };

    //    // Tab bar background
    //    var tabBarPanel = new StyleBoxFlat
    //    {
    //        BgColor = bgWhite,
    //        BorderWidthBottom = 1,
    //        BorderColor = borderColor
    //    };

    //    // Content area
    //    var contentPanel = new StyleBoxFlat
    //    {
    //        BgColor = bgWhite,
    //        BorderColor = borderColor,
    //        BorderWidthTop = 0,
    //        BorderWidthLeft = 1,
    //        BorderWidthRight = 1,
    //        BorderWidthBottom = 1
    //    };

    //    theme.SetStylebox("tab_bg", "TabBar", tabBg);
    //    theme.SetStylebox("tab_fg", "TabBar", tabFg);
    //    theme.SetStylebox("panel", "TabBar", tabBarPanel);
    //    theme.SetStylebox("panel", "TabContainer", contentPanel);

    //    theme.SetColor("font_color", "TabBar", Colors.Black);
    //    theme.SetColor("font_color_disabled", "TabBar", new Color(0.5f, 0.5f, 0.5f));
    //    theme.SetColor("font_color_focus", "TabBar", new Color(0.2f, 0.2f, 0.2f));

    //    theme.SetFont("font", "TabBar", fontVariation);
    //}
}
