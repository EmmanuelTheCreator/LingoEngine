using Godot;
using LingoEngine.FrameworkCommunication.Events;
using LingoEngine.Tools;

namespace LingoEngineGodot.Texts
{
    public static class LingoLabelExtensions
    {
        public static Label TryParseRtfFont(this Label label, string fileName, ILingoFontManager fontManager)
        {
            if (!File.Exists(fileName)) return label;
            var allRtf = File.ReadAllText(fileName);
            var rtfInfo = RtfExtracter.Parse(allRtf);
            if (rtfInfo == null) return label;
            var labelSettings = new LabelSettings
            {
                FontColor = new Color(rtfInfo.Color.R, rtfInfo.Color.G, rtfInfo.Color.B),
            };
            if (rtfInfo.Size > 0)
                labelSettings.FontSize = rtfInfo.Size;
            var font = fontManager.Get<FontFile>(rtfInfo.FontName);
            if (font != null)
                labelSettings.Font = fontManager.Get<FontFile>(rtfInfo.FontName);
            label.LabelSettings = labelSettings;
            return label;
        }
    }
}
