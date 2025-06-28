using Godot;
using LingoEngine.Primitives;
using LingoEngine.Styles;

namespace LingoEngine.LGodot.Texts
{
    public static class LingoLabelExtensions
    {
        public static LabelSettings SetLingoFont(this LabelSettings labelSettings, ILingoFontManager fontManager, string fontName)
        {
            var font = fontManager.Get<FontFile>(fontName);
            if (font != null)
                labelSettings.Font = font;
            return labelSettings;
        }
        public static LabelSettings SetLingoFontSize(this LabelSettings labelSettings, int fontSize)
        {
            if (fontSize > 0)
                labelSettings.FontSize = fontSize;
            return labelSettings;
        }
        public static LabelSettings SetLingoColor(this LabelSettings labelSettings, LingoColor color)
        {
            labelSettings.FontColor = new Color(color.R, color.G, color.B);
            return labelSettings;
        }
        public static LingoColor GetLingoColor(this LabelSettings labelSettings)
            => new LingoColor(-1, (byte)labelSettings.FontColor.R, (byte)labelSettings.FontColor.G, (byte)labelSettings.FontColor.B);
    }
}
