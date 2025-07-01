using Godot;
using LingoEngine.Director.Core.Styles;
using LingoEngine.LGodot.Primitives;
using LingoEngine.LGodot.Styles;
using LingoEngine.Primitives;

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
        theme.SetColor("font_color", "Label", Colors.Black);
        SetCloseButtonStyle(theme);
        //SetInputStyles(theme);
        SetTabColors(theme);
        SetItemListStyle(theme);
        SetButtonStyle(theme);
        SetPopupWindowStyle(theme);
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

    public Theme GetTabContainerTheme()
    {
        var theme = new Theme();
        SetTabColors(theme);
        return theme;
    }
    public Theme GetTabItemTheme()
    {
        var theme = new Theme();
        // You could define styles for buttons inside the tabs if needed
        return theme;
    } 
    public Theme GetPopupWindowTheme()
    {
        return Theme;
    }
    private static void SetTabColors(Theme theme)
    {
        var fontVariation = new FontVariation
        {
            BaseFont = DefaultFont
        };

        // Style for inactive tab
        var tabUnselected = new StyleBoxFlat
        {
            BgColor = DirectorColors.BG_Tabs.ToGodotColor(),
            BorderColor = DirectorColors.BG_Tabs.ToGodotColor(),
            BorderWidthTop = 3,
            BorderWidthBottom = 0,
            BorderWidthLeft = 1,
            BorderWidthRight = 0,
            CornerRadiusBottomLeft = 0,
            CornerRadiusBottomRight = 0,
            CornerRadiusTopLeft = 0,
            CornerRadiusTopRight = 0,
            ContentMarginLeft = 5,
            ContentMarginRight = 5,
        };

        // Style for active (selected) tab
        var tabSelected = new StyleBoxFlat
        {
            BgColor = DirectorColors.BG_WhiteMenus.ToGodotColor(),
            BorderColor = DirectorColors.BG_Tabs.ToGodotColor(),
            //BorderColorBottom = DirectorColors.BG_WhiteMenus.ToGodotColor(),
            BorderWidthTop = 3,
            BorderWidthBottom = 0, // - 1, // active tab blends into content area
            BorderWidthLeft = 0,
            BorderWidthRight = 0,
            CornerRadiusBottomLeft = 0,
            CornerRadiusBottomRight = 0,
            CornerRadiusTopLeft = 4,
            CornerRadiusTopRight = 4,
            ContentMarginLeft = 5,
            ContentMarginRight = 5,
        };
        var tabHovered = (StyleBoxFlat)tabUnselected.Duplicate() as StyleBoxFlat;
        tabHovered.BgColor = DirectorColors.BG_Tabs_Hover.ToGodotColor();
        // Focus style (optional)
        var tabFocus = new StyleBoxFlat
        {
            BgColor = DirectorColors.BG_WhiteMenus.ToGodotColor(),
            BorderColor = Colors.DodgerBlue,
            BorderWidthTop = 3,
            BorderWidthBottom = 0, // active tab blends into content area
            BorderWidthLeft = 0,
            BorderWidthRight = 0,
            CornerRadiusBottomLeft = 0,
            CornerRadiusBottomRight = 0,
            CornerRadiusTopLeft = 4,
            CornerRadiusTopRight = 4,
            ContentMarginLeft = 5,
            ContentMarginRight = 5,
            ContentMarginTop = 3,
        };

        // Background behind all tabs
        var tabPanel = new StyleBoxFlat
        {
            BgColor = LingoColorList.Magenta.ToGodotColor(), //DirectorColors.BG_WhiteMenus.ToGodotColor()
        };

       
        // Background behind the tabs
        var tabBarBackground = new StyleBoxFlat
        {
            BgColor = DirectorColors.BG_Tabs.ToGodotColor(),
        };
        // Content panel of the TabContainer
        var contentPanel = new StyleBoxFlat
        {
            BgColor = DirectorColors.BG_WhiteMenus.ToGodotColor(),
            //BorderWidthTop = 1,
            //BorderColor = Colors.White
        };

        // Apply styles to TabContainer (correct control name for theming)
        theme.SetStylebox("tab_unselected", "TabContainer", tabUnselected);
        theme.SetColor("font_unselected_color", "TabContainer", DirectorColors.Tab_Deselected_TextColor.ToGodotColor());
        theme.SetStylebox("tab_selected", "TabContainer", tabSelected);
        theme.SetColor("font_selected_color", "TabContainer", DirectorColors.Tab_Selected_TextColor.ToGodotColor());
        theme.SetStylebox("tab_hovered", "TabContainer", tabHovered);
        theme.SetStylebox("tab_focus", "TabContainer", tabFocus);
        theme.SetStylebox("tabbar_background", "TabContainer", tabBarBackground);
        theme.SetStylebox("panel", "TabContainer", contentPanel);

        // Font and spacing
        theme.SetFont("font", "TabContainer", fontVariation);
        theme.SetFontSize("font_size", "TabContainer", 10);


        theme.SetConstant("h_separation", "TabContainer", 2);
        theme.SetConstant("side_margin", "TabContainer", 3);
        theme.SetConstant("top_margin ", "TabContainer", 0);
        theme.SetConstant("tab_min_height", "TabContainer", 18);
        theme.SetConstant("tab_max_width", "TabContainer", 120);


        //theme.SetStylebox("panel", "TabBar", tabBarPanel);
    }
    private static void SetItemListStyle(Theme theme)
    {
        var panelStyle = new StyleBoxFlat
        {
            BgColor = DirectorColors.Input_Bg.ToGodotColor(),
            BorderColor = DirectorColors.Input_Border.ToGodotColor(),
            BorderWidthTop = 1,
            BorderWidthBottom = 1, 
            BorderWidthLeft = 1,
            BorderWidthRight = 1,
            CornerRadiusBottomLeft = 2,
            CornerRadiusBottomRight = 2,
            CornerRadiusTopLeft = 2,
            CornerRadiusTopRight = 2,
            ContentMarginLeft = 5,
            ContentMarginRight = 5,
            ContentMarginTop = 5,
            ContentMarginBottom = 5,
        };

        theme.SetStylebox("panel", "ItemList", panelStyle);
        theme.SetColor("font_color", "ItemList", Colors.Black);
        theme.SetFontSize("font_size", "ItemList", 10);

        var selected = new StyleBoxFlat
        {
            BgColor = Colors.DodgerBlue,
            BorderColor = Colors.Black,
        }; 
        var hover = new StyleBoxFlat
        {
            BgColor = Colors.DarkGray,
            BorderColor = Colors.Black,
        };

        theme.SetStylebox("selected", "ItemList", selected);
        theme.SetStylebox("selected_focus", "ItemList", selected);
        theme.SetStylebox("hovered_selected_focus", "ItemList", selected);
        theme.SetStylebox("hovered", "ItemList", hover);
        theme.SetStylebox("hovered_selected", "ItemList", selected);
    }
    private static void SetButtonStyle(Theme theme)
    {
        var normal = new StyleBoxFlat
        {
            BgColor = DirectorColors.Button_Bg_Normal.ToGodotColor(),
            BorderColor = DirectorColors.Button_Border_Normal.ToGodotColor(),
            BorderWidthTop = 1,
            BorderWidthBottom = 1,
            BorderWidthLeft = 1,
            BorderWidthRight = 1,
            CornerRadiusTopLeft = 2,
            CornerRadiusTopRight = 2,
            CornerRadiusBottomLeft = 2,
            CornerRadiusBottomRight = 2
        };

        var hover = (StyleBoxFlat)normal.Duplicate();
        hover.BgColor = DirectorColors.Button_Bg_Hover.ToGodotColor();
        hover.BorderColor = DirectorColors.Button_Border_Hover.ToGodotColor();

        var pressed = (StyleBoxFlat)normal.Duplicate();
        pressed.BgColor = DirectorColors.Button_Bg_Pressed.ToGodotColor();
        pressed.BorderColor = DirectorColors.Button_Border_Pressed.ToGodotColor();

        var disabled = (StyleBoxFlat)normal.Duplicate();
        disabled.BgColor = DirectorColors.Button_Bg_Disabled.ToGodotColor();
        disabled.BorderColor = DirectorColors.Button_Border_Disabled.ToGodotColor();

        theme.SetStylebox("normal", "Button", normal);
        theme.SetStylebox("hover", "Button", hover);
        theme.SetStylebox("pressed", "Button", pressed);
        theme.SetStylebox("disabled", "Button", disabled);
        theme.SetColor("font_color", "Button", DirectorColors.Button_Text_Normal.ToGodotColor());
        theme.SetColor("font_disabled_color", "Button", DirectorColors.Button_Text_Disabled.ToGodotColor());
        theme.SetFont("font", "Button", new FontVariation { BaseFont = DefaultFont });
        theme.SetColor("font_hover_color", "Button", DirectorColors.Button_Text_Normal.ToGodotColor());
        theme.SetColor("font_focus_color", "Button", DirectorColors.Button_Text_Normal.ToGodotColor());
        theme.SetFontSize("font_size", "Button", 10);
    }
    private static void SetPopupWindowStyle(Theme theme)
    {
        //theme.SetStylebox("embedded_border", "Window", header);
        theme.SetColor("title_color", "Window", DirectorColors.PopupWindow_Header_Text.ToGodotColor());
        theme.SetColor("title_outline_modulate", "Window", DirectorColors.PopupWindow_Header_BG.ToGodotColor());
        theme.SetFont("title_font", "Window", new FontVariation { BaseFont = DefaultFont });
        theme.SetFontSize("font_size", "Window", 11);
        theme.SetFontSize("title_font_size", "Window", 12);
        theme.SetFontSize("title_height", "Window", 20);
    }
}
