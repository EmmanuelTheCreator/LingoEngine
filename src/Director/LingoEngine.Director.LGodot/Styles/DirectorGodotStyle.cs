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

    public static Theme GetTabContainerTheme()
    {
        var theme = new Theme();
        SetTabColors(theme);
        return theme;
    }
    public static Theme GetTabItemTheme()
    {
        var theme = new Theme();
        // You could define styles for buttons inside the tabs if needed
        return theme;
    }
    private static void SetTabColors(Theme theme)
    {
        var fontVariation = new FontVariation
        {
            BaseFont = DefaultFont
        };

        // Style for inactive tab
        var tabBg = new StyleBoxFlat
        {
            BgColor = DirectorColors.BG_Tabs.ToGodotColor(),
            BorderColor = DirectorColors.Border_Tabs.ToGodotColor(),
            BorderWidthTop = 1,
            BorderWidthBottom = 1,
            BorderWidthLeft = 1,
            BorderWidthRight = 1,
            CornerRadiusBottomLeft = 0,
            CornerRadiusBottomRight = 0,
        };

        // Style for active (selected) tab
        var tabFg = new StyleBoxFlat
        {
            BgColor = DirectorColors.BG_WhiteMenus.ToGodotColor(),
            BorderColor = DirectorColors.TabActiveBorder.ToGodotColor(),
            BorderWidthTop = 1,
            BorderWidthBottom = 0, // active tab blends into content area
            BorderWidthLeft = 1,
            BorderWidthRight = 1,
            CornerRadiusBottomLeft = 0,
            CornerRadiusBottomRight = 0,
        };

        // Background behind all tabs
        var tabPanel = new StyleBoxFlat
        {
            BgColor = DirectorColors.BG_WhiteMenus.ToGodotColor()
        };

        // Content panel of the TabContainer
        var contentPanel = new StyleBoxFlat
        {
            BgColor = DirectorColors.BG_WhiteMenus.ToGodotColor()
        };

        theme.SetStylebox("tab_bg", "TabBar", tabBg);        // Inactive tabs
        theme.SetStylebox("tab_fg", "TabBar", tabFg);        // Active tab
        theme.SetStylebox("panel", "TabBar", tabPanel);      // TabBar background
        theme.SetStylebox("panel", "TabContainer", contentPanel); // Main content background

        theme.SetFont("font", "TabBar", fontVariation);
        theme.SetFontSize("font_size", "TabBar", 10);

        theme.SetColor("font_color", "TabBar", DirectorColors.TextColorLabels.ToGodotColor());
        theme.SetColor("font_color_disabled", "TabBar", DirectorColors.TextColorDisabled.ToGodotColor());
        theme.SetColor("font_color_focus", "TabBar", DirectorColors.TextColorFocused.ToGodotColor());

        theme.SetStylebox("focus", "TabBar", new StyleBoxEmpty()); // Remove blue focus ring

        // Optional: tighter spacing
        theme.SetConstant("h_separation", "TabBar", 2);
        theme.SetConstant("side_margin", "TabBar", 2);
        theme.SetConstant("top_margin", "TabBar", 2);
        theme.SetConstant("tab_min_height", "TabBar", 18);

        theme.SetConstant("h_separation", "TabBar", 2);
        theme.SetConstant("side_margin", "TabBar", 2);
        theme.SetConstant("top_margin", "TabBar", 2);
        theme.SetConstant("tab_max_width", "TabBar", 120);
        theme.SetConstant("tab_min_height", "TabBar", 18);


        theme.SetFont("font", "TabBar", new FontVariation { BaseFont = DefaultFont });
        theme.SetFontSize("font_size", "TabBar", 10);

        var tabBarPanel = new StyleBoxFlat
        {
            BgColor = DirectorColors.BG_PropWindowBar.ToGodotColor(), // <- blueish bar!
        };
        theme.SetStylebox("panel", "TabBar", tabBarPanel);
    }


}
