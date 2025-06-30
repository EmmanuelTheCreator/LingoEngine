using Godot;

namespace LingoEngine.LGodot.Styles
{
    public class LingoGodotStyle
    {
        public Theme Theme { get; }
        public static Font DefaultFont => GD.Load<Font>("res://Media/Fonts/ARIAL.TTF");

        public LingoGodotStyle()
        {
            Theme = BuildTheme();
        }
        private static Theme BuildTheme()
        {
            var theme = new Theme();

            // Default font size (not specific to a control type — fallback)
            theme.DefaultFontSize = 11;

            // Font sizes per control type
            theme.SetFontSize("font_size", "Button", 10);
            theme.SetFontSize("font_size", "Label", 11);
            theme.SetFontSize("font_size", "Tree", 11);
            theme.SetFontSize("title_button_font_size", "Tree", 12);

            SetInputStyles(theme);
            return theme;
        }


        private static void SetInputStyles(Theme theme)
        {
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
                ContentMarginTop = 1,
                ContentMarginBottom = 1,
                BorderWidthTop = 1,
                BorderWidthBottom = 1,
                BorderWidthLeft = 1,
                BorderWidthRight = 1,
                BorderColor = new Color("#333333"),
            };

            // Apply to each input-like control
            foreach (var controlType in new[] { "LineEdit", "TextEdit", "SpinBox", "SearchBox" })
            {
                theme.SetStylebox("normal", controlType, inputStyle);
                theme.SetStylebox("focus", controlType, inputStyle); // optional
                theme.SetStylebox("hover", controlType, inputStyle); // optional
                theme.SetColor("font_color", controlType, new Color("#333333"));
                theme.SetFont("font", controlType, fontVariation);
                theme.SetFontSize("font_size", controlType, 11);
                theme.SetConstant("minimum_height", controlType, 10);
                theme.SetConstant("minimum_width", controlType, 5);
                theme.SetConstant("minimum_spaces", controlType, 1);
                theme.SetConstant("minimum_character_width", controlType, 0);
            }

        }

    }
}
